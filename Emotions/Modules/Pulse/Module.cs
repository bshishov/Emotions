using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Emotions.Modules.Pulse.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Results;
using Gemini.Modules.MainMenu.Models;

namespace Emotions.Modules.Pulse
{
    [Export(typeof(IModule))]
    class Module : ModuleBase
    {
        public override void Initialize()
        {
            var view = MainMenu.All.First(x => x.Name == "View");
            view.Add(new MenuItem("Pulse", OpenPulseViewModel));
        }

        private static IEnumerable<IResult> OpenPulseViewModel()
        {
            yield return Show.Document <PulseViewModel>();
        }
    }
}
