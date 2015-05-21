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

        public Module()
        {
        }

        public override void Initialize()
        {
            var view = MainMenu.All.First(x => x.Name == "View");
            view.Add(new MenuItem("Game", () => Enumerable.Repeat((IResult)Show.Document<GameViewModel>(), 1)));
        }
    }
}
