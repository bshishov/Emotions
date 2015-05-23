using System;
using System.IO;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Readers;
using Emotions.KinectTools.Sources;
using Emotions.KinectTools.Tracking;
using Emotions.Modules.Game;

namespace Emotions.Services.Recording
{
    public class ReaderContainer : IReader<FramesContainer>, IReader<EngineInputFrame>, IReader<GameFrame>
    {
        private IReader<ColorFrame> _colorReader;
        private IReader<DepthFrame> _depthReader;
        private IReader<SkeletonFrame> _skeletonReader;
        private IReader<EngineInputFrame> _engineReader;
        private IReader<GameFrame> _gameReader;
        
        private FileStream _stream;
        private BinaryReader _reader;
        private long _startPos;

        public string Path { get; private set; }
        public int Time { get; private set; }
        public KinectSourceInfo Info { get; private set; }

        public bool IsEnded
        {
            get
            {
                if (_reader != null)
                    return _reader.BaseStream.Position >= _reader.BaseStream.Length;
                return false;
            }
        }
        
        public void Open(string path)
        {
            if(path == null)
                throw new ArgumentNullException("path");
            Path = path;
            
            _stream = File.Open(path, FileMode.Open);
            _reader = new BinaryReader(_stream);

            Info = new KinectSourceInfo();
            Info.FromStream(_reader);

            var directory = System.IO.Path.GetDirectoryName(path);

            _colorReader = new KinectColorStreamReader(Info);
            _colorReader.Open(System.IO.Path.Combine(directory,_reader.ReadString()));

            _depthReader = new KinectDepthStreamReader(Info);
            _depthReader.Open(System.IO.Path.Combine(directory, _reader.ReadString()));

            _skeletonReader = new StreamableReader<SkeletonFrame>();
            _skeletonReader.Open(System.IO.Path.Combine(directory, _reader.ReadString()));

            var engineReaderPath = _reader.ReadString();
            if (!String.IsNullOrEmpty(engineReaderPath))
            {
                _engineReader = new StreamableReader<EngineInputFrame>();
                _engineReader.Open(System.IO.Path.Combine(directory, engineReaderPath));
            }

            var gameReaderPath = _reader.ReadString();
            if (!String.IsNullOrEmpty(gameReaderPath))
            {
                _gameReader = new StreamableReader<GameFrame>();
                _gameReader.Open(System.IO.Path.Combine(directory, gameReaderPath));
            }

            _startPos = _reader.BaseStream.Position;

            Time = 0;
        }

        public FramesContainer Read()
        {
            var cFrame = _colorReader.Read();
            var dFrame = _depthReader.Read();
            var sFrame = _skeletonReader.Read();
            return new FramesContainer(cFrame, dFrame, sFrame);
        }

        public object ReadNextFrame(out int time)
        {
            var streamId = _reader.ReadByte();
            time = _reader.ReadInt32();
            switch (streamId)
            {
                case 0:
                    return ((IReader<FramesContainer>)this).Read();
                case 1:
                    return ((IReader<EngineInputFrame>)this).Read();
                case 2:
                    return ((IReader<GameFrame>)this).Read();
                default:
                    throw new Exception("Unrecognized stream id");
            }
        }

        public void Reset()
        {
            _reader.BaseStream.Seek(_startPos, SeekOrigin.Begin);
            _colorReader.Reset();
            _depthReader.Reset();
            _skeletonReader.Reset();

            if(_engineReader != null)
                _engineReader.Reset();

            if(_gameReader != null)
                _gameReader.Reset();
        }

        GameFrame IReader<GameFrame>.Read()
        {
            if (_gameReader != null)
            {
                var frame = _gameReader.Read();
                return frame;
            }
            throw new Exception("Game reader is not ready");
        }

        EngineInputFrame IReader<EngineInputFrame>.Read()
        {
            if (_engineReader != null)
            {
                var frame = _engineReader.Read();
                return frame;
            }
            throw  new Exception("Engine reader is not ready");
        }

        public void Close()
        {
            _reader.Close();
            _colorReader.Close();
            _depthReader.Close();
            _skeletonReader.Close();

            if (_engineReader != null)
                _engineReader.Close();

            if (_gameReader != null)
                _gameReader.Close();
        }

        public void Dispose()
        {
            if (_colorReader != null)
                _colorReader.Dispose();

            if (_depthReader != null)
                _depthReader.Dispose();

            if (_skeletonReader != null)
                _skeletonReader.Dispose();

            if (_engineReader != null)
                _engineReader.Dispose();

            if (_gameReader != null)
                _gameReader.Dispose();

            if(_reader != null)
                _reader.Dispose();
        }
    }
}
