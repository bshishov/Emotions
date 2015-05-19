using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Readers;

namespace Emotions.KinectTools.Sources
{
    public class KinectPlayer : IKinectSource
    {
        private CancellationTokenSource _cancellationTokenSource;
        private ReaderContainer _reader;
        private long _time;
        private CancellationToken _token;
        
        public string Name { get; private set; }
        public event Action<IKinectSource, FramesContainer> FramesReady;
        public event Action<IKinectSource> Started;
        public event Action<IKinectSource> Stopped;
        public KinectSourceInfo Info { get; private set; }
        public bool IsActive { get; private set; }
        public KinectPlayer(string path)
        {
            _reader = new ReaderContainer();
            KinectSourceInfo info;
            _reader.Open(path);
            Info = _reader.Info;
            Name = Path.GetFileName(_reader.Path);
        }

        public void Start()
        {
            _time = 0;
            _reader.Reset();
            _cancellationTokenSource = new CancellationTokenSource();
            _token = _cancellationTokenSource.Token;
            Task.Factory.StartNew(UpdateTask, _reader, _cancellationTokenSource.Token);
            IsActive = true;
            if (Started != null)
                Started(this);
        }

        public void Stop()
        {
            if (_cancellationTokenSource == null)
                return;
            _cancellationTokenSource.Cancel();
        }

        private void UpdateTask(object o)
        {
            var reader = o as ReaderContainer;
            if(reader == null)
                throw new Exception("No binary reader specified");
            while (!(reader.IsEnded || _token.IsCancellationRequested))
            {
                var frames = _reader.Read();

                if (_time == 0)
                    _time = frames.ColorFrame.TimeStamp;
                
                Thread.Sleep((int)(frames.ColorFrame.TimeStamp - _time));

                _time = frames.ColorFrame.TimeStamp;

                if(FramesReady != null)
                    FramesReady(this, frames);
            }
            IsActive = false;
            if (Stopped != null)
                Stopped(this);
            reader.Close();
        }

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader.Dispose();
                _reader = null;
            }
        }
    }
}