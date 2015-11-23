using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace Emotions.KinectTools.Tracking
{
    public class EngineInputFrame : IStreamable, ICloneable
    {
        [DebuggerDisplay("({X},{Y},{Z})")]
        public struct Point3 : IStreamable
        {
            public float X;
            public float Y;
            public float Z;

            public void ToStream(BinaryWriter writer)
            {
                writer.Write(X);
                writer.Write(Y);
                writer.Write(Z);
            }

            public void FromStream(BinaryReader reader)
            {
                this.X = reader.ReadSingle();
                this.Y = reader.ReadSingle();
                this.Z = reader.ReadSingle();
            }

            public override string ToString()
            {
                return string.Format("({0}, {1}, {2})", X, Y, Z);
            }
        }

        public TimeSpan Time { get; set; }

        /// <summary>
        /// FACS AU 10 
        /// </summary>
        public double LipRaiser { get; set; }

        /// <summary>
        /// FACS AU 26
        /// </summary>
        public double JawLowerer { get; set; }

        /// <summary>
        /// FACS AU 20
        /// </summary>
        public double LipStretcher { get; set; }

        /// <summary>
        /// FACS AU 4
        /// </summary>
        public double BrowLowerer { get; set; }

        /// <summary>
        /// FACS AU 15
        /// </summary>
        public double LipCornerDepressor { get; set; }

        /// <summary>
        /// FACS AU 2
        /// </summary>
        public double BrowRaiser { get; set; }

        public Point3 FacePosition { get; set; }
        public Point3 FaceRotation { get; set; }
        public Point3[] FeaturePoints { get; set; }
        public Point3 HeadPosition { get; set; }
        public Point3 ShoulderCenter { get; set; }
        public Point3 ShoulderLeft { get; set; }
        public Point3 ShoulderRight { get; set; }
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public void ToStream(BinaryWriter writer)
        {
            writer.Write(Time.TotalMilliseconds);

            writer.Write(LipRaiser);
            writer.Write(JawLowerer);
            writer.Write(LipStretcher);
            writer.Write(BrowLowerer);
            writer.Write(LipCornerDepressor);
            writer.Write(BrowRaiser);

            FacePosition.ToStream(writer);
            FaceRotation.ToStream(writer);

            HeadPosition.ToStream(writer);
            ShoulderCenter.ToStream(writer);
            ShoulderLeft.ToStream(writer);
            ShoulderRight.ToStream(writer);

            if(FeaturePoints == null)
                writer.Write(0);
            else
            {
                writer.Write(FeaturePoints.Length);
                foreach (var featurePoint in FeaturePoints)
                    featurePoint.ToStream(writer);    
            }
        }

        public void FromStream(BinaryReader reader)
        {
            Time = TimeSpan.FromMilliseconds(reader.ReadDouble());

            LipRaiser = reader.ReadDouble();
            JawLowerer = reader.ReadDouble();
            LipStretcher = reader.ReadDouble();
            BrowLowerer = reader.ReadDouble();
            LipCornerDepressor = reader.ReadDouble();
            BrowRaiser = reader.ReadDouble();

            var p = new Point3();
            p.FromStream(reader);
            FacePosition = p;

            p = new Point3();
            p.FromStream(reader);
            FaceRotation = p;

            p = new Point3();
            p.FromStream(reader);
            HeadPosition = p;

            p = new Point3();
            p.FromStream(reader);
            ShoulderCenter = p;

            p = new Point3();
            p.FromStream(reader);
            ShoulderLeft = p;

            p = new Point3();
            p.FromStream(reader);
            ShoulderRight = p;
            
            var len = reader.ReadInt32();
            if (len > 0 && len < 200)
            {
                FeaturePoints = new Point3[len];
                for (var i = 0; i < len; i++)
                    FeaturePoints[i].FromStream(reader);
            }
        }

        public static EngineInputFrame FromFaceTrackFrame(FaceTrackFrame faceFrame, Skeleton skeleton)
        {
            var au = faceFrame.GetAnimationUnitCoefficients();
            var featurepoints = faceFrame.Get3DShape();
            var points = new Point3[featurepoints.Count];
            for (var i = 0; i < featurepoints.Count; i++)
            {
                points[i] = new EngineInputFrame.Point3()
                {
                    X = featurepoints[i].X,
                    Y = featurepoints[i].Y,
                    Z = featurepoints[i].Z
                };
            }

            var frame = new EngineInputFrame()
            {
                FeaturePoints = points,
                LipRaiser = au[AnimationUnit.LipRaiser],
                JawLowerer = au[AnimationUnit.JawLower],
                LipStretcher = au[AnimationUnit.LipStretcher],
                BrowLowerer = au[AnimationUnit.BrowLower],
                LipCornerDepressor = au[AnimationUnit.LipCornerDepressor],
                BrowRaiser = au[AnimationUnit.BrowRaiser],
                FacePosition = new Point3()
                {
                    X = faceFrame.Translation.X,
                    Y = faceFrame.Translation.Y,
                    Z = faceFrame.Translation.Z,
                },
                FaceRotation = new Point3()
                {
                    X = faceFrame.Rotation.X,
                    Y = faceFrame.Rotation.Y,
                    Z = faceFrame.Rotation.Z,
                },
            };

            var hpos = skeleton.Joints[JointType.Head].Position;
            frame.HeadPosition = new Point3() { X = hpos.X, Y = hpos.Y, Z = hpos.Z };

            hpos = skeleton.Joints[JointType.ShoulderCenter].Position;
            frame.ShoulderCenter = new Point3() { X = hpos.X, Y = hpos.Y, Z = hpos.Z };

            hpos = skeleton.Joints[JointType.ShoulderLeft].Position;
            frame.ShoulderLeft = new Point3() { X = hpos.X, Y = hpos.Y, Z = hpos.Z };

            hpos = skeleton.Joints[JointType.ShoulderRight].Position;
            frame.ShoulderRight = new Point3() { X = hpos.X, Y = hpos.Y, Z = hpos.Z };

            return frame;
        }
    }
}
