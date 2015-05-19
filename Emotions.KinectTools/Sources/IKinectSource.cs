using System;
using Emotions.KinectTools.Frames;

namespace Emotions.KinectTools.Sources
{
    public interface IKinectSource : IDisposable
    {
        event Action<IKinectSource, FramesContainer> FramesReady;
        event Action<IKinectSource> Started;
        event Action<IKinectSource> Stopped;
        string Name { get; }
        bool IsActive { get; }
        KinectSourceInfo Info { get; }
        void Start();
        void Stop();
    }
}
