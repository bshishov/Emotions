using System.IO;
using Emotions.KinectTools;

namespace Emotions.Modules.Game
{
    public class GameFrame : IStreamable
    {
        public int FrameNumber { get; set; }

        public long Time { get; set; }

        /// <summary>
        /// Non-clicked circles
        /// </summary>
        public int Missed { get; set; }

        /// <summary>
        /// Clicks on the canvas
        /// </summary>
        public int Missclicks { get; set; }

        /// <summary>
        /// Right clicks
        /// </summary>
        public int Scored { get; set; }

        /// <summary>
        /// Clicks on wrong target
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// Time from creating circle to clicking on it
        /// </summary>
        public int ReactionTime { get; set; }

        public void ToStream(BinaryWriter writer)
        {
            writer.Write(FrameNumber);
            writer.Write(Time);
            writer.Write(Missed);
            writer.Write(Missclicks);
            writer.Write(Scored);
            writer.Write(Failed);
            writer.Write(ReactionTime);
        }

        public void FromStream(BinaryReader reader)
        {
            FrameNumber = reader.ReadInt32();
            Time = reader.ReadInt64();
            Missed = reader.ReadInt32();
            Missclicks = reader.ReadInt32();
            Scored = reader.ReadInt32();
            Failed = reader.ReadInt32();
            ReactionTime = reader.ReadInt32();
        }
    }
}
