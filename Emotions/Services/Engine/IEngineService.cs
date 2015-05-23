using System;
using Emotions.KinectTools.Sources;
using Emotions.KinectTools.Tracking;

namespace Emotions.Services.Engine
{
    interface IEngineService
    {
        event Action<IEngineService, EngineInputFrame> Updated;
        event Action<IEngineService, IKinectSource> SourceChanged;
        IKinectSource ActiveSource{ get; }
        bool IsRunning { get; }
        void Start(IKinectSource source);
        void Stop();
    }
}