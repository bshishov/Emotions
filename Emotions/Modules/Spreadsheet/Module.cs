
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Caliburn.Micro;
using Emotions.KinectTools.Readers;
using Emotions.KinectTools.Tracking;
using Emotions.Modules.Game;
using Emotions.Modules.Game.ViewModels;
using Emotions.Utilities;
using Gemini.Framework;
using MenuItem = Gemini.Modules.MainMenu.Models.MenuItem;

namespace Emotions.Modules.Spreadsheet
{
    [Export(typeof(IModule))]
    class Module : ModuleBase
    {
        public Module()
        {
            
        }

        public override void PostInitialize()
        {
            base.Initialize();
            var view = MainMenu.All.First(x => x.Name == "Tools");
            view.Add(new MenuItem("Results to separate datasets (dataset.csv)", ResultsToDatasets));
            view.Add(new MenuItem("Results to one dataset (dataset.csv)", ResultsToOneDataset));
        }

        private IEnumerable<IResult> ResultsToDatasets()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var collected = 0;
                foreach (var child in Directory.GetDirectories(dialog.SelectedPath))
                {
                    var resultsFolder = Path.Combine(dialog.SelectedPath, child);

                    using (var writer = new CsvWriter(Path.Combine(resultsFolder, "dataset.csv"), typeof(DatasetRow)))
                    {
                        WriteDataset(writer, resultsFolder);
                    }
                    collected++;
                }
                MessageBox.Show(string.Format("Collected results for {0} folders", collected));
            }

            yield return null;
        }

        private IEnumerable<IResult> ResultsToOneDataset()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (var writer = new CsvWriter(Path.Combine(dialog.SelectedPath, "dataset.csv"), typeof (DatasetRow)))
                {
                    var collected = 0;
                    foreach (var child in Directory.GetDirectories(dialog.SelectedPath))
                    {
                        WriteDataset(writer, Path.Combine(dialog.SelectedPath, child));
                        collected++;
                    }
                    MessageBox.Show(string.Format("Collected results for {0} folders", collected));
                }
            }

            yield return null;
        }
        

        private void WriteDataset(CsvWriter writer, string resultsFolderPath)
        {
            var values = Read(resultsFolderPath);

            foreach (var datasetRow in values)
                writer.WriteObject(datasetRow);
        }


        private IEnumerable<DatasetRow> Read(string basePath)
        {
            const string gameResultsLookup = "*game.bin";
            const string engineResultsLookup = "*engine.bin";

            var gameResultsFile = Directory.GetFiles(basePath, gameResultsLookup)[0];
            var gameFrames = GetGameFrames(gameResultsFile);

            var minTicks = gameFrames.Min(f => f.Time);
            var maxTicks = gameFrames.Max(f => f.Time);
            var tickPerMs = (maxTicks - minTicks)/GameViewModel.TargetTime;
            var gameFramesEnumerator = gameFrames.GetEnumerator();


            var results = new List<DatasetRow>();
            using (var engineReader = new StreamableReader<EngineInputFrame>())
            {
                var engineResultsFile = Directory.GetFiles(basePath, engineResultsLookup)[0];
                engineReader.Open(engineResultsFile);

                gameFramesEnumerator.MoveNext();
                var lastGameFrame = gameFramesEnumerator.Current;

                gameFramesEnumerator.MoveNext();
                var nextGameFrame = gameFramesEnumerator.Current;

                var id = 0;

                var startTime = TicksToRelativeTimeInMs(lastGameFrame.Time, minTicks, maxTicks);
                var time = startTime;
                while (!engineReader.IsEnded)
                {
                    if (nextGameFrame != null && time > TicksToRelativeTimeInMs(nextGameFrame.Time, minTicks, maxTicks))
                    {
                        lastGameFrame = nextGameFrame;

                        gameFramesEnumerator.MoveNext();
                        nextGameFrame = gameFramesEnumerator.Current;
                    }

                    var engineFrame = engineReader.Read();
                    var row = new DatasetRow(Path.GetFileName(basePath), id, time, engineFrame, lastGameFrame);
                    results.Add(row);

                    time += Engine.Engine.FrameDelay;
                    id++;
                }


                engineReader.Close();
            }
            return results;
        }

        private long TicksToRelativeTimeInMs(long ticks, long min, long max)
        {
            return (ticks - min)/((max - min)/GameViewModel.TargetTime);
        }

        private List<GameFrame> GetGameFrames(string path)
        {
            var frames = new List<GameFrame>();
            using (var reader = new StreamableReader<GameFrame>())
            {
                reader.Open(path);
                while (!reader.IsEnded)
                    frames.Add(reader.Read());
                reader.Close();
            }
            return frames;
        }
    }

    class DatasetRow
    {
        public string Name;
        public int Id;
        public long Time;

        //////////// START OF ENGINE
        // AUs
        public double LipRaiser;
        public double JawLowerer;
        public double LipStretcher;
        public double BrowLowerer;
        public double LipCornerDepressor;
        public double BrowRaiser;

        // FACE POS
        public float FacePositionX;
        public float FacePositionY;
        public float FacePositionZ;

        // FACE ROT
        public float FaceRotationX;
        public float FaceRotationY;
        public float FaceRotationZ;

        // SHOULDER CENTER
        public float ShoulderCenterX;
        public float ShoulderCenterY;
        public float ShoulderCenterZ;
        //////////// END OF ENGINE


        //////////// START OF GAME
        public int GameFrameNumber;
        public long GameTicks;
        public int GameMissed;
        public int GameMissclicks;
        public int GameScored;
        public int GameFailed;
        public int GameReactionTime;
        //////////// END OF GAME

        public DatasetRow(string name, int id, long time, EngineInputFrame engine, GameFrame game)
        {
            Name = name;
            Id = id;
            Time = time;

            LipRaiser = engine.LipRaiser;
            JawLowerer = engine.JawLowerer;
            LipStretcher = engine.LipStretcher;
            BrowLowerer = engine.BrowLowerer;
            LipCornerDepressor = engine.LipCornerDepressor;
            BrowRaiser = engine.BrowRaiser;

            FacePositionX = engine.FacePosition.X;
            FacePositionY = engine.FacePosition.Y;
            FacePositionZ = engine.FacePosition.Z;

            FaceRotationX = engine.FaceRotation.X;
            FaceRotationY = engine.FaceRotation.Y;
            FaceRotationZ = engine.FaceRotation.Z;

            ShoulderCenterX = engine.ShoulderCenter.X;
            ShoulderCenterY = engine.ShoulderCenter.Y;
            ShoulderCenterZ = engine.ShoulderCenter.Z;

            if (game != null)
            {
                GameFrameNumber = game.FrameNumber;
                GameTicks = game.Time;
                GameMissed = game.Missed;
                GameMissclicks = game.Missclicks;
                GameScored = game.Scored;
                GameFailed = game.Failed;
                GameReactionTime = game.ReactionTime;
            }
        }
    }
}
