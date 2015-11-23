using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Emotions.Controls;
using Emotions.KinectTools.Frames;
using Emotions.Modules.Pulse.Views;
using Emotions.Utilities;
using Gemini.Framework;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace Emotions.Modules.Pulse.ViewModels
{
    [Export(typeof(PulseViewModel))]
    class PulseViewModel : Document, IHandle<FramesContainer>, IHandle<FaceTrackFrame>
    {
        private const int ROIWidth = 20;
        private const int ROIHeight = 20;
        private readonly PulseDetector _detector;
        private readonly WriteableBitmap _amplifiedImage;
        private readonly WriteableBitmap _freqsImage;
        private readonly byte[] _freqsImageData;
        private RangedProperty _signalControl;

        public WriteableBitmap AmplifiedImage
        {
            get { return _amplifiedImage; }
        }

        public WriteableBitmap FreqsImage
        {
            get { return _freqsImage; }
        }

        public double Amplification
        {
            get { return _detector.Amplification; }
            set
            {
                _detector.Amplification = value;
                NotifyOfPropertyChange(()=>Amplification);
            }
        }

        public int BlurRadius
        {
            get { return _detector.BlurRadius; }
            set
            {
                _detector.BlurRadius = value;
                NotifyOfPropertyChange(() => BlurRadius);
            }
        }

        private double _freqsScale;

        public double FreqsScale
        {
            get { return _freqsScale; }
            set
            {
                _freqsScale = value;
                NotifyOfPropertyChange(() => FreqsScale);
            }
        }

        public double HeartRate { get; private set; }

        [ImportingConstructor]
        public PulseViewModel(IEventAggregator eventAggregator)
        {
            DisplayName = "Pulse";

            eventAggregator.Subscribe(this);
            
            _detector = new PulseDetector(ROIWidth, ROIHeight);
            _detector.SetRegionOfInteres(300,200);
            _amplifiedImage = new WriteableBitmap(ROIWidth, ROIHeight, 96, 96, PixelFormats.Bgr24, null);
            
            _freqsImage = new WriteableBitmap(300, 300, 96, 96, PixelFormats.Rgb24, null);
            _freqsImageData = new byte[(int)_freqsImage.Width * (int)_freqsImage.Height * (_freqsImage.Format.BitsPerPixel + 7) / 8];

            FreqsScale = 10;
        }
        
        private void DrawFreqs(WriteableBitmap image, double[] freqs)
        {
            var bpp = (image.Format.BitsPerPixel + 7) / 8; // bytes per pixel
            var bw = (int)(image.Width * bpp); // bytewidth

            // Shift
            for (var j = (int)image.Height - 1; j > 0 ; j--)
                for (var i = 0; i < bw; i++)
                    _freqsImageData[i + j*bw] = _freqsImageData[i + (j-1)*bw]; // copy prev row

            var len = (int)Math.Min(image.Width, freqs.Length);

            // apply the transformation
            for (var i = 0; i < len; i++)
            {
                var color = ColorScale.HeatMap2.Get(freqs[i] * _freqsScale);
                _freqsImageData[i * bpp + 0] = color.R;
                _freqsImageData[i * bpp + 1] = color.G;
                _freqsImageData[i * bpp + 2] = color.B;
            }

            image.WritePixels(new Int32Rect(0, 0, (int)image.Width, (int)image.Height), _freqsImageData, bw, 0);
        }
        
        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            var v = view as PulseView;
            if(v == null)
                return;
            _signalControl = v.HeartRateSignal;
        }

        public void Handle(FramesContainer framesContainer)
        {
            var frame = _detector.ProceedFrame(framesContainer.ColorFrame);

            _amplifiedImage.WritePixels(
                new Int32Rect(0, 0, (int)_amplifiedImage.Width, (int)_amplifiedImage.Height),
                frame.AmplifiedColorFrame,
                (int)_amplifiedImage.Width * ((_amplifiedImage.Format.BitsPerPixel + 7) / 8),
                0);

            if (_signalControl != null)
                _signalControl.Value = frame.Signal;

            DrawFreqs(_freqsImage, frame.FreqsData);
            HeartRate = frame.HeartRate;
            NotifyOfPropertyChange(() => HeartRate);
            NotifyOfPropertyChange(() => AmplifiedImage);
            NotifyOfPropertyChange(() => FreqsImage);
        }

        public void Handle(FaceTrackFrame frame)
        {
            var point = frame.GetProjected3DShape()[FeaturePoint.BelowThreeFourthRightEyelid];
            _detector.SetRegionOfInteres((int)point.X, (int)point.Y + 20);
        }
    }
}
