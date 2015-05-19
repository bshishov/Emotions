using System;
using System.IO;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;

namespace Emotions.KinectTools.Readers
{
    public class ReaderContainer : IReader<FramesContainer>
    {
        private IReader<ColorFrame> _colorReader;
        private IReader<DepthFrame> _depthReader;
        private IReader<SkeletonFrame> _skeletonReader;
        
        private FileStream _stream;
        private BinaryReader _reader;

        public string Path { get; private set; }
        public KinectSourceInfo Info { get; private set; }

        public bool IsEnded
        {
            get
            {
                if (_colorReader != null && _depthReader != null && _skeletonReader != null)
                    return _colorReader.IsEnded || _depthReader.IsEnded || _skeletonReader.IsEnded;
                return false;
            }
        }

        public ReaderContainer()
        {
            
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

            _skeletonReader = new KinectSkeletonStreamReader();
            _skeletonReader.Open(System.IO.Path.Combine(directory, _reader.ReadString()));
        }

        public FramesContainer Read()
        {
            var cFrame = _colorReader.Read();
            var dFrame = _depthReader.Read();
            var sFrame = _skeletonReader.Read();
            return new FramesContainer(cFrame, dFrame, sFrame);
        }

        public void Reset()
        {
            _colorReader.Reset();
            _depthReader.Reset();
            _skeletonReader.Reset();
        }

        public void Close()
        {
            _reader.Close();
            _colorReader.Close();
            _depthReader.Close();
            _skeletonReader.Close();
        }

        public void Dispose()
        {
            if (_colorReader != null)
                _colorReader.Dispose();

            if (_depthReader != null)
                _depthReader.Dispose();

            if (_skeletonReader != null)
                _skeletonReader.Dispose();

            if(_reader != null)
                _reader.Dispose();
        }
    }
}
