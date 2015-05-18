using System;
using Microsoft.Kinect;

namespace Emotions.KinectTools
{
    public interface IKinectSource : IDisposable
    {
        event Action<IKinectSource, FramesReadyEventArgs> FramesReady;
        event Action<IKinectSource> Started;
        event Action<IKinectSource> Stopped;
        string Name { get; }
        bool IsActive { get; }
        KinectSourceInfo Info { get; }
        void Start();
        void Stop();
    }
}
