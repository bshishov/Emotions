using System.IO;

namespace Emotions.KinectTools.Writers
{
    public class StreamableWriter<T> : IWriter<T>
        where T : IStreamable
    {
        private FileStream _stream;
        private BinaryWriter _writer;

        public string Path { get; private set; }

        public void Open(string path)
        {
            Path = path;
            _stream = File.Create(path);
            _writer = new BinaryWriter(_stream);
        }

        public void Write(T inputFrame)
        {
            inputFrame.ToStream(_writer);
        }

        public void Close()
        {
            _writer.Close();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
