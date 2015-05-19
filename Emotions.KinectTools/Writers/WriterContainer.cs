using System;
using System.IO;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;

namespace Emotions.KinectTools.Writers
{
    class WriterContainer : IWriter<FramesContainer>
    {
        private IWriter<ColorFrame> _colorWriter;
        private IWriter<DepthFrame> _depthWriter;
        private IWriter<SkeletonFrame> _skeletonWriter;

        private FileStream _stream;
        private BinaryWriter _writer;
        private KinectSourceInfo _info;

        public WriterContainer(KinectSourceInfo info)
        {
            _info = info;
            _colorWriter = new KinectColorStreamWriter(info);
            _depthWriter = new KinectDepthStreamWriter(info);
            _skeletonWriter = new KinectSkeletonStreamWriter();
        }

        public string Path { get; private set; }

        public void Open(string path)
        {
            if(path == null)
                throw new ArgumentNullException();

            Path = path;

            var dirInfo = Directory.CreateDirectory(path);
            var fileName = System.IO.Path.GetFileName(path);
            var fileNameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(path);

            _stream = File.Create(System.IO.Path.Combine(dirInfo.FullName, fileName));
            _writer = new BinaryWriter(_stream);


            _colorWriter.Open(System.IO.Path.Combine(dirInfo.FullName, string.Format("{0}_color.avi", fileNameWithoutExt)));
            _depthWriter.Open(System.IO.Path.Combine(dirInfo.FullName, string.Format("{0}_depth.avi", fileNameWithoutExt)));
            _skeletonWriter.Open(System.IO.Path.Combine(dirInfo.FullName, string.Format("{0}_skeleton.bin", fileNameWithoutExt)));

            _info.ToStream(_writer);
            _writer.Write(System.IO.Path.GetFileName(_colorWriter.Path));
            _writer.Write(System.IO.Path.GetFileName(_depthWriter.Path));
            _writer.Write(System.IO.Path.GetFileName(_skeletonWriter.Path));
            _writer.Flush();
        }

        public void Write(FramesContainer frames)
        {
            _colorWriter.Write(frames.ColorFrame);
            _depthWriter.Write(frames.DepthFrame);
            _skeletonWriter.Write(frames.SkeletonFrame);
        }

        public void Close()
        {
            _writer.Close();
            _colorWriter.Close();
            _depthWriter.Close();
            _skeletonWriter.Close();
        }

        public void Dispose()
        {
            if(_colorWriter != null)
                _colorWriter.Dispose();

            if (_depthWriter != null)
                _depthWriter.Dispose();

            if (_skeletonWriter != null)
                _skeletonWriter.Dispose();

            if(_writer != null)
                _writer.Dispose();
        }
    }
}
