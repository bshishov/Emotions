using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Emotions.KinectTools.Frames;

namespace Emotions.KinectTools
{
    public class KinectPlayer : IKinectSource
    {
        private CancellationTokenSource _cancellationTokenSource;
        private BinaryReader _reader;
        private FileStream _stream;
        private long _time;
        private CancellationToken _token;
        public event Action<IKinectSource, FramesReadyEventArgs> FramesReady;
        public event Action PlaybackEnded;
        public KinectSourceInfo Info { get; private set; }

        public KinectPlayer(string path)
        {
            _stream = File.Open(path, FileMode.Open);
            _reader = new BinaryReader(_stream);

            Info = new KinectSourceInfo();
            Info.FromStream(_reader);
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _token = _cancellationTokenSource.Token;
            Task.Factory.StartNew(UpdateTask, _reader, _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            if (_cancellationTokenSource == null)
                return;
            _cancellationTokenSource.Cancel();
        }

        private void UpdateTask(object o)
        {
            var reader = o as BinaryReader;
            if(reader == null)
                throw new Exception("No binary reader specified");

            var now = DateTime.Now.Millisecond;

            while (reader.BaseStream.Position < reader.BaseStream.Length && !_token.IsCancellationRequested)
            {
                var cFrame = new ColorFrame();
                cFrame.FromStream(reader);

                var dFrame = new DepthFrame();
                dFrame.FromStream(reader);

                var sFrame = new Emotions.KinectTools.Frames.SkeletonFrame();
                sFrame.FromStream(reader);

                if (_time == 0)
                    _time = cFrame.TimeStamp;

                // TODO: improve this part
                Thread.Sleep(TimeSpan.FromMilliseconds(cFrame.TimeStamp - _time));

                _time = cFrame.TimeStamp;

                if(FramesReady != null)
                    FramesReady(this, new FramesReadyEventArgs(cFrame, dFrame, sFrame));
            }

            if(PlaybackEnded != null)
                PlaybackEnded.Invoke();

            reader.Close();
            reader.Dispose();
        }

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader.Dispose();
                _reader = null;
            }

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }
    }
}