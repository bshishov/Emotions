using System;
using Microsoft.Kinect;

namespace Emotions.KinectTools
{
    public interface IKinectSource : IDisposable
    {
        event Action<IKinectSource, FramesReadyEventArgs> FramesReady;
        KinectSourceInfo Info { get; }
        void Start();
        void Stop();
    }
}
