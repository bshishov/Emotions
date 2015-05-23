using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Caliburn.Micro;
using Emotions.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace Emotions.Modules.Game.ViewModels
{
    [Export(typeof(GameViewModel))]
    public class GameViewModel : Document
    {
        class Circle
        {
            public readonly long CreationTime;
            public int TTL;
            public readonly bool IsGood;
            public readonly Ellipse Ellipse;

            public Circle(long time, Ellipse ellipse, int ttl, bool isgood)
            {
                CreationTime = time;
                Ellipse = ellipse;
                TTL = ttl;
                IsGood = isgood;
            }

            public void Update(int delay)
            {
                TTL -= delay;
            }
        }

        public event Action<GameViewModel, GameFrame> FrameReady;

        private Canvas _canvas;
        private bool _autoRec;
        private readonly Random _random;
        private int _scored;
        private int _failed;
        private int _missed;
        private int _missclicks;
        private int _reactionTime;
        private bool _showScoreboard;
        private int _totalScore;
        private readonly List<Circle> _circles = new List<Circle>();
        private readonly ILog _log = LogManager.GetLog(typeof(GameViewModel));
        private int _frame;
        private KinectOutputViewModel _kinectVm;

        // GAME PARAMS
        private const int FrameDelay = 1000 / 15;
        private const int MinSize = 50;
        private const int MaxSize = 200;
        private const int StartDelay= 700;
        private const int TargetDelay = 120;
        private const int TTL = 1300; // Time to live in ms
        private const double GoodCirclePropability = 0.5;
        private const long TargetTime = 1 * 60 * 1000; // minutes * (sec/min) * (ms / min)

        
        public bool ShowScoreboard
        {
            get { return _showScoreboard; }
            set
            {
                _showScoreboard = value;
                NotifyOfPropertyChange(() => ShowScoreboard);
            }
        }
        
        public int TotalScore
        {
            get { return _totalScore; }
            set
            {
                _totalScore = value;
                NotifyOfPropertyChange(() => TotalScore);
            }
        }

        public int Missclicks
        {
            get { return _missclicks; }
            set
            {
                _missclicks = value;
                NotifyOfPropertyChange(() => Missclicks);
            }
        }

        public int Missed
        {
            get { return _missed; }
            set
            {
                _missed = value;
                NotifyOfPropertyChange(() => Missed);
            }
        }

        public int Failed
        {
            get { return _failed; }
            set
            {
                _failed = value;
                NotifyOfPropertyChange(() => Failed);
            }
        }

        public int Scored
        {
            get { return _scored; }
            set
            {
                _scored = value;
                NotifyOfPropertyChange(() => Scored);
            }
        }

        public int ReactionTime
        {
            get { return _reactionTime; }
            set
            {
                _reactionTime = value;
                NotifyOfPropertyChange(() => ReactionTime);
            }
        }

        public bool AutoRec
        {
            get { return _autoRec; }
            set
            {
                _autoRec = value;
                NotifyOfPropertyChange(() => AutoRec);
            }
        }

        public GameViewModel()
        {
            DisplayName = "Game";
            _random = new Random();
        }

        public void OnCanvasLoaded(object sender, object context)
        {
            _canvas = sender as Canvas;
        }

        public void OnStartClicked()
        {
            Scored = 0;
            Failed = 0;
            Missed = 0;
            Missclicks = 0;
            ReactionTime = 0;
            _frame = 0;

            if (AutoRec)
            {
                var shell = IoC.Get<IShell>();
                var kinectVm = shell.Documents.FirstOrDefault(
                            d => d is KinectOutputViewModel && ((KinectOutputViewModel) d).IsEngineEnabled);
                if (kinectVm == null)
                {
                    _log.Warn("Can't start recording, no active kinect viewers with engine tracking found");
                    return;
                }
                _kinectVm = (KinectOutputViewModel) kinectVm;
                _kinectVm.StartRecording(this);
            }
            ShowScoreboard = false;
            IoC.Get<GameStatsViewModel>().Bind(this);
            Task.Factory.StartNew(UpdateCycle);
        }
        
        private void SpawnCircle(long time)
        {
            var isGood = _random.NextDouble() < GoodCirclePropability;
            var ellipse = new Ellipse();

            var size = _random.Next(MinSize, MaxSize);
            ellipse.Width = size;
            ellipse.Height = size;
            ellipse.Fill = isGood ? Brushes.Blue : Brushes.Red;

            //TODO prevent overlapping
            Canvas.SetLeft(ellipse, _random.Next(0, (int)_canvas.Width - (int)ellipse.Width));
            Canvas.SetTop(ellipse, _random.Next(0, (int)_canvas.Height - (int)ellipse.Height));

            _circles.Add(new Circle(time, ellipse, TTL, isGood));
            _canvas.Children.Add(ellipse);            
        }

        private void UpdateCycle()
        {
            long totalTime = 0;
            var startFrameTime = DateTime.Now;
            var lastCircleSpawnTime = DateTime.Now;
            var spawnCicrleDelegate = new Action<long>(SpawnCircle);
            var updateCirclesDelegate = new Action<double, int>(UpdateCircles);

            while (totalTime < TargetTime)
            {
                var progress = (double)totalTime / TargetTime;
                var spawnDelay = StartDelay - (int)(progress * (StartDelay - TargetDelay));
                
                if (DateTime.Now.Subtract(lastCircleSpawnTime).Milliseconds > spawnDelay)
                {
                    _canvas.Dispatcher.Invoke(spawnCicrleDelegate, new object[] { totalTime });
                    lastCircleSpawnTime = DateTime.Now;
                }

                _canvas.Dispatcher.Invoke(updateCirclesDelegate, new object[]{ progress, FrameDelay });
                startFrameTime = startFrameTime.Add(TimeSpan.FromMilliseconds(FrameDelay));
                Thread.Sleep(FrameDelay);
                totalTime += FrameDelay;
            }

            if (_kinectVm != null && _kinectVm.IsRecording)
                _kinectVm.StopRecording();

            TotalScore = Scored;
            ShowScoreboard = true;
        }

        private void UpdateCircles(double progress, int delta)
        {
            foreach (var circle in _circles.ToList())
            {
                circle.Update(delta);
                if (circle.TTL < 0)
                {
                    if (circle.IsGood)
                    {
                        Missed++;
                        if (FrameReady != null)
                            FrameReady(this, GetFrame());
                    }
                    _canvas.Children.Remove(circle.Ellipse);
                    _circles.Remove(circle);
                }
            }
        }

        public void OnCanvasMouseLeftButtonUp(object argsRaw)
        {
            var args = argsRaw as MouseButtonEventArgs;
            var circle = _circles.FirstOrDefault((e) => e.Ellipse.IsMouseOver);
            if (circle != null)
            {
                if (circle.IsGood)
                {
                    Scored += 1;
                    ReactionTime = DateTime.Now.Subtract(circle.CreationTime).Milliseconds;
                }
                else
                    Failed += 1;

                _canvas.Children.Remove(circle.Ellipse);
                _circles.Remove(circle);
            }
            else
            {
                Missclicks += 1;
            }

            if (FrameReady != null)
                FrameReady(this, GetFrame());
        }

        private GameFrame GetFrame()
        {
            var frame = new GameFrame()
            {
                Missed = Missed,
                FrameNumber = _frame++,
                Missclicks = Missclicks,
                Failed = Failed,
                ReactionTime = ReactionTime,
                Scored = Scored,
                Time = DateTime.Now.Ticks
            };

            return frame;
        }
    }
}
