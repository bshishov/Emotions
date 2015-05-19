using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Emotions.KinectTools;
using Emotions.KinectTools.Recorders;
using Emotions.KinectTools.Sources;
using Emotions.KinectTools.Tracking;
using Emotions.Services.Engine;
using Gemini.Framework;

namespace Emotions.ViewModels
{
    [Export(typeof(KinectOutputViewModel))]
    class KinectOutputViewModel : Document, IDisposable
    {
        private IEngineService _engine;
        private IKinectSource _currentSource;
        private Recorder _recorder;
        private readonly ILog _log = LogManager.GetLog(typeof(KinectOutputViewModel));
        private SkeletonTracker _skeletonTracker;
        private SkeletonFaceTracker _skeletonFaceTracker;

        public IKinectSource CurrentSource
        {
            get { return _currentSource; }
            set
            {
                _currentSource = value;
                NotifyOfPropertyChange(() => CurrentSource);
                NotifyOfPropertyChange(() => IsRunning);
            }
        }
        public bool IsRunning
        {
            get
            {
                if (_currentSource != null)
                    return _currentSource.IsActive;
                return false;
            }
            set
            {
                if (_currentSource != null)
                {
                    if(!_currentSource.IsActive)
                        _currentSource.Start();
                }
                NotifyOfPropertyChange(() => IsRunning);
            }
        }
        
        public bool IsRecording
        {
            get { return _recorder != null; }
            set
            {
                if (value)
                    StartRecording();
                else
                    StopRecording();
                NotifyOfPropertyChange(() => IsRecording);
            }
        }

        public bool IsTrackingEnabled
        {
            get { return _skeletonTracker != null; }
            set
            {
                if(value)
                    StartTracking();
                else
                    StopTracking();
                NotifyOfPropertyChange(() => IsTrackingEnabled);
            }
        }
        
        public SkeletonFaceTracker SkeletonFaceTracker
        {
            get { return _skeletonFaceTracker; }
            set
            {
                _skeletonFaceTracker = value;
                NotifyOfPropertyChange(() => SkeletonFaceTracker);
            }
        }
        
        public bool IsEngineEnabled
        {
            get
            {
                if (_engine != null && _skeletonFaceTracker != null)
                    return _engine.ActiveTracker == _skeletonFaceTracker;
                return false;
            }
            set
            {
                if (value)
                    BindEngine();
                else
                    UnbindEngine();
                NotifyOfPropertyChange(() => IsEngineEnabled);
            }
        }

        private void BindEngine()
        {
            if (_engine == null)
            {
                _log.Warn("Can't bind engine, engine is not running");
                return;
            }

            if (_skeletonFaceTracker == null)
            {
                _log.Warn("Can't bind engine, face is not tracked");
                return;
            }

            _engine.Bind(_skeletonFaceTracker);
        }

        private void UnbindEngine()
        {
            if (_engine == null)
                return;
            _engine.Unbind();
        }

        private void StartTracking()
        {
            _log.Info("Starting skeleton tracking");
            _skeletonTracker = new SkeletonTracker(_currentSource);
            _skeletonTracker.SkeletonTracked += OnSkeletonTracked;
            _skeletonTracker.SkeletonUnTracked += OnSkeletonUnTracked;
        }

        private void OnSkeletonTracked(object sender, SkeletonTrackArgs args)
        {
            _log.Info("Skeleton tracked");
            SkeletonFaceTracker = args.SkeletonFaceTracker;
        }

        private void OnSkeletonUnTracked(object sender, SkeletonTrackArgs args)
        {
            _log.Info("Skeleton untracked");
            SkeletonFaceTracker.Dispose();
            SkeletonFaceTracker = null;
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
            _log.Info("Stopped skeleton tracking");

            UnbindEngine();
        }
        
        private void StartRecording()
        {
            if (_currentSource != null)
            {
                var path = string.Format("kinect{0}.rec", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                _recorder = new Recorder(_currentSource, path);
                _recorder.Start();
                _log.Info("Recording started: {0}", path);
            }
        }

        private void StopRecording()
        {
            if (_recorder != null)
            {
                _recorder.Stop();
                _recorder.Dispose();
                _recorder = null;
                _log.Info("Recording finished");
            }
        }
        
        public KinectOutputViewModel(IKinectSource source)
        {
            DisplayName = source.Name;
            CurrentSource = source;
            source.Started += SourceOnStarted;
            source.Stopped += SourceOnStopped;
        }

        private void SourceOnStopped(IKinectSource obj)
        {
            NotifyOfPropertyChange(() => IsRunning);
            _log.Info("Source stopped");
        }

        private void SourceOnStarted(IKinectSource kinectSource)
        {
            NotifyOfPropertyChange(() => IsRunning);
            _log.Info("Source started");
        }

        public void OnViewerInitialized(object sender, object context)
        {   
            _log.Info("Kinect viewer initialized");
            _engine = IoC.Get<IEngineService>();
        }

        public void Stop()
        {
            if(IsRecording)
                StopRecording();
            _currentSource.Stop();
            NotifyOfPropertyChange(() => IsRecording);
        }

        public void Pause()
        {
            if (IsRecording)
                StopRecording();
            NotifyOfPropertyChange(() => IsRecording);
        }

        public void Dispose()
        {
            if(IsRecording)
                StopRecording();
            if (IsTrackingEnabled)
                StopTracking();

            if (_currentSource != null)
            {
                _currentSource.Stop();
                _currentSource.Dispose();
            }
        }
        
       
    }
}
