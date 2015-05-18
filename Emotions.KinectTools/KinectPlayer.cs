using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
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
        private long _startStreamPos;
        private string _path;
        public string Name { get; private set; }
        public event Action<IKinectSource, FramesReadyEventArgs> FramesReady;
        public event Action<IKinectSource> Started;
        public event Action<IKinectSource> Stopped;
        public KinectSourceInfo Info { get; private set; }
        public bool IsActive { get; private set; }
        public KinectPlayer(string path)
        {
            _path = path;
            _stream = File.Open(path, FileMode.Open);
            _reader = new BinaryReader(_stream);

            Info = new KinectSourceInfo();
            Info.FromStream(_reader);
            Name = Path.GetFileName(path);
            _startStreamPos = _stream.Position;
        }

        public void Start()
        {
            _time = 0;
            if (!_stream.CanRead)
            {
                _stream = File.Open(_path, FileMode.Open);
                _stream.Seek(_startStreamPos, SeekOrigin.Begin);
                _reader = new BinaryReader(_stream);
            }
            else
            {
                _stream.Position = _startStreamPos;
            }

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
            IsActive = false;
            if (Stopped != null)
                Stopped(this);
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