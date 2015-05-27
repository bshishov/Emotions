using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Emotions.Modules.Game.Views;
using Emotions.Modules.Kinect.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Emotions.Modules.Game.ViewModels
{
    [Export(typeof(GameStatsViewModel))]
    class GameStatsViewModel : Tool
    {
        private IShell _shell;
        private GameStatsView _view;
        private IGameFrameProvider _provider;

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Right; }
        }

        public GameStatsViewModel()
        {
            DisplayName = "Game stats";
            _shell = IoC.Get<IShell>();
            _shell.ActiveDocumentChanged += ShellOnActiveDocumentChanged;
        }

        private void ShellOnActiveDocumentChanged(object sender, EventArgs eventArgs)
        {
            IGameFrameProvider provider;
            if (_shell.ActiveItem is KinectOutputViewModel)
                provider = ((KinectOutputViewModel) _shell.ActiveItem).CurrentSource as IGameFrameProvider;
            else
                provider = _shell.ActiveItem as IGameFrameProvider;

            if (provider != null)
            {
                if (_provider != null)
                    _provider.GameFrameReady -= OnGameFrameReady;
                _provider = provider;
                _provider.GameFrameReady += OnGameFrameReady;
            }
        }

        protected override void OnViewLoaded(object view)
        {
            _view = this.GetView() as GameStatsView;
        }

        private void OnGameFrameReady(object sender, GameFrame frame)
        {
            if (_view != null)
                _view.Dispatcher.Invoke(() => _view.Update(frame));
        }
    }
}
