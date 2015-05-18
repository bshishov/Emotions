using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Emotions.Services.Engine
{
    [Serializable]
    public class EngineFrame : ICloneable
    {
        [Serializable]
        [DebuggerDisplay("({X},{Y},{Z})")]
        [StructLayout(LayoutKind.Sequential)]
        public struct Point3
        {
            public float X;
            public float Y;
            public float Z;
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
    }
}
