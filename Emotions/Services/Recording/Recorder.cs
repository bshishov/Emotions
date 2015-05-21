using System;
using Caliburn.Micro;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;
using Emotions.KinectTools.Tracking;
using Emotions.Modules.Game;
using Emotions.Modules.Game.ViewModels;
using Emotions.Services.Engine;
using Emotions.ViewModels;

namespace Emotions.Services.Recording
{
    public class Recorder : IDisposable
    {
        private readonly ILog _log = LogManager.GetLog(typeof(Recorder));
        private readonly WriterContainer _container;
        private readonly IKinectSource _source;
        private readonly IEngineService _engine;
        private readonly GameViewModel _gameVm;

        public Recorder(IKinectSource source, GameViewModel gameVm, string path)
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
            _container.Write(new EngineFrame()); // buffer
            _source = source;
        }

        private void GameVmOnFrameReady(GameViewModel gameViewModel, GameFrame gameFrame)
        {
            _container.Write(gameFrame);
        }

        private void EngineOnOnUpdate(object sender, EngineUpdateEventArgs args)
        {
            _container.Write(args.EngineFrame);
        }

        public void Start()
        {
            _log.Info("Recorder started");
            _source.FramesReady += FramesReady;
            _engine.OnUpdate += EngineOnOnUpdate;

            if(_gameVm != null)
                _gameVm.FrameReady += GameVmOnFrameReady;
        }

        private void FramesReady(object sender, FramesContainer e)
        {
            if (_container == null)
                throw new Exception("This recorder is stopped");

            _container.Write(e);
        }
        
        public void Stop()
        {
            _log.Info("Recorder stopped");
            _source.FramesReady -= FramesReady;
            _engine.OnUpdate -= EngineOnOnUpdate;

            if(_gameVm != null)
                _gameVm.FrameReady -= GameVmOnFrameReady;

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
