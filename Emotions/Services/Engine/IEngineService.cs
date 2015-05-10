using System;
using System.Threading;

namespace Emotions.Services.Engine
{
    interface IEngineService
    {
        event Engine.UpdateHandler OnUpdate;
        event Engine.EngineStateChangedHandler OnEngineStateChanged;
        EngineState CurrentState { get; }
        Recording Recording { get; }
        void ProceedInput(Frame input);
        void Start();
        void StartPlaying();
        void LoadRecording(Recording recording);
        void StartRecording();
        void StopRecording();
        void Stop();
        void UpdateTask(CancellationToken token);
        void RecordingUpdateTask(Recording recording, CancellationToken token);
        void Update(Frame snapshot);
        Frame GetFrame();
    }
}