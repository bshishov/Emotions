using System.ComponentModel.Composition;
using Caliburn.Micro;
using Emotions.KinectTools.Tracking;
using Emotions.Modules.Game;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace Emotions.Modules.Engine
{
    [Export(typeof(IEngineService))]
    class Engine : IHandle<GameFrame>, IHandle<FaceTrackFrame>
    {
        private readonly ILog _log = LogManager.GetLog(typeof(Engine));
        private readonly EventAggregator _eventAggregator;
        
        [ImportingConstructor]
        public Engine(EventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            _log.Info("Engine initialized");
        }
     
        public void Handle(GameFrame message)
        {
            _log.Info("Gameframe received");
        }

        public void Handle(FaceTrackFrame message)
        {
            var engineFrame = EngineInputFrame.FromFaceTrackFrame(message, null);
            _eventAggregator.Publish(engineFrame);
        }
    }
}
