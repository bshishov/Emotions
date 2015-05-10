using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Emotions.Services.Engine
{
    [Export(typeof(IEngineService))]
    class Engine : IEngineService
    {
        private CancellationTokenSource _tokenSource;
        private Task _task;
        private const int FrameDelay = 1000 / 10; // 1000milliseconds / frames per second
        private Frame _lastInput;
        private readonly ILog _log = LogManager.GetLog(typeof(Engine));
        private EngineState _state = EngineState.Stopped;
        private Stopwatch _recordingStopwatch;

        public delegate void UpdateHandler(object sender, EngineUpdateEventArgs args);
        public delegate void EngineStateChangedHandler(object sender, EngineStateChangedEventArgs args);
        public event UpdateHandler OnUpdate;
        public event EngineStateChangedHandler OnEngineStateChanged;
        

        public EngineState CurrentState
        {
            get { return _state; }
            set
            {
                if(OnEngineStateChanged != null)
                    OnEngineStateChanged(this, new EngineStateChangedEventArgs(_state, value));
                _state = value;
                _log.Info("Engine state: {0}", value);
            }
        }

        public Recording Recording { get; private set; }

        public Engine()
        {
            _log.Info("Engine initialized");
        }

        public void ProceedInput(Frame input)
        {
            _lastInput = input;
            if (CurrentState == EngineState.KinectRecording)
            {
                if (Recording == null)
                    Recording = new Recording();
                input.Time = _recordingStopwatch.Elapsed;
                Recording.Add(input);
            }
        }

        public void Start()
        {
            _lastInput = new Frame();
            if (CurrentState == EngineState.Stopped)
            {
                _tokenSource = new CancellationTokenSource();
                _task = Task.Factory.StartNew(() => UpdateTask(_tokenSource.Token), _tokenSource.Token);
                CurrentState = EngineState.KinectRealTime;
            }
        }

        public void StartPlaying()
        {
            if (CurrentState == EngineState.Stopped)
            {
                if (Recording == null)
                {
                    _log.Warn("No recording loaded");
                    return;
                }

                _tokenSource = new CancellationTokenSource();
                _task = Task.Factory.StartNew(() => RecordingUpdateTask(Recording, _tokenSource.Token), _tokenSource.Token);
                CurrentState = EngineState.PlayingRecording;
            }
        }

        public void LoadRecording(Recording recording)
        {
            if (recording != null)
            {
                Recording = recording;
                _log.Info("Loaded recording: {0} frames", recording.Frames.Count());
            }
        }

        public void StartRecording()
        {
            if (CurrentState == EngineState.KinectRealTime)
            {
                _recordingStopwatch = new Stopwatch();
                _recordingStopwatch.Start();
                Recording = new Recording();
                CurrentState = EngineState.KinectRecording;
            }
        }

        public void StopRecording()
        {
            if (CurrentState == EngineState.KinectRecording)
            {
                CurrentState = EngineState.KinectRealTime;
                if (Recording != null)
                {
                    var fileName = string.Format(Recording.NameFormat, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                    try
                    {
                        Recording.Save(fileName);
                        _log.Info("Recording saved to {0}", fileName);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex);
                    }
                }

                Recording = null;
                _recordingStopwatch = null;
            }
        }
        
        public void Stop()
        {
            if (CurrentState == EngineState.KinectRecording)
            {
                _tokenSource.Cancel();
                StopRecording();
            }
            if (CurrentState == EngineState.KinectRealTime || CurrentState == EngineState.PlayingRecording)
                _tokenSource.Cancel();
            _log.Info("Engine stopped");
        }

        public void UpdateTask(CancellationToken token)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();   
            while (true)
            {
                if (stopwatch.ElapsedMilliseconds > FrameDelay)
                {
                    stopwatch.Restart();
                    Update(_lastInput);
                }

                if (token.IsCancellationRequested)
                {
                    _log.Info("Cancelation requested");
                    break;
                }
            }

            _log.Info("Update task finished. Exiting");
            CurrentState = EngineState.Stopped;
        }

        public void RecordingUpdateTask(Recording recording, CancellationToken token)
        {
            var enumerator = recording.Frames.GetEnumerator();
            enumerator.MoveNext();
            var snapshot = enumerator.Current;
            
            if (snapshot == null)
            {
                CurrentState = EngineState.Stopped;
                _log.Warn("No frames detected");
                return;
            }

            var startTime = DateTime.Now;
            while (true)
            {
                if (DateTime.Now - startTime >= snapshot.Time)
                {
                    Update(snapshot);
                    enumerator.MoveNext();
                    snapshot = enumerator.Current;
                    if (snapshot == null) break; // Playing ends
                }
                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
            CurrentState = EngineState.Stopped;
            _log.Info("End of recording");
        }

        public void Update(Frame frame)
        {
            /*if (_recognizer != null)
            {
                var ausDouble = new double[] { au[0], au[1], au[2], au[3], au[4], au[5] };
                NeutralEmotion.Value = _recognizer.Compute(EmotionType.Neutral, ausDouble);
                JoyEmotion.Value = _recognizer.Compute(EmotionType.Joy, ausDouble);
                SurpriseEmotion.Value = _recognizer.Compute(EmotionType.Surprise, ausDouble);
                AngerEmotion.Value = _recognizer.Compute(EmotionType.Anger, ausDouble);
                FearEmotion.Value = _recognizer.Compute(EmotionType.Fear, ausDouble);
                SadnessEmotion.Value = _recognizer.Compute(EmotionType.Sadness, ausDouble);
            }*/
            if (OnUpdate != null)
            {
                OnUpdate(this, new EngineUpdateEventArgs((Frame)frame.Clone()));
            }
        }


        public Frame GetFrame()
        {
            return (Frame)_lastInput.Clone();
        }
    }
}
