using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Rect = System.Windows.Rect;

namespace Emotions
{
    /// <summary>
    /// Interaction logic for KinectViewer.xaml
    /// </summary>
    public partial class KinectViewer : UserControl
    {
        public static readonly DependencyProperty KinectProperty = DependencyProperty.Register(
            "Kinect",
            typeof(KinectSensor),
            typeof(KinectViewer),
            new PropertyMetadata(
                null, (o, args) => ((KinectViewer)o).OnSensorChanged((KinectSensor)args.OldValue, (KinectSensor)args.NewValue)));

       
        public KinectSensor Kinect
        {
            get { return (KinectSensor) GetValue(KinectProperty); }
            set { SetValue(KinectProperty, value); }
        }

        private ColorImageFormat _currentColorImageFormat;
        private byte[] _colorImageData;
        private WriteableBitmap _colorImageWritableBitmap;
        private AdornerLayer _parentAdorner;
        private List<FaceDrawer> _drawers = new List<FaceDrawer>();

        public KinectViewer()
        {
            InitializeComponent();
            _parentAdorner = AdornerLayer.GetAdornerLayer(ColorImage);
        }

        private void OnSensorChanged(KinectSensor oldSensor, KinectSensor newSensor)
        {
            if (oldSensor != null)
                oldSensor.AllFramesReady -= this.OnAllFramesReady;

            if (newSensor != null)
                newSensor.AllFramesReady += this.OnAllFramesReady;
        }

        private void OnAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            DrawColorStream(e);
        }

        private void DrawColorStream(AllFramesReadyEventArgs e)
        {
            using (var colorImageFrame = e.OpenColorImageFrame())
            {
                if (colorImageFrame == null)
                {
                    return;
                }

                // Make a copy of the color frame for displaying.
                var haveNewFormat = ColorImageFormat.Undefined != colorImageFrame.Format;
                if (haveNewFormat)
                {
                    _currentColorImageFormat = colorImageFrame.Format;
                    _colorImageData = new byte[colorImageFrame.PixelDataLength];
                    _colorImageWritableBitmap = new WriteableBitmap(
                        colorImageFrame.Width, colorImageFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                    ColorImage.Source = _colorImageWritableBitmap;
                }

                colorImageFrame.CopyPixelDataTo(_colorImageData);
                _colorImageWritableBitmap.WritePixels(
                    new Int32Rect(0, 0, colorImageFrame.Width, colorImageFrame.Height),
                    this._colorImageData,
                    colorImageFrame.Width * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8),
                    0);
            }
        }

        public void TrackSkeleton(SkeletonFaceTracker skeletonFaceTracker)
        {
            var drawer = new FaceDrawer(ColorImage, skeletonFaceTracker);
            _drawers.Add(drawer);
            _parentAdorner.Add(drawer);
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
