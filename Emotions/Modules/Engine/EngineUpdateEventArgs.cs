using System;
using Emotions.KinectTools.Tracking;

namespace Emotions.Modules.Engine
{
    class EngineUpdateEventArgs : EventArgs
    {
        public readonly EngineInputFrame EngineInputFrame;

        public EngineUpdateEventArgs(EngineInputFrame engineInputFrame)
        {
            EngineInputFrame = engineInputFrame;
        }
    }
}