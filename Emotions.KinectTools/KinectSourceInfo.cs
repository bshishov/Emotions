using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Kinect;

namespace Emotions.KinectTools
{
    public class KinectSourceInfo : IStreamable
    {
        public ColorImageFormat ColorImageFormat { get; private set; }
        public DepthImageFormat DepthImageFormat { get; private set; }
        public int ColorFrameWidth { get; private set; }
        public int ColorFrameHeight { get; private set; }
        public int DepthFrameWidth { get; private set; }
        public int DepthFrameHeight { get; private set; }
        public CoordinateMapper Mapper { get; private set; }
        public float ColorFocalLength { get; private set; }
        public float DepthFocalLength { get; private set; }
        
        public KinectSourceInfo(KinectSensor sensor)
        {
            ColorImageFormat = sensor.ColorStream.Format;
            ColorFrameWidth = sensor.ColorStream.FrameWidth;
            ColorFrameHeight = sensor.ColorStream.FrameHeight;
            ColorFocalLength = sensor.ColorStream.NominalFocalLengthInPixels;

            DepthImageFormat = sensor.DepthStream.Format;
            DepthFrameWidth = sensor.DepthStream.FrameWidth;
            DepthFrameHeight = sensor.DepthStream.FrameHeight;
            DepthFocalLength = sensor.DepthStream.NominalFocalLengthInPixels;

            Mapper = sensor.CoordinateMapper;
        }

        public KinectSourceInfo()
        {
            
        }

        public void ToStream(BinaryWriter writer)
        {
            writer.Write((int)ColorImageFormat);
            writer.Write(ColorFrameWidth);
            writer.Write(ColorFrameHeight);
            writer.Write(ColorFocalLength);

            writer.Write((int)DepthImageFormat);
            writer.Write(DepthFrameWidth);
            writer.Write(DepthFrameHeight);
            writer.Write(DepthFocalLength);
            
            var mapper = new Byte[Mapper.ColorToDepthRelationalParameters.Count];            
            Mapper.ColorToDepthRelationalParameters.CopyTo(mapper, 0);
            writer.Write(mapper.Length);
            writer.Write(mapper);
        }

        public void FromStream(BinaryReader reader)
        {
            ColorImageFormat = (ColorImageFormat)reader.ReadInt32();
            ColorFrameWidth = reader.ReadInt32();
            ColorFrameHeight = reader.ReadInt32();
            ColorFocalLength = reader.ReadSingle();

            DepthImageFormat = (DepthImageFormat)reader.ReadInt32();
            DepthFrameWidth = reader.ReadInt32();
            DepthFrameHeight = reader.ReadInt32();
            DepthFocalLength = reader.ReadSingle();

            var paramLenght = reader.ReadInt32();
            var parametersRaw = reader.ReadBytes(paramLenght);
            Mapper = new CoordinateMapper(parametersRaw);
        }
    }
}
