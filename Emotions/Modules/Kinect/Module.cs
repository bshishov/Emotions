using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Emotions.Modules.Kinect.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Results;
using Gemini.Modules.MainMenu.Models;

namespace Emotions.Modules.Kinect
{
    [Export(typeof(IModule))]
    class Module : ModuleBase
    {
        [Import]
        private IKinectInputService _kinectService;

        public override void Initialize()
        {
            base.Initialize();
            var view = MainMenu.All.First(x => x.Name == "View");
            view.Add(new MenuItem("Kinect Output", ShowKinectOutput));
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
