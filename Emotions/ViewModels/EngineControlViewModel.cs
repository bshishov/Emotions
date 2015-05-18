using System;
using System.ComponentModel.Composition;
using Emotions.Services.Engine;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Emotions.ViewModels
{
    [Export(typeof(EngineControlViewModel))]
    class EngineControlViewModel : Tool
    {
        [Import]
        private IEngineService _engine;
        private bool _rtChecked;
        private EngineState _state;

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
            _engine.OnEngineStateChanged += EngineOnOnEngineStateChanged;
            UpdateState(_engine.CurrentState);
        }

        private void EngineOnOnEngineStateChanged(object sender, EngineStateChangedEventArgs args)
        {
            var state = args.NewValue;
            UpdateState(state);
        }

        private void UpdateState(EngineState state)
        {
            _state = state;
            NotifyOfPropertyChange(() => RTEnabled);
            NotifyOfPropertyChange(() => RecEnabled);
            NotifyOfPropertyChange(() => PlayEnabled);
            NotifyOfPropertyChange(() => PauseEnabled);
            NotifyOfPropertyChange(() => RTChecked);
            NotifyOfPropertyChange(() => RecChecked);
            NotifyOfPropertyChange(() => PlayChecked);
            NotifyOfPropertyChange(() => PauseChecked);
        }

        public bool RTChecked
        {
            get { return _state == EngineState.KinectRealTime; }
            set
            {
                if (_engine != null)
                    _engine.Start();
            }
        }

        public bool RecChecked
        {
            get { return _state == EngineState.KinectRecording; }
            set
            {
                if (_engine != null)
                    _engine.StartRecording();
            }
        }

        public bool PlayChecked
        {
            get { return _state == EngineState.PlayingRecording; }
            set
            {
                if (_engine != null)
                    _engine.StartPlaying();
            }
        }

        public bool PauseChecked
        {
            get { return _state == EngineState.Stopped; }
            set
            {
                if (_engine != null)
                    _engine.Stop();
            }
        }

        public bool RTEnabled
        {
            get
            {
                if (_state == EngineState.PlayingRecording)
                    return false;
                return true;
            }
        }

        public bool RecEnabled
        {
            get
            {
                if (_state == EngineState.KinectRealTime)
                    return true;
                return false;
            }
        }

        public bool PlayEnabled
        {
            get
            {
                if (_state == EngineState.Stopped)
                    return true;
                return false;
            }
        }

        public bool PauseEnabled
        {
            get
            {
                if (_state == EngineState.PlayingRecording)
                    return true;
                if (_state == EngineState.KinectRealTime)
                    return true;
                if (_state == EngineState.KinectRecording)
                    return true;
                return false;
            }
        }
    }
}