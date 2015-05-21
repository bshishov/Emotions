using System.IO;
using Emotions.KinectTools;

namespace Emotions
{
    public class GameFrame : IStreamable
    {
        public int FrameNumber;

        public long Time;

        /// <summary>
        /// Non-clicked circles
        /// </summary>
        public int Missed;

        /// <summary>
        /// Clicks on the canvas
        /// </summary>
        public int Missclicks;

        /// <summary>
        /// Right clicks
        /// </summary>
        public int Scored;

        /// <summary>
        /// Clicks on wrong target
        /// </summary>
        public int Failed;

        /// <summary>
        /// Time from creating circle to clicking on it
        /// </summary>
        public int ReactionTime;

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
