using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

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

        private int _frameNum = 0;
        private int _frameSkip = 15;
        private EmotionRecognizer _recognizer;

        private void SkeletonFaceTrackerOnTrackSucceed(object sender, FaceTrackFrame frame)
        {
            _lastFrame = frame;
            if(!RTAUTracking)
                return;
            if (_frameNum++ < _frameSkip)
                return;
            
            _frameNum = 0;
            
            var au = frame.GetAnimationUnitCoefficients();
            AU1.Value = au[0];
            AU2.Value = au[1];
            AU3.Value = au[2];
            AU4.Value = au[3];
            AU5.Value = au[4];
            AU6.Value = au[5];

            PosXLabel.Content = frame.Translation.X;
            PosYLabel.Content = frame.Translation.Y;
            PosZLabel.Content = frame.Translation.Z;

            RotXLabel.Content = frame.Rotation.X;
            RotYLabel.Content = frame.Rotation.Y;
            RotZLabel.Content = frame.Rotation.Z;

            if (_recognizer != null)
            {
                var ausDouble = new double[] {au[0], au[1], au[2], au[3], au[4], au[5]};
                NeutralEmotion.Value =  _recognizer.Compute(EmotionType.Neutral, ausDouble);
                JoyEmotion.Value =      _recognizer.Compute(EmotionType.Joy, ausDouble);
                SurpriseEmotion.Value = _recognizer.Compute(EmotionType.Surprise, ausDouble);
                AngerEmotion.Value =    _recognizer.Compute(EmotionType.Anger, ausDouble);
                FearEmotion.Value =     _recognizer.Compute(EmotionType.Fear, ausDouble);
                SadnessEmotion.Value =  _recognizer.Compute(EmotionType.Sadness, ausDouble);
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
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
            _recognizer = new EmotionRecognizer(_trainingData);
        }
    }

    public class AUCoeffDesc
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }
}
