using System.IO;
using Emotions.KinectTools.Frames;

namespace Emotions.KinectTools.Writers
{
    class KinectSkeletonStreamWriter : IWriter<SkeletonFrame>
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

        public void Write(SkeletonFrame colorFrame)
        {
            colorFrame.ToStream(_writer);
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
