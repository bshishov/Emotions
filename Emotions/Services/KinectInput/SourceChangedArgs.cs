using Emotions.KinectTools;
using Emotions.KinectTools.Sources;

namespace Emotions.Services.KinectInput
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