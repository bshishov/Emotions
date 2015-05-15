using System.ComponentModel.Composition;
using Caliburn.Micro;
using Emotions.Services.KinectInput;
using Gemini.Framework;

namespace Emotions.ViewModels
{
    [Export(typeof(KinectOutputViewModel))]
    class KinectOutputViewModel : Document
    {
        public string DisplayName
        {
            get { return "Kinect Output"; }
        }

        public void OnViewerInitialized(object sender, object context)
        {   
            var control = sender as KinectViewer;

            IoC.Get<IKinectInputService>().AttachViewer(control);
        }
    }
}
