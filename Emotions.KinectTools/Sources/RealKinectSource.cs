using System;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Tracking;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using SkeletonFrame = Microsoft.Kinect.SkeletonFrame;

namespace Emotions.KinectTools.Sources
{
    public class RealKinectSource : IKinectSource
    {
        private readonly KinectSensor _sensor;
        
        private byte[] _colorImage;
        private short[] _depthImage;
        private Skeleton[] _skeletonData;

        private ColorImageFormat _colorImageFormat = ColorImageFormat.Undefined;
        private DepthImageFormat _depthImageFormat = DepthImageFormat.Undefined;
        private SkeletonTracker _skeletonTracker;

        public event Action<IKinectSource, FramesContainer> FramesReady;
        public event Action<IKinectSource, EngineFrame> EngineFrameReady;
        public event Action<IKinectSource> Started;
        public event Action<IKinectSource> Stopped;

        public string Name { get { return "Kinect Sensor"; } }

        public bool IsActive { get { return _sensor.IsRunning; } }

        public KinectSourceInfo Info { get; private set; }

        public SkeletonFaceTracker SkeletonFaceTracker { get; private set; }
        
        public RealKinectSource()
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                _sensor = KinectSensor.KinectSensors[0];
                try
                {
                    InitSensor();
                    Start();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
                throw new Exception("No kinect sensors detected");
            Info = new KinectSourceInfo(_sensor);
        }

        public void Start()
        {
            if(_sensor == null)
                throw new Exception("No sensor");

            if (!_sensor.IsRunning)
            {
                _sensor.Start();
                _sensor.AllFramesReady += SensorOnAllFramesReady;
                if (Started != null)
                    Started(this);
                StartTracking();
            }
        }

        public void Stop()
        {
            if (_sensor == null)
                throw new Exception("No sensor");
            
            _sensor.Stop();
            StopTracking();
            if (Stopped != null)
                Stopped(this);
        }

        private void StartTracking()
        {
            _skeletonTracker = new SkeletonTracker(this);
            _skeletonTracker.SkeletonTracked += OnSkeletonTracked;
            _skeletonTracker.SkeletonUnTracked += OnSkeletonUnTracked;
        }

        private void StopTracking()
        {
            if (SkeletonFaceTracker != null)
            {
                SkeletonFaceTracker.Dispose();
                SkeletonFaceTracker = null;
            }

            _skeletonTracker.Dispose();
            _skeletonTracker.SkeletonTracked -= OnSkeletonTracked;
            _skeletonTracker.SkeletonUnTracked -= OnSkeletonUnTracked;
            _skeletonTracker = null;
        }

        private void OnSkeletonTracked(object sender, SkeletonTrackArgs args)
        {
            SkeletonFaceTracker = args.SkeletonFaceTracker;
            SkeletonFaceTracker.TrackSucceed += SkeletonFaceTrackerOnTrackSucceed;
        }
       
        private void OnSkeletonUnTracked(object sender, SkeletonTrackArgs args)
        {
            if (SkeletonFaceTracker != null)
            {
                SkeletonFaceTracker.Dispose();
                SkeletonFaceTracker = null;
            }
        }

        private void SkeletonFaceTrackerOnTrackSucceed(object sender, FaceTrackFrame frame, Skeleton skeleton)
        {
            if(EngineFrameReady != null)   
                EngineFrameReady.Invoke(this, EngineFrame.FromFaceTrackFrame(frame, skeleton));
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