using System;
using System.Linq;
using System.Windows;
using Emotions.KinectTools.Frames;
using Emotions.Utilities;

namespace Emotions.Modules.Pulse
{
    class PulseDetector
    {
        public double Amplification { get; set; }
        public int BlurRadius { get; set; }
        public double CutOff { get; set; }
        public double Signal { get { return _signal; } }
        
        private readonly IFilter[] _hgfilters;
        private readonly IFilter[] _lgfilters;
        private readonly byte[] _imageData;
        private readonly double[] _red;
        private readonly double[] _green;
        private readonly double[] _blue;

        private Rect _roi;
        private readonly STFT _pulser = new STFT(256, 255);
        private double _signal;

        public PulseDetector(double amplification, int width, int height)
        {
            _roi = new Rect(0, 0, width, height);
            CutOff = 1000;
            BlurRadius = 5;

            _red = new double[width * height];
            _green = new double[width * height];
            _blue = new double[width * height];
            _imageData = new byte[width * height * 3];
            
            _hgfilters = new IFilter[_red.Length];
            _lgfilters = new IFilter[_red.Length];
            

            for (var i = 0; i < _red.Length; i++)
            {
                _hgfilters[i] = new Bandpass1Hzto4Hz();
                _lgfilters[i] = new LowpassFilter4Hz();
            }

            Amplification = amplification;
        }

        public void SetRegionOfInteres(int x, int y)
        {
            _roi.X = x - _roi.Width / 2;
            _roi.Y = y - _roi.Height / 2;
        }

        public PulseFrame ProceedFrame(ColorFrame source)
        {
            BoxBlur.BoxBlurRgb(source, (int)_roi.X, (int)_roi.Y, (int)_roi.Width, (int)_roi.Height, BlurRadius, 2);
            ToIntensity(source, _roi, _red, _green, _blue);
            _signal = 0.0;
            for (var i = 0; i < _red.Length; i++)
            {
                //var gfiltered = _hgfilters[i].Filter(_lgfilters[i].Filter(_green[i]));
                var gfiltered = _hgfilters[i].Filter(_green[i]);
                _green[i] += Math.Min((1 + Amplification) * gfiltered, CutOff / 8.0);
                _signal += _green[i] / _red.Length;
            }
            _pulser.Proceed(_signal); 

            FromIntensity(_red, _green, _blue, _imageData);
            return new PulseFrame()
            {
                HeartRate = GetBpm(), 
                Signal = _signal, 
                FreqsData = _pulser.FFTMagnitude.ToArray(),
                RegionOfInterest = _roi,
                AmplifiedColorFrame = _imageData
            };
        }

        private static void FromIntensity(double[] r, double[] g, double[] b, byte[] image)
        {
            for (var i = 0; i < r.Length; i++)
            {
                image[i * 3 + 0] = (byte)(Math.Max(0, Math.Min(255, r[i] * 255.0)));
                image[i * 3 + 1] = (byte)(Math.Max(0, Math.Min(255, g[i] * 255.0)));
                image[i * 3 + 2] = (byte)(Math.Max(0, Math.Min(255, b[i] * 255.0)));
            }
        }

        private static void ToIntensity(ColorFrame image, Rect roi, double[] r, double[] g, double[] b)
        {
            // apply the transformation
            for (var i = 0; i < r.Length; i++)
            {
                var x = (int)roi.X + i % (int)roi.Width;
                var y = (int)roi.Y + i / (int)roi.Width;
                var offset = image.GetOffset(x,y);
                r[i] = image.Data[offset + 0] / 255.0;
                g[i] = image.Data[offset + 1] / 255.0;
                b[i] = image.Data[offset + 2] / 255.0;
            }
        }

        public double GetBpm()
        {
            return _pulser.GetFreq(30) * 60;
        }
    }
}
