using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;

namespace Emotions.Controls
{
    public enum Modes
    {
        None,
        Color,
        Depth
    }

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

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
            "Mode",
            typeof(Modes),
            typeof(KinectViewer),
            new PropertyMetadata(
                Modes.Color, (o, args) => ((KinectViewer)o).OnModeChanged((Modes)args.OldValue, (Modes)args.NewValue)));

        private void OnModeChanged(Modes oldValue, Modes newValue)
        {
            _cachedMode = newValue;
            _writableBitmap = new WriteableBitmap(10, 10, 96, 96, PixelFormats.Bgr32, null);
            ColorImage.Source = _writableBitmap;
        }

        public IKinectSource Kinect
        {
            get { return (IKinectSource)GetValue(KinectProperty); }
            set { SetValue(KinectProperty, value); }
        }

        public Modes Mode
        {
            get { return (Modes)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }
        
        private WriteableBitmap _writableBitmap;
        private readonly Action<IKinectSource, FramesContainer> _drawColorDelegate;
        private readonly Action<IKinectSource, FramesContainer> _drawDepthDelegate;
        private Modes _cachedMode = Modes.Color;

        public KinectViewer()
        {
            InitializeComponent();
            _writableBitmap = new WriteableBitmap(640, 480, 96, 96, PixelFormats.Bgr32, null);
            ColorImage.Source = _writableBitmap;
            _drawColorDelegate = DrawColorStream;
            _drawDepthDelegate = DrawDepthStream;
        }

        private void OnSensorChanged(IKinectSource oldSensor, IKinectSource newSensor)
        {
            if (oldSensor != null)
                oldSensor.FramesReady -= OnFramesReady;

            if (newSensor != null)
                newSensor.FramesReady += OnFramesReady;
        }

        private void OnFramesReady(object sender, FramesContainer framesContainer)
        {
            switch (_cachedMode)
            {
                case Modes.Color:
                    Dispatcher.Invoke(_drawColorDelegate, sender as IKinectSource, framesContainer);
                    break;
                case Modes.Depth:
                    Dispatcher.Invoke(_drawDepthDelegate, sender as IKinectSource, framesContainer);
                    break;
            }
        }

        private void DrawColorStream(IKinectSource source, FramesContainer e)
        {
            if (source == null || e.ColorFrame.Data == null)
                return;

            if (_writableBitmap.PixelWidth != source.Info.ColorFrameWidth ||
                _writableBitmap.PixelHeight != source.Info.ColorFrameHeight)
            {
                _writableBitmap = new WriteableBitmap(
                    source.Info.ColorFrameWidth, 
                    source.Info.ColorFrameHeight, 
                    96, 96, PixelFormats.Bgr32, null);
                ColorImage.Source = _writableBitmap;
            }

            _writableBitmap.WritePixels(
                new Int32Rect(0, 0, source.Info.ColorFrameWidth, source.Info.ColorFrameHeight),
                e.ColorFrame.Data,
                source.Info.ColorFrameWidth * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8),
                0);
        }

        private void DrawDepthStream(IKinectSource source, FramesContainer e)
        {
            if (source == null || e.ColorFrame.Data == null)
                return;

            if (_writableBitmap.PixelWidth != source.Info.DepthFrameWidth ||
                _writableBitmap.PixelHeight != source.Info.DepthFrameHeight)
            {
                _writableBitmap = new WriteableBitmap(
                    source.Info.DepthFrameWidth,
                    source.Info.DepthFrameHeight,
                    96, 96, PixelFormats.Rgb24, null);
                ColorImage.Source = _writableBitmap;
            }

            _writableBitmap.WritePixels(
                new Int32Rect(0, 0, source.Info.DepthFrameWidth, source.Info.DepthFrameHeight),
                e.DepthFrame.GetRgb24Bytes(),
                source.Info.DepthFrameWidth * ((PixelFormats.Rgb24.BitsPerPixel + 7) / 8),
                0);
        }
    }
}
