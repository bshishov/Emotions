using System.IO;
using System.Windows;
using Emotions.KinectTools;

namespace Emotions.Modules.Pulse
{
    public class PulseFrame : IStreamable
    {
        public double HeartRate { get; set; }
        public double Signal { get; set; }
        public Rect RegionOfInterest { get; set; }
        public byte[] AmplifiedColorFrame { get; set; }
        public double[] FreqsData { get; set; }

        public void ToStream(BinaryWriter writer)
        {
            writer.Write(HeartRate);
            writer.Write(Signal);
            writer.Write(RegionOfInterest.X);
            writer.Write(RegionOfInterest.Y);
            writer.Write(RegionOfInterest.Width);
            writer.Write(RegionOfInterest.Height);
        }

        public void FromStream(BinaryReader reader)
        {
            HeartRate = reader.ReadDouble();
            Signal = reader.ReadDouble();

            RegionOfInterest = new Rect(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }
    }
}
