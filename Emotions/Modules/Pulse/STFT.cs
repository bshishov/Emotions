using System;
using System.Collections.Generic;
using System.Linq;
using AForge.Math;
using AForge.Math.Metrics;

namespace Emotions.Modules.Pulse
{
    /// <summary>
    /// Short time fourier transfrom
    /// </summary>
    public class STFT
    {
        private readonly double[] _signalBuffer;
        private readonly Complex[] _fft;
        private readonly int _windowSize;
        private readonly int _overlap;
        private int _idx;

        public int WindowSize { get { return _windowSize; } }

        public IEnumerable<double> FFTMagnitude { get { return _fft.Select(f => f.Magnitude); } }

        public STFT(int windowSize, int overlap)
        {
            _idx = -_overlap;
            _windowSize = windowSize;
            _overlap = overlap;
            _signalBuffer = new double[windowSize];
            _fft = new Complex[windowSize];
        }

        public void Proceed(double value)
        {
            for (var i = 0; i < _signalBuffer.Length - 1; i++)
                _signalBuffer[i] = _signalBuffer[i + 1];
            _signalBuffer[_signalBuffer.Length - 1] = value;

            if (_idx++ % (_windowSize - _overlap) == 0)
            {
                _idx = 0;
                for (var i = 0; i < _fft.Length; i++)
                    _fft[i] = new Complex(Hamming(i) * _signalBuffer[i], 0);
                AForge.Math.FourierTransform.FFT(_fft, FourierTransform.Direction.Forward);
            }
        }

        private double Hamming(double n)
        {
            return 0.54 - 0.46*Math.Cos(Math.PI*2*n/(_windowSize - 1));
        }

        /// <summary>
        /// Returns max amplitude frequency from FFT
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <returns>Frequency in Hz</returns>
        public double GetFreq(double sampleRate)
        {
            var maxi = 0;
            var max = Complex.Zero;
            for (var i = 5; i < _fft.Length / 2; i++)
            {
                if (_fft[i].Magnitude >= max.Magnitude)
                {
                    max = _fft[i];
                    maxi = i;
                }
            }

            return maxi * sampleRate / (double)_windowSize;
        }
    }
}
