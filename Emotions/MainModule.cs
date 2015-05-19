using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Emotions.Services.KinectInput;
using Emotions.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Results;
using Gemini.Modules.MainMenu.Models;

namespace Emotions
{
    [Export(typeof(IModule))]
    class MainModule : ModuleBase
    {
        [Import] private IKinectInputService _kinectService;

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
            var view = MainMenu.All.First(x => x.Name == "View");
            view.Add(new MenuItem("Kinect Output", ShowKinectOutput));
            view.Add(new MenuItem("Animation Units", () => Enumerable.Repeat((IResult)Show.Tool<AUViewModel>(), 1)));
            view.Add(new MenuItem("Engine Control", () => Enumerable.Repeat((IResult)Show.Tool<EngineControlViewModel>(), 1)));
            view.Add(new MenuItem("Game", () => Enumerable.Repeat((IResult)Show.Document<GameViewModel>(), 1)));
        }

        public override void PostInitialize()
        {
            base.PostInitialize();
            if (_kinectService.IsKinectAvailable)
                Shell.OpenDocument(new KinectOutputViewModel(_kinectService.GetKinect()));
        }

        private IEnumerable<IResult> ShowKinectOutput()
        {
            if (_kinectService.IsKinectAvailable)
                yield return Show.Document(new KinectOutputViewModel(_kinectService.GetKinect()));
            yield return null;
        }
    }
}
