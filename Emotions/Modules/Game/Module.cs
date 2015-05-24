using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Emotions.Modules.Game.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Results;
using Gemini.Modules.MainMenu.Models;
using Microsoft.Win32;

namespace Emotions.Modules.Game
{
    [Export(typeof(IModule))]
    class Module : ModuleBase
    {
        public override IEnumerable<Type> DefaultTools
        {
            get
            {
                yield return typeof(GameStatsViewModel);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            var view = MainMenu.All.First(x => x.Name == "View");
            view.Add(new MenuItem("Game", () => Enumerable.Repeat((IResult)Show.Document<GameViewModel>(), 1)));
            view.Add(new MenuItem("Game Stats", () => Enumerable.Repeat((IResult)Show.Tool<GameStatsViewModel>(), 1)));

            var game = new MenuItem("Game")
            {
                new MenuItem("New game", () => Enumerable.Repeat((IResult) Show.Document<GameViewModel>(), 1)),
                new MenuItem("Stats", () => Enumerable.Repeat((IResult) Show.Tool<GameStatsViewModel>(), 1)),
                new MenuItemSeparator(),
                new MenuItem("Open data", OpenData)
            };
            MainMenu.Add(game);
        }

        private IEnumerable<IResult> OpenData()
        {
            var dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true
            };
            yield return Show.CommonDialog(dialog);
            var provider = new GameDataEditorProvider();
            if (provider.Handles(dialog.FileName))
            {
                yield return Show.Document(provider.Create(dialog.FileName));
            }
        }
    }
}
