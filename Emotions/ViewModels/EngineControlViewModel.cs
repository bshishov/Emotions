using System;
using System.ComponentModel.Composition;
using Emotions.KinectTools;
using Emotions.Services.Engine;
using Emotions.Services.KinectInput;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Emotions.ViewModels
{
    [Export(typeof(EngineControlViewModel))]
    class EngineControlViewModel : Tool
    {
        [Import] private IKinectInputService _inputService;
        private EngineState _state;
        private Recorder _recorder;

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Left; }
        }

        public EngineControlViewModel()
        {
            DisplayName = "Engine control panel";
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            _inputService.SourceChanged += (s, e) => UpdateState();
        }

        private void UpdateState()
        {
            NotifyOfPropertyChange(() => RTEnabled);
            NotifyOfPropertyChange(() => RecEnabled);
            NotifyOfPropertyChange(() => PlayEnabled);
            NotifyOfPropertyChange(() => PauseEnabled);
            NotifyOfPropertyChange(() => RTChecked);
            NotifyOfPropertyChange(() => PlayChecked);
            NotifyOfPropertyChange(() => RecChecked);
            NotifyOfPropertyChange(() => PauseChecked);
        }

        public bool RTChecked
        {
            get { return _inputService.ActiveSource is RealKinectSource; }
            set
            {
                //if (_engine != null)
                    //_engine.Start();
                _inputService.LoadRealKinect();
                NotifyOfPropertyChange(() => RTChecked);
            }
        }

        public bool RecChecked
        {
            get { return _recorder != null; }
            set
            {
                var fileName = string.Format("kinect{0}.rec", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                _recorder = new Recorder(_inputService.ActiveSource, fileName);
                _recorder.Start();
                NotifyOfPropertyChange(() => RecChecked);
                UpdateState();
            }
        }

        public bool PlayChecked
        {
            get { return _inputService.ActiveSource is KinectPlayer; }
            set 
            {                

            }
        }

        public bool PauseChecked
        {
            get { return false; }
            set
            {
                if (_recorder != null)
                {
                    _recorder.Stop();
                    _recorder.Dispose();
                    _recorder = null;
                    UpdateState();
                    NotifyOfPropertyChange(() => PauseChecked);
                }

                if (_inputService.ActiveSource is KinectPlayer)
                {
                    _inputService.ActiveSource.Stop();
                }
            }
        }

        public bool RTEnabled
        {
            get
            {
                return true;
            }
        }

        public bool RecEnabled
        {
            get
            {
                if (_recorder == null)
                    return true;
                return false;
            }
        }

        public bool PlayEnabled
        {
            get
            {
                if (_inputService.ActiveSource is KinectPlayer)
                    return true;
                return false;
            }
        }

        public bool PauseEnabled
        {
            get
            {
                if (_recorder != null)
                    return true;
                if (_inputService.ActiveSource is KinectPlayer)
                    return true;
                return false;
            }
        }
    }
}
