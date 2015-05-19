using System;
using System.Collections.Generic;

namespace Emotions.RecordingTools
{
    public interface IRecorder : IDisposable
    {
        event Action RecordingStarted;
        event Action RecordingEnded;
        void Start();
        void Stop();
    }

    public interface IPlayer : IDisposable
    {
        event Action PlaybackStarted;
        event Action PlaybackEnded;
        void Start();
        void Stop();
    }

    public interface IContainer : IDisposable
    {
        void Open(string path);
        IRecorder CreateRecorder();
        IPlayer CreatePlayer();
    }

    public interface IStream<TMeta, TData> : IDisposable
        where TMeta : IStreamable
        where TData : IStreamable
    {
        string Path { get; }
        void Open(string path);
        void WriteMetaData(TMeta data);
        void Write(TData data);
        TMeta ReadMetaData();
        TData Read();
    }
}
