using System;

namespace Emotions
{
    public class InputSnapshot
    {
        public struct Point3
        {
            public float X;
            public float Y;
            public float Z;
        }

        public DateTime DateTime = DateTime.Now;
        public double AU1;
        public double AU2;
        public double AU3;
        public double AU4;
        public double AU5;
        public double AU6;
        public Point3 FacePosition;
        public Point3 FaceRotation;
    }
}
