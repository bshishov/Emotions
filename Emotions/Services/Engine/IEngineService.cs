using Emotions.KinectTools.Sources;

namespace Emotions.Services.Engine
{
    interface IEngineService
    {
        event Engine.UpdateHandler OnUpdate;
        event Engine.EngineStateChangedHandler OnEngineStateChanged;
        EngineState CurrentState { get; }
        Recording Recording { get; }
        IKinectSource ActiveSource{ get; }
        void Start();
        void StartPlaying();
        void LoadRecording(Recording recording);
        void StartRecording();
        void StopRecording();
        void Stop();
        void Bind(IKinectSource source);
        void Unbind();
    }
}