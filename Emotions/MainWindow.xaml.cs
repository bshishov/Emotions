using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Emotions.Services.Engine;
using Emotions.Services.KinectInput;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using Frame = Emotions.Services.Engine.Frame;

namespace Emotions
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private KinectSensor _sensor;
        private FaceDetector _tracker;
        private TrainingDataSet _trainingData;
        private Engine _engine;
        private FaceTrackFrame _lastFrame;
        
        public ComboBoxItem SelectedEmotion { get; set; }
        public string TrainingDBFile{ get; set; }
        public string TrainingComment { get; set; }
        public bool RTAUTracking { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            if (KinectSensor.KinectSensors.Count > 0)
            {
                _sensor = KinectSensor.KinectSensors[0];
                InitSensor();
            }
            else
            {
                throw new Exception("No kinect sensor detected");
            }


            AU1.Caption = String.Format("AU 1 {0}", Enum.GetName(typeof(AnimationUnit), 0));
            AU2.Caption = String.Format("AU 2 {0}", Enum.GetName(typeof(AnimationUnit), 1));
            AU3.Caption = String.Format("AU 3 {0}", Enum.GetName(typeof(AnimationUnit), 2));
            AU4.Caption = String.Format("AU 4 {0}", Enum.GetName(typeof(AnimationUnit), 3));
            AU5.Caption = String.Format("AU 5 {0}", Enum.GetName(typeof(AnimationUnit), 4));
            AU6.Caption = String.Format("AU 6 {0}", Enum.GetName(typeof(AnimationUnit), 5));

            TrainingDBFile = "database.csv";
            TrainingComment = "";
            SelectedEmotion = new ComboBoxItem() {Content = "Neutral"};
            NotifyPropertyChanged("SelectedEmotion");
            NotifyPropertyChanged("TrainingDBFile");
            NotifyPropertyChanged("TrainingComment");

            _trainingData = TrainingDataSet.FromFile(TrainingDBFile);
        }

        private void InitSensor()
        {
            try
            {
                _sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                _sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                try
                {
                    // This will throw on non Kinect For Windows devices.
                    _sensor.DepthStream.Range = DepthRange.Near;
                    _sensor.SkeletonStream.EnableTrackingInNearRange = true;
                }
                catch (InvalidOperationException)
                {
                    _sensor.DepthStream.Range = DepthRange.Default;
                    _sensor.SkeletonStream.EnableTrackingInNearRange = false;
                }

                _sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                _sensor.SkeletonStream.Enable();
            }
            catch (InvalidOperationException)
            {
                // This exception can be thrown when we are trying to
                // enable streams on a device that has gone away.  This
                // can occur, say, in app shutdown scenarios when the sensor
                // goes away between the time it changed status and the
                // time we get the sensor changed notification.
                //
                // Behavior here is to just eat the exception and assume
                // another notification will come along if a sensor
                // comes back.
            }

            Viewer.Kinect = _sensor;
            _sensor.Start();
            _tracker = new FaceDetector(_sensor);
            _tracker.SkeletonTracked += TrackerOnSkeletonTracked;
            _tracker.SkeletonUnTracked += TrackerOnSkeletonUnTracked;

            _engine = new Engine();
            _engine.OnUpdate += EngineOnOnUpdate;
            _engine.Start();
        }
        private void EngineOnOnUpdate(object sender, EngineUpdateEventArgs args)
        {
            this.Dispatcher.Invoke(new UpdateUICallback(UpdateUI), args.Frame);
        }

        public delegate void UpdateUICallback(Frame buffer);
        private void UpdateUI(Frame buffer)
        {
            AU1.Value = buffer.AU1;
            AU2.Value = buffer.AU2;
            AU3.Value = buffer.AU3;
            AU4.Value = buffer.AU4;
            AU5.Value = buffer.AU5;
            AU6.Value = buffer.AU6;

            PosXLabel.Content = buffer.FacePosition.X;
            PosYLabel.Content = buffer.FacePosition.Y;
            PosZLabel.Content = buffer.FacePosition.Z;

            RotXLabel.Content = buffer.FaceRotation.X;
            RotYLabel.Content = buffer.FaceRotation.Y;
            RotZLabel.Content = buffer.FaceRotation.Z;
        }

        private void TrackerOnSkeletonUnTracked(object sender, SkeletonTrackArgs args)
        {
            Viewer.UnTrackSkeleton(args.SkeletonFaceTracker);
            args.SkeletonFaceTracker.TrackSucceed -= SkeletonFaceTrackerOnTrackSucceed;
        }

        private void TrackerOnSkeletonTracked(object sender, SkeletonTrackArgs args)
        {
            Viewer.TrackSkeleton(args.SkeletonFaceTracker);
            args.SkeletonFaceTracker.TrackSucceed += SkeletonFaceTrackerOnTrackSucceed;
        }
        
        private void SkeletonFaceTrackerOnTrackSucceed(object sender, FaceTrackFrame frame)
        {
            _lastFrame = frame;
            if(!RTAUTracking)
                return;
            
            var au = frame.GetAnimationUnitCoefficients();
            var snapshot = new Frame()
            {
                AU1 = au[0],
                AU2 = au[1],
                AU3 = au[2],
                AU4 = au[3],
                AU5 = au[4],
                AU6 = au[5],
                FacePosition = new Frame.Point3()
                {
                    X = frame.Translation.X,
                    Y = frame.Translation.Y,
                    Z = frame.Translation.Z,
                },
                FaceRotation = new Frame.Point3()
                {
                    X = frame.Rotation.X,
                    Y = frame.Rotation.Y,
                    Z = frame.Rotation.Z,
                },
            };
            _engine.ProceedInput(snapshot);
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            _engine.Stop();
            _tracker.Dispose();
            _sensor.Stop();
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void NotifyPropertyChanged(
            string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnTrainingRecordSave(object sender, RoutedEventArgs e)
        {
            if(_lastFrame == null)
                return;

            var t = (EmotionType) System.Enum.Parse(typeof (EmotionType), SelectedEmotion.Content.ToString());
            _trainingData.Rows.Add(new TrainingDataSet.TrainingDataRow(t, TrainingComment, _lastFrame));
            _trainingData.ToFile(TrainingDBFile);
        }

        private void OnTrain(object sender, RoutedEventArgs e)
        {
            /*
            _recognizer = new EmotionRecognizer(_trainingData);*/
        }
    }
}
