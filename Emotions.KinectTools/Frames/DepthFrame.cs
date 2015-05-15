using System.IO;
using Microsoft.Kinect;

namespace Emotions.KinectTools.Frames
{
    public class DepthFrame: IFrame
    {
        public int FrameNumber { get; private set; }
        public long TimeStamp { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int BytesPerPixel { get; private set; }
        public DepthImageFormat Format { get; private set; }
        public int PixelDataLength { get; set; }
        public short[] Data { get; private set; }

        public DepthFrame(DepthImageFrame frame)
        {
            Format = frame.Format;
            BytesPerPixel = frame.BytesPerPixel;
            FrameNumber = frame.FrameNumber;
            TimeStamp = frame.Timestamp;
            Width = frame.Width;
            Height = frame.Height;
            PixelDataLength = frame.PixelDataLength;
            Data = new short[frame.PixelDataLength];
            frame.CopyPixelDataTo(Data);
        }

        public DepthFrame()
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

            foreach (short val in Data)
                writer.Write(val);
        }

        public void FromStream(BinaryReader reader)
        {
            FrameNumber = reader.ReadInt32();
            TimeStamp = reader.ReadInt64();
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            BytesPerPixel = reader.ReadInt32();
            Format = (DepthImageFormat)reader.ReadInt32();
            PixelDataLength = reader.ReadInt32();
            Data = new short[PixelDataLength];
            for (var index = 0; index < PixelDataLength; index++)
                Data[index] = reader.ReadInt16();
        }
    }
}
