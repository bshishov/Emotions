using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Emotions.Services.Engine
{
    [Serializable]
    public class Frame : ICloneable
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
        public double AU1;
        public double AU2;
        public double AU3;
        public double AU4;
        public double AU5;
        public double AU6;
        public Point3 FacePosition;
        public Point3 FaceRotation;
        public Point3[] FeaturePoints;

        public object Clone()
        {
            // TODO improve
            return this.MemberwiseClone();
        }
    }
}
