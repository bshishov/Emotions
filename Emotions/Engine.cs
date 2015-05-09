using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Emotions
{
    class Engine
    {
        private CancellationTokenSource _tokenSource;
        private Task _task;
        private const int FrameDelay = 100; // milliseconds
        private InputSnapshot _lastInput;

        public class UpdateEventArgs : EventArgs
        {
            public InputSnapshot Snapshot;
        }
        public delegate void UpdateHandler(object sender, UpdateEventArgs args);
        public event UpdateHandler OnUpdate;

        public bool IsRunning { get { return _task != null; } }
        public bool IsRecording { get; private set; }
        public Recording Recording { get; private set; }

        public Engine()
        {
            
        }

        public void ProceedInput(InputSnapshot input)
        {
            _lastInput = input;
            if (IsRecording)
            {
                if (Recording == null)
                    Recording = new Recording();
                Recording.Add(input);
            }
        }

        public void Start()
        {
            _lastInput = new InputSnapshot();
            if (!IsRunning)
            {
                _tokenSource = new CancellationTokenSource();
                _task = Task.Factory.StartNew(() => UpdateTask(_tokenSource.Token), _tokenSource.Token);
            }
        }

        public void StartPlaying(Recording recording)
        {
            if (!IsRunning)
            {
                _tokenSource = new CancellationTokenSource();
                _task = Task.Factory.StartNew(() => RecordingUpdateTask(recording, _tokenSource.Token), _tokenSource.Token);
            }
        }

        public void StartRecording()
        {
            if(IsRunning)
                IsRecording = true;
        }

        public Recording StopRecording()
        {
            if (IsRunning && IsRecording)
            {
                IsRecording = false;
                return Recording;
            }
            return null;
        }
        
        public void Stop()
        {
            if(IsRecording)
                StopRecording();
            if (IsRunning)
                _tokenSource.Cancel();
        }

        private void UpdateTask(CancellationToken token)
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

                if(token.IsCancellationRequested)
                    break;
            }
        }

        private void RecordingUpdateTask(Recording recording, CancellationToken token)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var enumerator = recording.Snapshots.GetEnumerator();
            var snapshot = enumerator.Current;
            if (snapshot == null)
                return;
            
            TimeSpan inputFrameTime = TimeSpan.Zero;
            
            while (true)
            {
                if (stopwatch.Elapsed > inputFrameTime)
                {
                    var prevFrameTime = snapshot.DateTime;
                    stopwatch.Restart();
                    Update(snapshot);
                    enumerator.MoveNext();
                    snapshot = enumerator.Current;
                    if(snapshot == null)
                        break; // Playing ends
                    inputFrameTime = snapshot.DateTime - prevFrameTime;
                }

                if (token.IsCancellationRequested)
                    break;
            }
        }

        private void Update(InputSnapshot snapshot)
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
                OnUpdate(this, new UpdateEventArgs() { Snapshot = snapshot });
            }
        }
    }
}
