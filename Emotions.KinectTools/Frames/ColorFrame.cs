using System.IO;
using Microsoft.Kinect;

namespace Emotions.KinectTools.Frames
{
    public class ColorFrame : IFrame
    {
        public int FrameNumber { get; private set; }
        public long TimeStamp { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int BytesPerPixel { get; private set; }
        public ColorImageFormat Format { get; private set; }
        public int PixelDataLength { get; set; }
        public byte[] Data { get; private set; }

        public ColorFrame(ColorImageFrame frame)
        {
            Format = frame.Format;
            BytesPerPixel = frame.BytesPerPixel;
            FrameNumber = frame.FrameNumber;
            TimeStamp = frame.Timestamp;
            Width = frame.Width;
            Height = frame.Height;
            PixelDataLength = frame.PixelDataLength;
            Data = new byte[frame.PixelDataLength];
            frame.CopyPixelDataTo(Data);
        }

        public ColorFrame()
        {
            FrameNumber = 0;
        }


        public void ToStream(BinaryWriter writer)
        {
            writer.Write(FrameNumber);
            writer.Write(TimeStamp);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(BytesPerPixel);
            writer.Write((int) Format);
            writer.Write(PixelDataLength);
            writer.Write(Data);
        }

        public void FromStream(BinaryReader reader)
        {
            FrameNumber = reader.ReadInt32();
            TimeStamp = reader.ReadInt64();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            BytesPerPixel = reader.ReadInt32();
            Format = (ColorImageFormat)reader.ReadInt32();
            PixelDataLength = reader.ReadInt32();
            Data = reader.ReadBytes(PixelDataLength);
        }
      
    }
}
