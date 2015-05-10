using System;

namespace Emotions.Services.Engine
{
    class EngineUpdateEventArgs : EventArgs
    {
        public readonly Frame Frame;

        public EngineUpdateEventArgs(Frame frame)
        {
            Frame = frame;
        }
    }
}