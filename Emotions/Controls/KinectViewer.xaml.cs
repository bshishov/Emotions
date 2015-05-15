using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emotions.KinectTools;
using Emotions.Services.KinectInput;
using Emotions.Utilities;
using Microsoft.Kinect;

namespace Emotions
{
    /// <summary>
    /// Interaction logic for KinectViewer.xaml
    /// </summary>
    public partial class KinectViewer : UserControl
    {
        public static readonly DependencyProperty KinectProperty = DependencyProperty.Register(
            "Kinect",
            typeof(IKinectSource),
            typeof(KinectViewer),
            new PropertyMetadata(
                null, (o, args) => ((KinectViewer)o).OnSensorChanged((IKinectSource)args.OldValue, (IKinectSource)args.NewValue)));

       
        public IKinectSource Kinect
        {
            get { return (IKinectSource)GetValue(KinectProperty); }
            set { SetValue(KinectProperty, value); }
        }

        private ColorImageFormat _currentColorImageFormat;
        private WriteableBitmap _colorImageWritableBitmap;
        private AdornerLayer _parentAdorner;
        private List<KinectDrawer> _drawers = new List<KinectDrawer>();

        public KinectViewer()
        {
            InitializeComponent();
            _parentAdorner = AdornerLayer.GetAdornerLayer(ColorImage);
            _parentAdorner.Add(new EngineDrawer(ColorImage));
            ColorImage.Source = new WriteableBitmap(640, 480, 96, 96, PixelFormats.Bgr32, null);
        }

        private void OnSensorChanged(IKinectSource oldSensor, IKinectSource newSensor)
        {
            if (oldSensor != null)
                oldSensor.FramesReady -= OnAllFramesReady;

            if (newSensor != null)
                newSensor.FramesReady += OnAllFramesReady;
        }

        private void OnAllFramesReady(object sender, FramesReadyEventArgs framesReadyEventArgs)
        {
            Dispatcher.Invoke(() => DrawColorStream((IKinectSource)sender, framesReadyEventArgs));
        }

        private void DrawColorStream(IKinectSource source, FramesReadyEventArgs e)
        {
            if(e.ColorFrame.Data == null)
                return;

            // Make a copy of the color frame for displaying.
            var haveNewFormat = ColorImageFormat.Undefined != source.Info.ColorImageFormat;
            if (haveNewFormat)
            {
                _currentColorImageFormat = source.Info.ColorImageFormat;
                _colorImageWritableBitmap = new WriteableBitmap(
                    source.Info.ColorFrameWidth, source.Info.ColorFrameHeight, 96, 96, PixelFormats.Bgr32, null);
                ColorImage.Source = _colorImageWritableBitmap;
            }

            _colorImageWritableBitmap.WritePixels(
                new Int32Rect(0, 0, source.Info.ColorFrameWidth, source.Info.ColorFrameHeight),
                e.ColorFrame.Data,
                source.Info.ColorFrameWidth * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8),
                0);
        }

        public void TrackSkeleton(SkeletonFaceTracker skeletonFaceTracker)
        {
            Dispatcher.Invoke(() =>
            {
                var drawer = new KinectDrawer(ColorImage, skeletonFaceTracker);
                _drawers.Add(drawer);
                _parentAdorner.Add(drawer);
            });
        }

        public void UnTrackSkeleton(SkeletonFaceTracker skeletonFaceTracker)
        {
            var drawer = _drawers.FirstOrDefault(d => d.Tracker.Equals(skeletonFaceTracker));
            if(drawer != null)
                _parentAdorner.Remove(drawer);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(ColorImage != null)
                ColorImage.Opacity = (e.NewValue/100);
        }

        private void ShowColorChecked(object sender, RoutedEventArgs e)
        {
            if (ColorImage != null)
                ColorImage.Visibility = Visibility.Visible;
        }

        private void ShowFaceChecked(object sender, RoutedEventArgs e)
        {
            if(_parentAdorner != null)
                _parentAdorner.Visibility = Visibility.Visible;
        }

        private void ShowColorUnChecked(object sender, RoutedEventArgs e)
        {
            if (ColorImage != null)
                ColorImage.Visibility = Visibility.Hidden;
        }

        private void ShowFaceUnChecked(object sender, RoutedEventArgs e)
        {
            if (_parentAdorner != null)
                _parentAdorner.Visibility = Visibility.Hidden;
        }
    }
}
