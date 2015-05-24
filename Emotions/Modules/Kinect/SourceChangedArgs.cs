using Emotions.KinectTools.Sources;

namespace Emotions.Modules.Kinect
{
    class SourceChangedArgs
    {
        public IKinectSource OldSource { get; private set; }
        public IKinectSource NewSource { get; private set; }

        public SourceChangedArgs(IKinectSource oldSource, IKinectSource newSource)
        {
            OldSource = oldSource;
            NewSource = newSource;
        }
    }
}