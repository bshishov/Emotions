using System;
using System.IO;

namespace Emotions.KinectTools
{
    public class Recorder : IDisposable
    {
        private BinaryWriter _writer;
        private FileStream _stream;
        private readonly IKinectSource _source;
        private DateTime _previousFlushDate;

        public Recorder(IKinectSource source, string path)
        {
            _source = source;
            if(_source == null)
                throw new Exception("This Kinect Source is null");

            _stream = File.Create(path);
            _source = source;
            _previousFlushDate = DateTime.Now;
        }

        public void Start()
        {
            _writer = new BinaryWriter(_stream);
            _source.FramesReady += FramesReady;
            _source.Info.ToStream(_writer);
        }

        private void FramesReady(object sender, FramesReadyEventArgs e)
        {
            if (_writer == null)
                throw new Exception("This recorder is stopped");

            e.ColorFrame.ToStream(_writer);
            e.DepthFrame.ToStream(_writer);
            e.SkeletonFrame.ToStream(_writer);

            Flush();
        }
        
        private void Flush()
        {
            // Flush every second
            if (DateTime.Now.Subtract(_previousFlushDate).TotalSeconds < 60) 
                return;

            _previousFlushDate = DateTime.Now;
            _writer.Flush();
        }

        public void Stop()
        {
            _source.FramesReady -= FramesReady;

            if (_writer == null)
                throw new Exception("This recorder is already stopped");

            _writer.Close();
            _writer.Dispose();

            _stream.Dispose();
            _stream = null;
        }

        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Close();
                _writer.Dispose();
            }

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }
    }
}
