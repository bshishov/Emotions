using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Kinect;

namespace Emotions.KinectTools.Frames
{
    public class SkeletonFrame : IFrame
    {
        public int FrameNumber { get; private set; }
        public long TimeStamp { get; private set; }
        public Tuple<float, float, float, float> FloorClipPlane { get; private set; }
        public Skeleton[] Skeletons { get; private set; }
        public SkeletonTrackingMode TrackingMode { get; set; }

        private readonly BinaryFormatter _formatter;

        public SkeletonFrame(Microsoft.Kinect.SkeletonFrame frame)
        {
            FloorClipPlane = frame.FloorClipPlane;
            FrameNumber = frame.FrameNumber;
            TimeStamp = frame.Timestamp;
            TrackingMode = frame.TrackingMode;

            Skeletons = new Skeleton[frame.SkeletonArrayLength];
            frame.CopySkeletonDataTo(Skeletons);

            _formatter = new BinaryFormatter();
        }

        public SkeletonFrame(int frame, long time)
        {
            FrameNumber = frame;
            TimeStamp = time;
            _formatter = new BinaryFormatter();
        }


        public void ToStream(BinaryWriter writer)
        {
            writer.Write(TimeStamp);
            writer.Write((int)TrackingMode);
            writer.Write(FloorClipPlane.Item1);
            writer.Write(FloorClipPlane.Item2);
            writer.Write(FloorClipPlane.Item3);
            writer.Write(FloorClipPlane.Item4);
            writer.Write(FrameNumber);
            
            _formatter.Serialize(writer.BaseStream, Skeletons);
        }

        public void FromStream(BinaryReader reader)
        {
            //TimeStamp = reader.ReadInt64();
            var dumb1 = reader.ReadInt64();
            TrackingMode = (SkeletonTrackingMode)reader.ReadInt32();
            FloorClipPlane = new Tuple<float, float, float, float>(
                reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle());

            //FrameNumber = reader.ReadInt32();
            var dumb2 = reader.ReadInt32();
            Skeletons = (Skeleton[])_formatter.Deserialize(reader.BaseStream);
        }
    }
}
