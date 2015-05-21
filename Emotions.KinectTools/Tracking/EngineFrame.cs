using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace Emotions.KinectTools.Tracking
{
    [Serializable]
    public class EngineFrame : IStreamable, ICloneable
    {
        [Serializable]
        [DebuggerDisplay("({X},{Y},{Z})")]
        [StructLayout(LayoutKind.Sequential)]
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
                X = reader.ReadSingle();
                Y = reader.ReadSingle();
                Z = reader.ReadSingle();
            }
        }

        public TimeSpan Time;

        /// <summary>
        /// FACS AU 10 
        /// </summary>
        public double LipRaiser; 

        /// <summary>
        /// FACS AU 26
        /// </summary>
        public double JawLowerer;

        /// <summary>
        /// FACS AU 20
        /// </summary>
        public double LipStretcher;

        /// <summary>
        /// FACS AU 4
        /// </summary>
        public double BrowLowerer;

        /// <summary>
        /// FACS AU 15
        /// </summary>
        public double LipCornerDepressor;

        /// <summary>
        /// FACS AU 2
        /// </summary>
        public double BrowRaiser;

        public Point3 FacePosition; 
        public Point3 FaceRotation;
        public Point3[] FeaturePoints;

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

            FacePosition = new Point3();
            FacePosition.FromStream(reader);
            
            FaceRotation = new Point3();
            FaceRotation.FromStream(reader);
            
            var len = reader.ReadInt32();
            if (len > 0)
            {
                FeaturePoints = new Point3[len];
                for (var i = 0; i < len; i++)
                    FeaturePoints[i].FromStream(reader);
            }
        }

        public static EngineFrame FromFaceTrackFrame(FaceTrackFrame faceFrame, Skeleton skeleton)
        {
            var au = faceFrame.GetAnimationUnitCoefficients();
            var featurepoints = faceFrame.Get3DShape();
            var points = new Point3[featurepoints.Count];
            for (var i = 0; i < featurepoints.Count; i++)
            {
                points[i] = new EngineFrame.Point3()
                {
                    X = featurepoints[i].X,
                    Y = featurepoints[i].Y,
                    Z = featurepoints[i].Z
                };
            }

            var frame = new EngineFrame()
            {
                FeaturePoints = points,
                LipRaiser = au[AnimationUnit.LipRaiser],
                JawLowerer = au[AnimationUnit.JawLower],
                LipStretcher = au[AnimationUnit.LipStretcher],
                BrowLowerer = au[AnimationUnit.BrowLower],
                LipCornerDepressor = au[AnimationUnit.LipCornerDepressor],
                BrowRaiser = au[AnimationUnit.BrowRaiser],
                FacePosition = new EngineFrame.Point3()
                {
                    X = faceFrame.Translation.X,
                    Y = faceFrame.Translation.Y,
                    Z = faceFrame.Translation.Z,
                },
                FaceRotation = new EngineFrame.Point3()
                {
                    X = faceFrame.Rotation.X,
                    Y = faceFrame.Rotation.Y,
                    Z = faceFrame.Rotation.Z,
                },
            };
            return frame;
        }
    }
}
