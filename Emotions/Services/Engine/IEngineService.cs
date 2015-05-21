using System;
using Emotions.KinectTools.Sources;

namespace Emotions.Services.Engine
{
    interface IEngineService
    {
        event Engine.UpdateHandler OnUpdate;
        event Action<IEngineService, IKinectSource> SourceChanged;
        IKinectSource ActiveSource{ get; }
        bool IsRunning { get; }
        void Start(IKinectSource source);
        void Stop();
    }
}