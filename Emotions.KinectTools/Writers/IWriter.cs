using System;

namespace Emotions.KinectTools.Writers
{
    public interface IWriter<in T> : IDisposable
    {
        string Path { get; }
        void Open(string path);
        void Write(T data);
        void Close();
    }
}