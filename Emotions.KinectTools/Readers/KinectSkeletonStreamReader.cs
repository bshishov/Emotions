using System.IO;
using System.Runtime.InteropServices;
using Emotions.KinectTools.Frames;

namespace Emotions.KinectTools.Readers
{
    class KinectSkeletonStreamReader : IReader<SkeletonFrame>
    {
        private FileStream _stream;
        private BinaryReader _reader;
        private string _path;
        private int _frame;
        private const int FrameDelay = 30;

        public bool IsEnded
        {
            get 
            { 
                if(_reader != null)
                    return !_reader.BaseStream.CanRead;
                return false;
            }
        }

        public void Open(string path)
        {
            _path = path;
            Init();
        }

        private void Init()
        {
            _frame = 0;
            _stream = File.Open(_path, FileMode.Open);
            _reader = new BinaryReader(_stream);
        }

        public void Reset()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader.Dispose();
            }

            Init();
        }

        public SkeletonFrame Read()
        {
            var frame = new SkeletonFrame(_frame, _frame * FrameDelay);
            frame.FromStream(_reader);
            return frame;
        }

        public void Close()
        {
            _reader.Close();
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}