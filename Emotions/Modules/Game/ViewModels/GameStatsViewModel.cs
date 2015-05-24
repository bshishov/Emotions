using System.ComponentModel.Composition;
using Emotions.Modules.Game.Views;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Emotions.Modules.Game.ViewModels
{
    [Export(typeof(GameStatsViewModel))]
    class GameStatsViewModel : Tool
    {
        private GameStatsView _view;

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Right; }
        }

        public GameStatsViewModel()
        {
            DisplayName = "Game stats";
        } 

        protected override void OnViewLoaded(object view)
        {
            _view = this.GetView() as GameStatsView;
        }

        private void OnGameFrameReady(GameViewModel gameViewModel, GameFrame gameFrame)
        {
            if(_view != null)
                _view.Update(gameFrame);
        }

        public void Bind(GameViewModel vm)
        {
            vm.FrameReady += OnGameFrameReady;
        }
    }
}
