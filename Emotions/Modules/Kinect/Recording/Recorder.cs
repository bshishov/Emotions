using System;
using Caliburn.Micro;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;
using Emotions.KinectTools.Tracking;
using Emotions.Modules.Engine;
using Emotions.Modules.Game;

namespace Emotions.Modules.Kinect.Recording
{
    public class Recorder : IDisposable
    {
        private readonly ILog _log = LogManager.GetLog(typeof(Recorder));
        private readonly WriterContainer _container;
        private readonly IKinectSource _source;
        private readonly IEngineService _engine;
        private readonly IGameFrameProvider _gameVm;
        
        public Recorder(IKinectSource source, IGameFrameProvider gameVm, string path)
        {
            _source = source;
            _engine = IoC.Get<IEngineService>();
            if(_source == null)
                throw new Exception("This Kinect Source is null");
            if(_engine == null)
                throw new Exception("Can't detect engine");
            if(_engine.ActiveSource != _source)
                throw  new Exception("Engine is binded to a different source");

            if (gameVm == null)
            {
                _log.Error(new Exception("No game view model specified"));
            }
            else
            {
                _log.Info("Binding game stream to recorder");
                _gameVm = gameVm;
            }

            _container = new WriterContainer(source.Info);
            _container.Open(path);
            _container.Write(new EngineInputFrame()); // buffer
            _source = source;
        }

        private void GameVmOnFrameReady(object o, GameFrame gameFrame)
        {
            if (_container == null)
                throw new Exception("This recorder is stopped");
            _container.Write(gameFrame);
        }

        private void EngineOnUpdated(IEngineService engineService, EngineInputFrame engineInputFrame)
        {
            if (_container == null)
                throw new Exception("This recorder is stopped");
            _container.Write(engineInputFrame);
        }
        
        private void FramesReady(object sender, FramesContainer e)
        {
            if (_container == null)
                throw new Exception("This recorder is stopped");
            _container.Write(e);
        }
        
        public void Start()
        {
            _log.Info("Recorder started");
            _source.FramesReady += FramesReady;
            _engine.Updated += EngineOnUpdated;

            if(_gameVm != null)
                _gameVm.GameFrameReady += GameVmOnFrameReady;
        }

       
        public void Stop()
        {
            _log.Info("Recorder stopped");
            _source.FramesReady -= FramesReady;
            _engine.Updated -= EngineOnUpdated;

            if(_gameVm != null)
                _gameVm.GameFrameReady -= GameVmOnFrameReady;

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
