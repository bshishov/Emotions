using System.ComponentModel.Composition;
using Caliburn.Micro;
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
        private readonly ILog _log = LogManager.GetLog(typeof(AUViewModel));

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
            _engine.OnUpdate += EngineOnOnUpdate;
            _view = this.GetView() as AUView;
        }

        public delegate void UpdateUICallback(Frame buffer);

        private void EngineOnOnUpdate(object sender, EngineUpdateEventArgs args)
        {
            _view.Dispatcher.Invoke(new UpdateUICallback(UpdateUI), args.Frame);
        }

        private void UpdateUI(Frame frame)
        {
            _view.Update(frame);
        }
    }
}
