using Emotions.KinectTools.Frames;

namespace Emotions.KinectTools
{
    public class FramesReadyEventArgs
    {
        public readonly ColorFrame ColorFrame;
        public readonly DepthFrame DepthFrame;
        public readonly SkeletonFrame SkeletonFrame;

        public FramesReadyEventArgs(ColorFrame colorFrame, DepthFrame depthFrame, SkeletonFrame skeletonFrame)
        {
            DepthFrame = depthFrame;
            SkeletonFrame = skeletonFrame;
            ColorFrame = colorFrame;
        }
    }
}