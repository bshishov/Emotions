using System;
using System.Drawing;

namespace Emotions.Utilities
{
    class ColorScale
    {
        internal struct Pole
        {
            public double Position;
            public double Radius;
            public double R;
            public double G;
            public double B;

            public Pole(double position, double radius, int r, int g, int b)
            {
                Position = position;
                Radius = radius;
                R = r;
                G = g;
                B = b;
            }
        }

        private Pole[] _poles;

        public static ColorScale HeatMap = new ColorScale(
            new Pole(0.00, 0.3, 0, 0, 255),
            new Pole(0.33, 0.3, 0, 255, 255),
            new Pole(0.66, 0.3, 255, 255, 0),
            new Pole(1.00, 0.3, 255, 0, 0)
            );

        public static ColorScale HeatMap2 = new ColorScale(
            new Pole(0.00, 0.3, 0, 0, 0),
            new Pole(0.33, 0.3, 255, 0, 255),
            new Pole(0.66, 0.3, 255, 0, 0),
            new Pole(1.00, 0.3, 255, 255, 0)
            );

        public ColorScale(params Pole[] poles)
        {
            _poles = poles;
        }

        public Color Get(double val)
        {
            double r = 0, g = 0, b = 0, k = 0;
            if (val > 1.0)
                val = 1.0;

            foreach (var pole in _poles)
            {
                k = 1.0 - Math.Abs(val - pole.Position) / pole.Radius;
                if(k < 0)
                    continue;
                r += pole.R * k;
                g += pole.G * k;
                b += pole.B * k;
            }

            if (r > 255)
                r = 255;
            if (g > 255)
                g = 255;
            if (g > 255)
                g = 255;
            if (b > 255)
                b = 255;

            return Color.FromArgb((int)r, (int)g, (int)b);
        }
    }
}
