using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Emotions.KinectTools.Sources;
using Emotions.KinectTools.Tracking;
using Emotions.Services.Engine;
using Emotions.Services.Recording;
using Gemini.Framework;
using Gemini.Framework.Results;
using Gemini.Modules.Shell.Views;

namespace Emotions.ViewModels
{
    [Export(typeof(KinectOutputViewModel))]
    class KinectOutputViewModel : Document, IDisposable
    {
        private IEngineService _engine;
        private IKinectSource _currentSource;
        private Recorder _recorder;
        private readonly ILog _log = LogManager.GetLog(typeof(KinectOutputViewModel));

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
            }
        }
        
        public SkeletonFaceTracker SkeletonFaceTracker
        {
            get { return CurrentSource.SkeletonFaceTracker; }
        }
        
        public bool IsEngineEnabled
        {
            get
            {
                return _engine != null && _engine.ActiveSource == CurrentSource;
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

            _engine.Bind(CurrentSource);
        }

        private void UnbindEngine()
        {
            if (_engine == null)
                return;
            _engine.Unbind();
        }
        
        public void StartRecording(GameViewModel gameVm = null)
        {
            if (_currentSource != null)
            {
                var path = string.Format("kinect{0}.rec", DateTime.Now.ToString("yyyyMMddHHmmss"));
                _recorder = new Recorder(_currentSource, gameVm, path);
                _recorder.Start();
                _log.Info("Recording started: {0}", path);
            }
            NotifyOfPropertyChange(() => IsRecording);
        }

        public void StopRecording()
        {
            if (_recorder != null)
            {
                _recorder.Stop();
                _recorder.Dispose();
                _recorder = null;
                _log.Info("Recording finished");
            }
            NotifyOfPropertyChange(() => IsRecording);
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

            if (_currentSource != null)
            {
                _currentSource.Stop();
                _currentSource.Dispose();
            }
        }
    }
}
