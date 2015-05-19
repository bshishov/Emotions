using System;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;
using Emotions.KinectTools.Writers;

namespace Emotions.KinectTools.Recorders
{
    public class KinectVideoRecorder : IDisposable
    {
        private readonly IWriter<DepthFrame> _writer;
        private readonly IKinectSource _source;        

        public KinectVideoRecorder(IKinectSource source, string path)
        {
            _source = source;
            if(_source == null)
                throw new Exception("This Kinect Source is null");

            _writer = new KinectDepthStreamWriter(source.Info);
            _writer.Open(path);
        }

        public void Start()
        {
            _source.FramesReady += FramesReady;
        }

        private void FramesReady(object sender, FramesContainer e)
        {
            if (_writer == null)
                throw new Exception("This recorder is stopped");

            _writer.Write(e.DepthFrame);
        }

        public void Stop()
        {
            _source.FramesReady -= FramesReady;
            _writer.Close();
            _writer.Dispose();
        }

        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Close();
                _writer.Dispose();
            }
        }
    }
}
