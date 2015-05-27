using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;
using Emotions.KinectTools.Tracking;
using Emotions.Modules.Game;
using Emotions.Modules.Kinect.Recording;

namespace Emotions.Modules.Kinect
{
    public class KinectPlayer : IKinectSource, IGameFrameProvider
    {
        private CancellationTokenSource _cancellationTokenSource;
        private ReaderContainer _reader;
        private CancellationToken _token;

        public SkeletonFaceTracker SkeletonFaceTracker { get; private set; }
        public string Name { get; private set; }
        public event Action<IKinectSource, FramesContainer> FramesReady;
        public event Action<IKinectSource, EngineInputFrame> EngineFrameReady;
        public event Action<object, GameFrame> GameFrameReady;
        public event Action<IKinectSource> Started;
        public event Action<IKinectSource> Stopped;
        public KinectSourceInfo Info { get; private set; }
        public bool IsActive { get; private set; }
        public KinectPlayer(string path)
        {
            _reader = new ReaderContainer();
            _reader.Open(path);
            Info = _reader.Info;
            Name = Path.GetFileName(_reader.Path);
        }

        public void Start()
        {
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
            var lastTime = 0;
            if(reader == null)
                throw new Exception("No binary reader specified");
            while (!(reader.IsEnded || _token.IsCancellationRequested))
            {
                int time;
                var frame = _reader.ReadNextFrame(out time);
                Thread.Sleep(Math.Abs(time - lastTime) + 1);
                lastTime = time;

                if (frame is FramesContainer)
                {
                    if (FramesReady != null)
                        FramesReady(this, (FramesContainer)frame);
                }

                if (frame is EngineInputFrame)
                {
                    if (EngineFrameReady != null)
                        EngineFrameReady(this, (EngineInputFrame)frame);
                }

                if (frame is GameFrame)
                {
                    if (GameFrameReady != null)
                        GameFrameReady(this, (GameFrame)frame);
                }
            }
            IsActive = false;
            if (Stopped != null)
                Stopped(this);
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