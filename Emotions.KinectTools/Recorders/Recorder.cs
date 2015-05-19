using System;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;
using Emotions.KinectTools.Writers;

namespace Emotions.KinectTools.Recorders
{
    public class Recorder : IDisposable
    {
        private WriterContainer _container;
        private readonly IKinectSource _source;

        public Recorder(IKinectSource source, string path)
        {
            _source = source;
            if(_source == null)
                throw new Exception("This Kinect Source is null");

            _container = new WriterContainer(source.Info);
            _container.Open(path);
            _source = source;
        }

        public void Start()
        {
            _source.FramesReady += FramesReady;
        }

        private void FramesReady(object sender, FramesContainer e)
        {
            if (_container == null)
                throw new Exception("This recorder is stopped");

            _container.Write(e);
        }
        
        public void Stop()
        {
            _source.FramesReady -= FramesReady;

            if (_container == null)
                throw new Exception("This recorder is already stopped");

            _container.Close();
        }

        public void Dispose()
        {
            if (_container != null)
            {
                _container.Close();
                _container.Dispose();
            }
        }
    }
}
