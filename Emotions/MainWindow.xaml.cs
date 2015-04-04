using System;
using System.Windows;
using System.Windows.Data;
using Microsoft.Kinect;

namespace Emotions
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor _sensor;

        public MainWindow()
        {
            InitializeComponent();
            //var kinectViewerBinding = new Binding("Kinect") {Source = sensorChooser};

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
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            _sensor.Stop();
        }
    }
}
