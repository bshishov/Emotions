using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Kinect.Toolkit.FaceTracking;
using Newtonsoft.Json;

namespace Emotions.Services.Engine
{
    class TrainingDataSet
    {
        public struct Point3
        {
            public float X;
            public float Y;
            public float Z;
        }

        public class TrainingDataRow
        {
            public EmotionType EmotionType;
            public string Comment;
            public List<float> AUs;
            public DateTime DateTime;

            public TrainingDataRow() { }

            public TrainingDataRow(EmotionType emotionType, string comment, FaceTrackFrame frame)
            {
                EmotionType = emotionType;
                Comment = comment;
                AUs = new List<float>();
                var aus = frame.GetAnimationUnitCoefficients();
                
                foreach (var au in aus)
                    AUs.Add(au);

                DateTime = DateTime.Now;
            }

        }

        public List<TrainingDataRow> Rows = new List<TrainingDataRow>();

        public static TrainingDataSet FromFile(string path)
        {
            if(File.Exists(path))
                return JsonConvert.DeserializeObject<TrainingDataSet>(File.ReadAllText(path));
            else
                return new TrainingDataSet();
        }

        public void ToFile(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
