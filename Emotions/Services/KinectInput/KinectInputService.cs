using System;
using System.ComponentModel.Composition;
using System.IO;
using Caliburn.Micro;
using Emotions.KinectTools;
using Emotions.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using Emotions.Services.Engine;

namespace Emotions.Services.KinectInput
{
    [Export(typeof(IEditorProvider))]
    [Export(typeof(IKinectInputService))]
    class KinectInputService : IKinectInputService, IEditorProvider
    {
        private IEngineService _engine;
        private readonly ILog _log = LogManager.GetLog(typeof(KinectInputService));
        private SkeletonTracker _tracker;
        private KinectViewer _viewer;
        private IKinectSource _kinectSource;

        public KinectInputService()
        {
            _engine = IoC.Get<IEngineService>();
            SourceChanged += OnSourceChanged;
            LoadRealKinect();
        }

        public void LoadRealKinect()
        {
            try
            {
                var source = new RealKinectSource();
                _log.Info("Sensor initialized");
                ActiveSource = source;
            }
            catch (Exception ex)
            {
                _log.Warn("Failed to initialize kinect");
                _log.Error(ex);
                _log.Warn("Running without sensor");
            }
        }

        private void OnSourceChanged(object sender, SourceChangedArgs e)
        {
            if (e.OldSource != null)
            {
                e.OldSource.Stop();
            }

            if (e.NewSource != null)
            {
                _log.Info("New kinect source: {0}", e.NewSource);
                _engine.Start();

                if (_tracker != null)
                {
                    _tracker.SkeletonTracked -= TrackerOnSkeletonTracked;
                    _tracker.SkeletonUnTracked -= TrackerOnSkeletonUnTracked;
                    _tracker.Dispose();
                }

                _tracker = new SkeletonTracker(_kinectSource);
                _tracker.SkeletonTracked += TrackerOnSkeletonTracked;
                _tracker.SkeletonUnTracked += TrackerOnSkeletonUnTracked;

                _viewer.Kinect = _kinectSource;
            }
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
                LipRaiser = au[AnimationUnit.LipRaiser],
                JawLowerer = au[AnimationUnit.JawLower],
                LipStretcher = au[AnimationUnit.LipStretcher],
                BrowLowerer = au[AnimationUnit.BrowLower],
                LipCornerDepressor = au[AnimationUnit.LipCornerDepressor],
                BrowRaiser = au[AnimationUnit.BrowRaiser],
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

        public event EventHandler<SourceChangedArgs> SourceChanged;

        public IKinectSource ActiveSource
        {
            get { return _kinectSource; }
            private set
            {
                var old = _kinectSource;
                _kinectSource = value;
                
                if (SourceChanged != null)
                    SourceChanged(this, new SourceChangedArgs(old, value));
            }
        }

        public void Dispose()
        {
            _engine.Stop();
            _tracker.Dispose();
            _kinectSource.Stop();
        }

        public void AttachViewer(KinectViewer viewer)
        {
            _log.Info("Viewer attached");
            _viewer = viewer;
            _viewer.Kinect = ActiveSource;
        }

        public void LoadRecording(string path)
        {
            try
            {
                _log.Info("Loading recording {0}", path);
                var source = new KinectPlayer(path);
                source.PlaybackEnded += SourceOnPlaybackEnded;
                source.Start();
                ActiveSource = source;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        private void SourceOnPlaybackEnded()
        {
            _log.Info("Playback ended");
            _viewer.Dispatcher.Invoke(LoadRealKinect);
        }

        public bool Handles(string path)
        {
            return true;
        }

        public IDocument Create(string path)
        {
            LoadRecording(path);
            //return new KinectOutputViewModel();
            return null;
        }
    }
}
