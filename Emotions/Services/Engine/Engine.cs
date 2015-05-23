using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Emotions.KinectTools.Sources;
using Emotions.KinectTools.Tracking;
using Emotions.Modules.Game;
using Emotions.Services.KinectInput;

namespace Emotions.Services.Engine
{
    [Export(typeof(IEngineService))]
    class Engine : IEngineService, IDisposable
    {
        private CancellationTokenSource _tokenSource;
        private const int FrameDelay = 1000 / 10; // 1000milliseconds / frames per second
        private EngineInputFrame _inputBuffer;
        private readonly ILog _log = LogManager.GetLog(typeof(Engine));
        private IKinectSource _source;

        public event Action<IEngineService, EngineInputFrame> Updated;
        public event Action<IEngineService, IKinectSource> SourceChanged;

        public IKinectSource ActiveSource
        {
            get { return _source; }
            set
            {
                _source = value;
                if (SourceChanged != null)
                    SourceChanged(this, value);
            }
             
        }
        public bool IsRunning { get { return ActiveSource != null; } }

        public Engine()
        {
            _log.Info("Engine initialized");
        }

        private void ProceedInput(EngineInputFrame input)
        {
            _inputBuffer = input;
        }

        public void Start(IKinectSource source)
        {
            _inputBuffer = new EngineInputFrame();
            _tokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(() => UpdateTask(_tokenSource.Token), _tokenSource.Token);

            if (ActiveSource != null)
                ActiveSource.EngineFrameReady -= EngineFrameReady;
            ActiveSource = source;
            ActiveSource.EngineFrameReady += EngineFrameReady;

            var player = ActiveSource as KinectPlayer;
            if (player != null)
                player.GameFrameReady += OnGameFrameReady;
        }
        
        public void Stop()
        {
            _tokenSource.Cancel();
            _log.Info("Engine stopped");

            if (ActiveSource != null)
            {
                ActiveSource.EngineFrameReady -= EngineFrameReady;

                var player = ActiveSource as KinectPlayer;
                if (player != null)
                    player.GameFrameReady -= OnGameFrameReady;
            }

            ActiveSource = null;
        }

        private void UpdateTask(CancellationToken token)
        {
            while (true)
            {
                Thread.Sleep(FrameDelay);
                Update(_inputBuffer);

                if (token.IsCancellationRequested)
                {
                    _log.Info("Cancelation requested");
                    break;
                }
            }

            _log.Info("Update task finished. Exiting");
        }

        private void Update(EngineInputFrame engineInputFrame)
        {
            if (Updated != null)
                Updated(this, (EngineInputFrame)engineInputFrame.Clone());
        }
        
        private void OnGameFrameReady(IKinectSource kinectSource, GameFrame gameFrame)
        {
            _log.Info("Gameframe received");
        }

        private void EngineFrameReady(IKinectSource source, EngineInputFrame inputFrame)
        {
            if(ActiveSource != source)
                throw new Exception("Source is unbinded");
            ProceedInput(inputFrame);
        }
        
        public void Dispose()
        {
            if (IsRunning)
                Stop();
        }
    }
}
