using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Gemini.Framework;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;
using Series = OxyPlot.Series.Series;


namespace Emotions.Modules.Game.ViewModels
{
    [Export(typeof(PlotViewModel))]
    class PlotViewModel : Document
    {
        private PlotModel _model;
        public PlotModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                NotifyOfPropertyChange(() => Model);
            }
        }
        
        public IEnumerable<Series> Series
        {
            get { return Model.Series; }
        }

        public PlotViewModel(string fileName, List<GameFrame> gameFrames)
        {
            DisplayName = fileName;

            var model = new PlotModel
            {
                Title = String.Format("{0} Gamestats", fileName),
                LegendPlacement = LegendPlacement.Inside,
                LegendPosition = LegendPosition.LeftTop,
            };

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Title = "Time, ms"
            });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Right, Key = "Right", Title = "Count" });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Key = "Left", Minimum = -1000, Maximum = 1500, Title = "Reaction Time, ms" });

            var reactiontimeSeries = new LineSeries
            {
                Title = "Reaction Time", 
                YAxisKey = "Left",
                MarkerType = MarkerType.Circle,
                Smooth = true
            };
            var missedSeries = new LineSeries
            {
                Title = "Missed", 
                YAxisKey = "Right",
                MarkerFill = OxyColors.Red,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Cross,
            };
            var missclickedSeries = new LineSeries
            {
                Title = "Missclicked", 
                YAxisKey = "Right",
                Color = OxyColors.Blue,
                MarkerFill = OxyColors.Blue,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Cross,
            };
            var totalFails = new LineSeries
            {
                Title = "Missed + Missclicked + Failed",
                YAxisKey = "Right",
                MarkerFill = OxyColors.Red,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Star,
            };
            var scored = new LineSeries
            {
                Title = "Scored - (Missed + Missclicked + Failed)",
                YAxisKey = "Right",
                MarkerFill = OxyColors.Red,
                MarkerStroke = OxyColors.Black,
                MarkerType = MarkerType.Diamond,
            };


            var startTime = gameFrames.First().Time;
            var prev = new GameFrame();

            foreach (var frame in gameFrames)
            {
                var ms = (frame.Time - startTime)/TimeSpan.TicksPerMillisecond;

                if(frame.Scored > prev.Scored)
                    reactiontimeSeries.Points.Add(new DataPoint(ms, frame.ReactionTime));
                else
                    totalFails.Points.Add(new DataPoint(ms, frame.Missed + frame.Missclicks + frame.Failed));

                scored.Points.Add(new DataPoint(ms, frame.Scored - (frame.Missed + frame.Missclicks + frame.Failed)));
                if (frame.Missed > prev.Missed)
                {
                    missedSeries.Points.Add(new DataPoint(ms, frame.Missed));
                }

                if (frame.Missclicks > prev.Missclicks)
                {
                    missclickedSeries.Points.Add(new DataPoint(ms, frame.Missclicks));
                }

                prev = frame;
            }

            model.Series.Add(reactiontimeSeries);
            model.Series.Add(missedSeries);
            model.Series.Add(missclickedSeries);

            model.Series.Add(totalFails);
            model.Series.Add(scored);
            
            Model = model;
            NotifyOfPropertyChange(()=>Series);
        }

        public void OnExport()
        {
            var dialog = new SaveFileDialog()
            {
                DefaultExt = ".png",
                AddExtension = true,
            };
            var res = dialog.ShowDialog();
            if (res != null && (bool)res)
            {
                using (var stream = File.Create(dialog.FileName))
                {
                    var pngExporter = new PngExporter {Width = 1024, Height = 768};
                    pngExporter.Export(Model, stream);
                }
            }
        }

        public void OnAddAnnotation()
        {
            
        }

        public void OnRefresh()
        {
            Model.InvalidatePlot(true);
        }
    }
}
