using System;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using Emotions.Services.Engine;

namespace Emotions.Services.KinectInput
{
    [Export(typeof(IKinectInputService))]
    class KinectInputService : IKinectInputService
    {
        private IEngineService _engine;
        private readonly ILog _log = LogManager.GetLog(typeof(KinectInputService));
        private KinectSensor _sensor;
        private FaceDetector _tracker;
        private KinectViewer _viewer;
        private Frame.Point3[] _points;

        public KinectInputService()
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
                    _log.Error(ex);
                }
                _engine = IoC.Get<IEngineService>();       
                _engine.Start();
            }
            else
            {
                _log.Warn("No kinect sensor detected");
                _log.Warn("Running without sensor");
                _engine = IoC.Get<IEngineService>(); 
            }
        }

        public void InitSensor()
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
                    _log.Warn("Failed to init near range sensor. Falling back to deafult range");
                    _sensor.DepthStream.Range = DepthRange.Default;
                    _sensor.SkeletonStream.EnableTrackingInNearRange = false;
                }

                _sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                _sensor.SkeletonStream.Enable();
                _log.Info("Sensor initialized");
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
                _log.Error(ex);
                throw;
            }
            
            _sensor.Start();
            _tracker = new FaceDetector(_sensor);
            _tracker.SkeletonTracked += TrackerOnSkeletonTracked;
            _tracker.SkeletonUnTracked += TrackerOnSkeletonUnTracked;
        }

        void TrackerOnSkeletonTracked(object sender, SkeletonTrackArgs args)
        {
            _log.Info("Skeleton tracked");

            if (_viewer != null)
                _viewer.TrackSkeleton(args.SkeletonFaceTracker);

            args.SkeletonFaceTracker.TrackSucceed += SkeletonFaceTrackerOnTrackSucceed;
        }

        void TrackerOnSkeletonUnTracked(object sender, SkeletonTrackArgs args)
        {
            _log.Info("Skeleton untracked");

            if (_viewer != null)
                _viewer.UnTrackSkeleton(args.SkeletonFaceTracker);

            args.SkeletonFaceTracker.TrackSucceed -= SkeletonFaceTrackerOnTrackSucceed;
        }

        /// <summary>
        /// Translates Kinect putput data to engine input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="faceFrame">Face frame</param>
        void SkeletonFaceTrackerOnTrackSucceed(object sender, FaceTrackFrame faceFrame, Skeleton skeleton)
        {
            var au = faceFrame.GetAnimationUnitCoefficients();
            var featurepoints = faceFrame.Get3DShape();
            var points = new Frame.Point3[featurepoints.Count];
            for (var i = 0; i < featurepoints.Count; i++)
            {
                points[i] = new Frame.Point3()
                {
                    X = featurepoints[i].X,
                    Y = featurepoints[i].Y,
                    Z = featurepoints[i].Z
                };
            }
            
            var frame = new Frame()
            {
                FeaturePoints = points,
                AU1 = au[0],
                AU2 = au[1],
                AU3 = au[2],
                AU4 = au[3],
                AU5 = au[4],
                AU6 = au[5],
                FacePosition = new Frame.Point3()
                {
                    X = faceFrame.Translation.X,
                    Y = faceFrame.Translation.Y,
                    Z = faceFrame.Translation.Z,
                },
                FaceRotation = new Frame.Point3()
                {
                    X = faceFrame.Rotation.X,
                    Y = faceFrame.Rotation.Y,
                    Z = faceFrame.Rotation.Z,
                },
            };
            _engine.ProceedInput(frame);
        }
        
        public void Dispose()
        {
            _engine.Stop();
            _tracker.Dispose();
            _sensor.Stop();
        }

        public void AttachViewer(KinectViewer viewer)
        {
            _log.Info("Viewer attached");
            _viewer = viewer;
            _viewer.Kinect = _sensor;
        }
    }
}
