using System.IO;

namespace Emotions.KinectTools.Readers
{
    public class StreamableReader<T> : IReader<T>
        where T : IStreamable, new()
    {
        private FileStream _stream;
        private BinaryReader _reader;
        private string _path;

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

        public T Read()
        {
            var data = new T();
            data.FromStream(_reader);
            return data;
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