using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Emotions.KinectTools.Tracking;

namespace Emotions.Services.Engine
{
    [Serializable]
    class Recording
    {
        public const string NameFormat = "Recording-{0}.rec";

        public IEnumerable<EngineFrame> Frames { get { return _frames; } }
        private readonly List<EngineFrame> _frames;

        public Recording()
        {
            _frames = new List<EngineFrame>();
        }

        public static Recording FromFile(string path)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var obj = (Recording)formatter.Deserialize(stream);
            stream.Close();
            return obj;
        }

        public void Add(EngineFrame engineFrame)
        {
            _frames.Add(engineFrame);
        }

        public void Save(string path)
        {
            var formatter = new BinaryFormatter();
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, this);
            stream.Close();
        }
    }
}