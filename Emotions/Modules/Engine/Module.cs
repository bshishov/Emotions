using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Emotions.Modules.Engine.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Results;
using Gemini.Modules.MainMenu.Models;

namespace Emotions.Modules.Engine
{
    [Export(typeof(IModule))]
    class Module : ModuleBase
    {
        public override IEnumerable<Type> DefaultTools
        {
            get
            {
                yield return typeof(AUViewModel);
                yield return typeof(EngineControlViewModel);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            var view = MainMenu.All.First(x => x.Name == "View");
            view.Add(new MenuItem("Action Units", () => Enumerable.Repeat((IResult)Show.Tool<AUViewModel>(), 1)));
            view.Add(new MenuItem("Engine Control", () => Enumerable.Repeat((IResult)Show.Tool<EngineControlViewModel>(), 1)));
        }
    }
}
