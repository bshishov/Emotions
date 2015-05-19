using System;

namespace Emotions.KinectTools.Writers
{
    internal interface IWriter<in T> : IDisposable
    {
        string Path { get; }
        void Open(string path);
        void Write(T colorFrame);
        void Close();
    }
}