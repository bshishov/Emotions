using System.ComponentModel.Composition;
using Caliburn.Micro;
using Emotions.KinectTools.Tracking;
using Emotions.Modules.Engine.Views;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Emotions.Modules.Engine.ViewModels
{
    [Export(typeof(AUViewModel))]
    class AUViewModel : Tool, IHandle<EngineInputFrame>
    {
        private AUView _view;

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Right; }
        }

        [ImportingConstructor]
        public AUViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
            DisplayName = "Action Units";
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            _view = this.GetView() as AUView;
        }

        //public delegate void UpdateUICallback(EngineInputFrame buffer);

        public void Handle(EngineInputFrame engineInputFrame)
        {
            //_view.Dispatcher.Invoke(new UpdateUICallback(UpdateUI), engineInputFrame);
            _view.Update(engineInputFrame);
        }
    }
}
