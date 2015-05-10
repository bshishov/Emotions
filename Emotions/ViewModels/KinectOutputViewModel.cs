using System.ComponentModel.Composition;
using Emotions.Services.KinectInput;
using Gemini.Framework;

namespace Emotions.ViewModels
{
    [Export(typeof(KinectOutputViewModel))]
    class KinectOutputViewModel : Document
    {
        [Import]
        private IKinectInputService _inputService;

        public string DisplayName
        {
            get { return "Kinect Output"; }
        }

        public void OnViewerInitialized(object sender, object context)
        {
            var control = sender as KinectViewer;
            _inputService.AttachViewer(control);
        }
    }
}
