using System;

namespace Emotions.KinectTools.Readers
{
    public interface IReader<out T> : IDisposable
    {
        bool IsEnded { get; }
        void Open(string path);
        void Reset();
        T Read();
        void Close();
    }
}
