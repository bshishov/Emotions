using System;

namespace Emotions.Modules.Game
{
    public interface IGameFrameProvider
    {
        event Action<object, GameFrame> GameFrameReady;
    }
}
