using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emotions.Controls;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;
using Emotions.Modules.Kinect;
using Emotions.Modules.Pulse.Views;
using Gemini.Framework;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace Emotions.Modules.Pulse.ViewModels
{
    [Export(typeof(PulseViewModel))]
    class PulseViewModel  : Document, IDisposable
    {
        private const int ROIWidth = 20;
        private const int ROIHeight = 20;
        private readonly IKinectSource _kinect;
        private readonly PulseDetector _detector;
        private readonly WriteableBitmap _amplifiedImage;
        private readonly WriteableBitmap _freqsImage;
        private byte[] _freqsImageData;
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

        public double HeartRate { get; private set; }

        [ImportingConstructor]
        public PulseViewModel(IKinectInputService source)
        {
            DisplayName = "Pulse";

            _kinect = source.GetKinect();
            _kinect.FramesReady += KinectOnFramesReady;
            if (_kinect.SkeletonFaceTracker != null)
                _kinect.SkeletonFaceTracker.TrackSucceed += SkeletonFaceTrackerOnTrackSucceed;

            _detector = new PulseDetector(100, ROIWidth, ROIHeight);
            _detector.SetRegionOfInteres(300,200);
            _amplifiedImage = new WriteableBitmap(ROIWidth, ROIHeight, 96, 96, PixelFormats.Bgr24, null);
            
            _freqsImage = new WriteableBitmap(300, 300, 96, 96, PixelFormats.Gray8, null);
            _freqsImageData = new byte[300 * 300];
        }

        private void SkeletonFaceTrackerOnTrackSucceed(object sender, FaceTrackFrame frame, Skeleton skeleton)
        {
            var point = frame.GetProjected3DShape()[FeaturePoint.BelowThreeFourthRightEyelid];
            //var x = frame.FaceRect.Left + (int)(0.2 * frame.FaceRect.Width);
            //var y = frame.FaceRect.Top + (int)(0.5 * frame.FaceRect.Height);
            _detector.SetRegionOfInteres((int)point.X, (int)point.Y + 20);
        }

        private void KinectOnFramesReady(IKinectSource kinectSource, FramesContainer framesContainer)
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

        private void DrawFreqs(WriteableBitmap image, double[] freqs)
        {
            for (var i = 0; i < _freqsImageData.Length; i++)
                _freqsImageData[i] = 255;

            // apply the transformation
            for (var i = 0; i < freqs.Length / 2; i++)
            {
                var x = i;
                var height = freqs[i] * 100 * 50;
                if (height > image.Height)
                    height = image.Height;

                for (int j = 0; j < height; j++)
                {
                    var y = (int)image.Height - j - 1;
                    var pos = y * (int)image.Width + x;
                    _freqsImageData[pos] = 0;
                }
            }

            image.WritePixels(
              new Int32Rect(0, 0, (int)image.Width, (int)image.Height), 
              _freqsImageData,
              (int)image.Width * ((image.Format.BitsPerPixel + 7) / 8),
              0);
        }

        public void Dispose()
        {
            _kinect.FramesReady -= KinectOnFramesReady;

            if(_kinect.SkeletonFaceTracker != null)
                _kinect.SkeletonFaceTracker.TrackSucceed -= SkeletonFaceTrackerOnTrackSucceed;
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            var v = view as PulseView;
            if(v == null)
                return;
            _signalControl = v.HeartRateSignal;
        }
    }
}
