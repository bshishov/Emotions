using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Emotions.Modules.Game.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Results;
using Gemini.Modules.MainMenu.Models;

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

        public Module()
        {
        }

        public override void Initialize()
        {
            var view = MainMenu.All.First(x => x.Name == "View");
            view.Add(new MenuItem("Game", () => Enumerable.Repeat((IResult)Show.Document<GameViewModel>(), 1)));
            view.Add(new MenuItem("Game Stats", () => Enumerable.Repeat((IResult)Show.Tool<GameStatsViewModel>(), 1)));
        }
    }
}
