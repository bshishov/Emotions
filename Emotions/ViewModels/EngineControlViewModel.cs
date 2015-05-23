using System.ComponentModel.Composition;
using Emotions.KinectTools.Sources;
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
            _engine.SourceChanged += EngineOnSourceChanged;
        }

        private void EngineOnSourceChanged(IEngineService engineService, IKinectSource kinectSource)
        {
            NotifyOfPropertyChange(() => SourceName);
            NotifyOfPropertyChange(() => IsRunning);                
        }

        public string SourceName
        {
            get
            {
                if (_engine.ActiveSource == null)
                    return "None";
                return _engine.ActiveSource.Name;
            }
        }

        public bool IsRunning
        {
            get { return _engine.IsRunning; }
            set
            {
                if(!value)
                    _engine.Stop();
                NotifyOfPropertyChange(() => IsRunning);
            }
        }
    }
}