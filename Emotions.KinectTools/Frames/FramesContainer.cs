namespace Emotions.KinectTools.Frames
{
    public class FramesContainer
    {
        public readonly ColorFrame ColorFrame;
        public readonly DepthFrame DepthFrame;
        public readonly SkeletonFrame SkeletonFrame;

        public FramesContainer(ColorFrame colorFrame, DepthFrame depthFrame, SkeletonFrame skeletonFrame)
        {
            DepthFrame = depthFrame;
            SkeletonFrame = skeletonFrame;
            ColorFrame = colorFrame;
        }
    }
}