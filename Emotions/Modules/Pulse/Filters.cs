namespace Emotions.Modules.Pulse
{
    interface IFilter
    {
        double Filter(double val);
    }

    /// <summary>
    /// Created using http://www-users.cs.york.ac.uk/~fisher/mkfilter/trad.html
    /// </summary>
    class Bandpass1Hzto4Hz : IFilter
    {
        private const int NZEROS = 4;
        private const int NPOLES = 4;
        private static double GAIN = 2.974075024e+01;

        private readonly double[] xv = new double[NZEROS + 1];
        private readonly double[] yv = new double[NPOLES + 1];

        public double Filter(double val)
        {
            xv[0] = xv[1]; xv[1] = xv[2]; xv[2] = xv[3]; xv[3] = xv[4]; 
            xv[4] = val / GAIN;
            yv[0] = yv[1]; yv[1] = yv[2]; yv[2] = yv[3]; yv[3] = yv[4]; 
            yv[4] =   (xv[0] + xv[4]) - 2 * xv[2]
                         + ( -0.5532698897 * yv[0]) + (  2.3587233091 * yv[1])
                         + ( -4.0115929650 * yv[2]) + (  3.1931745979 * yv[3]);
            return yv[4];
        }
    }

    /// <summary>
    /// Created using http://www-users.cs.york.ac.uk/~fisher/mkfilter/trad.html
    /// </summary>
    class LowpassFilter4Hz : IFilter
    {
        private const int NZEROS = 2;
        private const int NPOLES = 2;
        private static double GAIN = 1.596581364e+02;

        private readonly double[] xv = new double[NZEROS + 1];
        private readonly double[] yv = new double[NPOLES + 1];

        public double Filter(double val)
        {
            xv[0] = xv[1]; xv[1] = xv[2]; 
            xv[2] = val / GAIN;
            yv[0] = yv[1]; yv[1] = yv[2]; 
            yv[2] =   (xv[0] + xv[2]) + 2 * xv[1]
                         + ( -0.7890314828 * yv[0]) + (  1.7639779523 * yv[1]);
            return yv[2];
        }
    }
}
