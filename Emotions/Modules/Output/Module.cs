using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Emotions.Modules.Output.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Results;
using Gemini.Modules.MainMenu.Models;
using NLog.Config;

namespace Emotions.Modules.Output
{
    [Export(typeof(IModule))]
    class Module : ModuleBase
    {
        private OutputViewModel _outputViewModel;

        [ImportingConstructor]
        public Module(OutputViewModel outputViewModel)
        {
            _outputViewModel = outputViewModel;
        }

        public override IEnumerable<Type> DefaultTools
        {
            get
            {
                yield return typeof(OutputViewModel);
            }
        }

        public Module()
        {
            ConfigurationItemFactory.Default.Targets.RegisterDefinition("ShoutingTarget", typeof(ShoutingTarget));
            LogManager.GetLog = type => new NLogLogger(type);
        }

        public override void Initialize()
        {
            var view = MainMenu.All.First(x => x.Name == "View");
            view.Add(new MenuItem("Output", ShowLog));
            var log = LogManager.GetLog(typeof(Module));
            log.Info("Logger module initialized");
        }

        private IEnumerable<IResult> ShowLog()
        {
            yield return Show.Tool<OutputViewModel>();
        }
    }
}
