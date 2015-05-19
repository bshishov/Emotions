using System;
using Emotions.KinectTools.Frames;
using Microsoft.Kinect;
using SkeletonFrame = Microsoft.Kinect.SkeletonFrame;

namespace Emotions.KinectTools.Sources
{
    public class RealKinectSource : IKinectSource
    {
        private byte[] _colorImage;
        private ColorImageFormat _colorImageFormat = ColorImageFormat.Undefined;
        private short[] _depthImage;
        private DepthImageFormat _depthImageFormat = DepthImageFormat.Undefined;

        private Skeleton[] _skeletonData;
        private readonly KinectSensor _sensor;

        public event Action<IKinectSource> Started;
        public event Action<IKinectSource> Stopped;
        public string Name { get { return "Kinect Sensor"; } }
        public bool IsActive { get { return _sensor.IsRunning; } }
        public event Action<IKinectSource, FramesContainer> FramesReady;
        

        public KinectSourceInfo Info { get; private set; }

        public void Start()
        {
            if(_sensor == null)
                throw new Exception("No sensor");

            if (!_sensor.IsRunning)
            {
                _sensor.Start();
                if (Started != null)
                    Started(this);
            }
        }

        public void Stop()
        {
            if (_sensor == null)
                throw new Exception("No sensor");
            
            _sensor.Stop();
            if (Stopped != null)
                Stopped(this);
        }

        public RealKinectSource()
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                _sensor = KinectSensor.KinectSensors[0];
                try
                {
                    InitSensor();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
                throw new Exception("No kinect sensors detected");
            
            Info = new KinectSourceInfo(_sensor);
            _sensor.AllFramesReady += SensorOnAllFramesReady;
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
            catch (InvalidOperationException ex)
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
                throw;
            }

            _sensor.Start();
        }

        private void SensorOnAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (FramesReady == null) return;

            ColorImageFrame colorImageFrame = null;
            DepthImageFrame depthImageFrame = null;
            SkeletonFrame skeletonFrame = null;
            
            try
            {
                colorImageFrame = e.OpenColorImageFrame();
                depthImageFrame = e.OpenDepthImageFrame();
                skeletonFrame = e.OpenSkeletonFrame();

                if (colorImageFrame == null || depthImageFrame == null || skeletonFrame == null)
                {
                    return;
                }

                // Check for image format changes.  The FaceTracker doesn't
                // deal with that so we need to reset.
                if (_depthImageFormat != depthImageFrame.Format)
                {
                    _depthImage = null;
                    _depthImageFormat = depthImageFrame.Format;
                }

                if (_colorImageFormat != colorImageFrame.Format)
                {
                    _colorImage = null;
                    _colorImageFormat = colorImageFrame.Format;
                }

                // Create any buffers to store copies of the data we work with
                if (_depthImage == null)
                    _depthImage = new short[depthImageFrame.PixelDataLength];

                if (_colorImage == null)
                    _colorImage = new byte[colorImageFrame.PixelDataLength];

                // Get the skeleton information
                if (_skeletonData == null || _skeletonData.Length != skeletonFrame.SkeletonArrayLength)
                    _skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];

                FramesReady.Invoke(this, new FramesContainer(
                    new ColorFrame(colorImageFrame), 
                    new DepthFrame(depthImageFrame), 
                    new Frames.SkeletonFrame(skeletonFrame)));
            }
            finally
            {
                if (colorImageFrame != null)
                    colorImageFrame.Dispose();

                if (depthImageFrame != null)
                    depthImageFrame.Dispose();

                if (skeletonFrame != null)
                    skeletonFrame.Dispose();
            }
        }

        public void Dispose()
        {
            if (_sensor != null)
            {
                _sensor.AllFramesReady -= SensorOnAllFramesReady;
                _sensor.Stop();
                _sensor.Dispose();
            }
        }
    }
}