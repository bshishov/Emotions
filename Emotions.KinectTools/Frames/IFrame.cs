namespace Emotions.KinectTools.Frames
{
    interface IFrame : IStreamable
    {
        int FrameNumber { get; }
        long TimeStamp { get; }
    }
}
