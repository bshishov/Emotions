using System;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Tracking;

namespace Emotions.KinectTools.Sources
{
    public interface IKinectSource : IDisposable
    {
        event Action<IKinectSource, FramesContainer> FramesReady;
        event Action<IKinectSource, EngineInputFrame> EngineFrameReady;
        event Action<IKinectSource> Started;
        event Action<IKinectSource> Stopped;

        SkeletonFaceTracker SkeletonFaceTracker { get; }
        string Name { get; }
        bool IsActive { get; }
        KinectSourceInfo Info { get; }
        void Start();
        void Stop();
    }
}
