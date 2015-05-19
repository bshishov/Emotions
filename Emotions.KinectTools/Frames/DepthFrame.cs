using System;
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

        public DepthFrame(byte[] data, DepthImageFormat format, int frameNumber, long timeStamp)
        {
            Data = FromRgb24(data);
            Format = format;
            FrameNumber = frameNumber;
            TimeStamp = timeStamp;

            switch (format)
            {
                case DepthImageFormat.Resolution320x240Fps30:
                    Width = 320;
                    Height = 240;
                    BytesPerPixel = 2;
                    break;
                case DepthImageFormat.Resolution640x480Fps30:
                    Width = 640;
                    Height = 480;
                    BytesPerPixel = 2;
                    break;
                case DepthImageFormat.Resolution80x60Fps30:
                    Width = 80;
                    Height = 60;
                    BytesPerPixel = 2;
                    break;
                case DepthImageFormat.Undefined:
                    throw new Exception("Format is not defined");
                default:
                    throw new Exception("Unsupported");
            }

            PixelDataLength = Width * Height * BytesPerPixel / 2;

            if (Data.Length != PixelDataLength)
                throw new Exception("Data.Length != PixelDataLength.");
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

        public byte[] GetRgb16Bytes()
        {
            var bytes = new byte[Data.Length * 2];
            for (var i = 0; i < bytes.Length; i += 2)
            {
                bytes[i] = (byte)((Data[i / 3] & 0xFF00) >> 8);
                bytes[i + 1] = (byte)((Data[i / 3] & 0x00FF));
            }

            return bytes;
        }

        public byte[] GetRgb24Bytes()
        {
            var bytes = new byte[Data.Length * 3];
            for (var i = 0; i < bytes.Length; i += 3)
            {
                bytes[i] = (byte)((Data[i / 3] & 0xFF00) >> 8);
                bytes[i + 1] = (byte)((Data[i / 3] & 0x00FF));
                bytes[i + 2] = (byte)((Data[i / 3] & 0x00FF));
            }

            return bytes;
        }

        public short[] FromRgb24(byte[] bytes)
        {
            var lenght = bytes.Length/3;
            var shorts = new short[lenght];
            for (var i = 0; i < lenght; i++)
            {
                shorts[i] = (short)((short)(bytes[3 * i] << 8) + (short)(bytes[3 * i + 1]));
            }
            return shorts;
        }
    }
}
