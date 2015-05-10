using Emotions.ViewModels;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace Emotions.Services.KinectInput
{
    internal interface IKinectInputService
    {
        void InitSensor();
        void Dispose();
        void AttachViewer(KinectViewer viewer);
    }
}