using Emotions.KinectTools.Sources;

namespace Emotions.Modules.Kinect
{
    internal interface IKinectInputService
    {
        bool IsKinectAvailable { get; }
        IKinectSource GetKinect();
        void Dispose();
    }
}