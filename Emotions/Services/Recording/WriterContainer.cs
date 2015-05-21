using System;
using System.IO;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;
using Emotions.KinectTools.Tracking;
using Emotions.KinectTools.Writers;

namespace Emotions.Services.Recording
{
    class WriterContainer : 
        IWriter<FramesContainer>,
        IWriter<EngineFrame>,
        IWriter<GameFrame>
    {
        private IWriter<ColorFrame> _colorWriter;
        private IWriter<DepthFrame> _depthWriter;
        private IWriter<SkeletonFrame> _skeletonWriter;
        private IWriter<EngineFrame> _engineWriter;
        private IWriter<GameFrame> _gameWriter;

        private FileStream _stream;
        private BinaryWriter _writer;
        private KinectSourceInfo _info;
        private DateTime _prevTime;
        

        public WriterContainer(KinectSourceInfo info)
        {
            _info = info;
            _colorWriter = new KinectColorStreamWriter(info);
            _depthWriter = new KinectDepthStreamWriter(info);
            _skeletonWriter = new StreamableWriter<SkeletonFrame>();
            _engineWriter = new StreamableWriter<EngineFrame>();
            _gameWriter = new StreamableWriter<GameFrame>();
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
            _engineWriter.Open(System.IO.Path.Combine(dirInfo.FullName, string.Format("{0}_engine.bin", fileNameWithoutExt)));
            _gameWriter.Open(System.IO.Path.Combine(dirInfo.FullName, string.Format("{0}_game.bin", fileNameWithoutExt)));

            _info.ToStream(_writer);
            _writer.Write(System.IO.Path.GetFileName(_colorWriter.Path));
            _writer.Write(System.IO.Path.GetFileName(_depthWriter.Path));
            _writer.Write(System.IO.Path.GetFileName(_skeletonWriter.Path));
            _writer.Write(System.IO.Path.GetFileName(_engineWriter.Path));
            _writer.Write(System.IO.Path.GetFileName(_gameWriter.Path));
            _writer.Flush();
        }
        
        private void WriteTimeStamp(byte streamId)
        {
            int time;
            if (_prevTime == default(DateTime))
                time = 0;
            else
                time = DateTime.Now.Subtract(_prevTime).Milliseconds;    
            
            _prevTime = DateTime.Now;   
            _writer.Write(streamId);
            _writer.Write(time);
        }

        public void Write(FramesContainer frames)
        {
            WriteTimeStamp(0);
            _colorWriter.Write(frames.ColorFrame);
            _depthWriter.Write(frames.DepthFrame);
            _skeletonWriter.Write(frames.SkeletonFrame);
        }  

        public void Write(EngineFrame frame)
        {
            WriteTimeStamp(1);
            _engineWriter.Write(frame);
        }

        public void Write(GameFrame data)
        {
            WriteTimeStamp(2);
            _gameWriter.Write(data);
        }

        public void Close()
        {
            _prevTime = default(DateTime);
            _writer.Close();
            _colorWriter.Close();
            _depthWriter.Close();
            _skeletonWriter.Close();
            _engineWriter.Close();
            _gameWriter.Close();
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

            if(_engineWriter != null)
                _engineWriter.Dispose();

            if (_gameWriter != null)
                _gameWriter.Dispose();
        }
    }
}
