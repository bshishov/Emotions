using System.ComponentModel.Composition;
using Emotions.KinectTools.Tracking;
using Emotions.Services.Engine;
using Emotions.Views;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Emotions.ViewModels
{
    [Export(typeof(AUViewModel))]
    class AUViewModel : Tool
    {
        [Import] private IEngineService _engine;
        private AUView _view;

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Right; }
        }

        public AUViewModel()
        {
            DisplayName = "Action Units";
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            _engine.Updated += EngineOnUpdated;
            _view = this.GetView() as AUView;
        }

        public delegate void UpdateUICallback(EngineInputFrame buffer);

        private void EngineOnUpdated(IEngineService engineService, EngineInputFrame engineInputFrame)
        {
            _view.Dispatcher.Invoke(new UpdateUICallback(UpdateUI), engineInputFrame);
        }

        private void UpdateUI(EngineInputFrame engineInputFrame)
        {
            _view.Update(engineInputFrame);
        }
    }
}
