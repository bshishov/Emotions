using System;
using Emotions.KinectTools;

namespace Emotions.Services.KinectInput
{
    internal interface IKinectInputService
    {
        event EventHandler<SourceChangedArgs> SourceChanged;
        IKinectSource ActiveSource { get; }
        void Dispose();
        void AttachViewer(KinectViewer viewer);
        void LoadRealKinect();
        void LoadRecording(string path);
    }
}