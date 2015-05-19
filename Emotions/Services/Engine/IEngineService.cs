using Emotions.KinectTools;
using Emotions.KinectTools.Tracking;

namespace Emotions.Services.Engine
{
    interface IEngineService
    {
        event Engine.UpdateHandler OnUpdate;
        event Engine.EngineStateChangedHandler OnEngineStateChanged;
        EngineState CurrentState { get; }
        Recording Recording { get; }
        SkeletonFaceTracker ActiveTracker { get; }
        void Start();
        void StartPlaying();
        void LoadRecording(Recording recording);
        void StartRecording();
        void StopRecording();
        void Stop();
        void Bind(SkeletonFaceTracker skeletonFaceTracker);
        void Unbind();
    }
}