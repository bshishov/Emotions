using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace Emotions
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<AUCoeffDesc> AUCoeffs { get; set; }

        private KinectSensor _sensor;
        private FaceDetector _tracker;

        public MainWindow()
        {
            InitializeComponent();
            AUCoeffs = new ObservableCollection<AUCoeffDesc>();
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
        }

        private void TrackerOnSkeletonTracked(object sender, SkeletonTrackArgs args)
        {
            Viewer.TrackSkeleton(args.SkeletonFaceTracker);
            args.SkeletonFaceTracker.TrackSucceed += SkeletonFaceTrackerOnTrackSucceed;
        }

        private void SkeletonFaceTrackerOnTrackSucceed(object sender, FaceTrackFrame frame)
        {
            var au = frame.GetAnimationUnitCoefficients();
            AUCoeffs.Clear();
            for (var i = 0; i < au.Count; i++)
            {
                AUCoeffs.Add(new AUCoeffDesc()
                {
                    Name = String.Format("AU {0} {1}", i, Enum.GetName(typeof(AnimationUnit), i)),
                    Value = au[i]
                });    
            }
            NotifyPropertyChanged("AUCoeffs");
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
    }

    public class AUCoeffDesc
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }
}
