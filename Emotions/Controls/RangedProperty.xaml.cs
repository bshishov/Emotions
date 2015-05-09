using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Emotions
{
    /// <summary>
    /// Interaction logic for RangedProperty.xaml
    /// </summary>
    public partial class RangedProperty : UserControl
    {
        class TimeLineDrawer
        {
            public WriteableBitmap Output { get { return _output; } }

            private WriteableBitmap _output;
            private const int Height = 100;
            private const int Width = 100;
            private int bytesPerPixel;
            private int stride;
            private int arraySize;
            private byte[] colorArray;
            private Int32Rect rect;
            private Random _random;

            public TimeLineDrawer()
            {
                _output = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgr32, null);
                bytesPerPixel = (_output.Format.BitsPerPixel + 7) / 8;
                stride = _output.PixelWidth * bytesPerPixel;
                arraySize = stride * _output.PixelHeight;
                colorArray = new byte[arraySize];
                for (var i = 0; i < arraySize; i++)
                    colorArray[i] = 255;
                rect = new Int32Rect(0, 0, _output.PixelWidth, _output.PixelHeight);
                _random = new Random();
            }

            public void Update(double value)
            {
                //Shift
                for (var i = 0; i < Width - 1; i++)
                    for (var j = 0; j < Height; j++)
                    {
                        colorArray[(i * Height + j)*bytesPerPixel + 0] =
                            colorArray[(i * Height + j + 1) * bytesPerPixel + 0];

                        colorArray[(i * Height + j) * bytesPerPixel + 1] =
                            colorArray[(i * Height + j + 1) * bytesPerPixel + 1];

                        colorArray[(i * Height + j) * bytesPerPixel + 2] =
                            colorArray[(i * Height + j + 1) * bytesPerPixel + 2];

                        colorArray[(i * Height + j) * bytesPerPixel + 3] =
                            colorArray[(i * Height + j + 1) * bytesPerPixel + 3];
                    }

                var lastCol = Width - 1;
                
                var pixelHeight = (int)Math.Floor((1 - value) * Height);
                for (var j = pixelHeight; j < Height; j++)
                {
                    colorArray[GetOffset(j, lastCol) + 0] = 0;
                    colorArray[GetOffset(j, lastCol) + 1] = 150;
                    colorArray[GetOffset(j, lastCol) + 2] = 0;
                    colorArray[GetOffset(j, lastCol) + 3] = 255;
                }

                _output.WritePixels(rect, colorArray, stride, 0);
            }

            private int GetOffset(int row, int col)
            {
                return (row * Height + col) * bytesPerPixel;
            }
        }

        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption",
            typeof(string), typeof(RangedProperty), new PropertyMetadata(
                default(string), (o, args) => ((RangedProperty) o).OnCaptionChanged((string) args.OldValue, (string) args.NewValue)));

        private readonly TimeLineDrawer _timeLineDrawer;

        private void OnCaptionChanged(string oldValue, string newValue)
        {
            CaptionLabel.Content = newValue;
        }

        public static readonly DependencyProperty MinProperty = DependencyProperty.Register("Min", typeof(double), typeof(RangedProperty), new PropertyMetadata(
                default(double), (o, args) => ((RangedProperty)o).OnMinChanged((double)args.OldValue, (double)args.NewValue)));

        private void OnMinChanged(double oldValue, double newValue)
        {
            TickBar.Minimum = 0;
            MinLabel.Content = String.Format("{0:0.####}", newValue);
            CenterLabel.Content = String.Format("{0:0.####}", (Max - Min) / 2);
            UpdateValueRect();
        }

        public static readonly DependencyProperty MaxProperty = DependencyProperty.Register("Max", typeof(double), typeof(RangedProperty), new PropertyMetadata(
                default(double), (o, args) => ((RangedProperty)o).OnMaxChanged((double)args.OldValue, (double)args.NewValue)));

        private void OnMaxChanged(double oldValue, double newValue)
        {
            TickBar.Maximum = 20;
            MaxLabel.Content = String.Format("{0:0.####}", newValue);
            CenterLabel.Content = String.Format("{0:0.####}", (Max-Min) / 2);
            UpdateValueRect();
        }

        public static readonly DependencyProperty DoubleSidedProperty = DependencyProperty.Register("DoubleSided", typeof(bool), typeof(RangedProperty), new PropertyMetadata(
                default(bool), (o, args) => ((RangedProperty)o).OnDoubleSidedChanged((bool)args.OldValue, (bool)args.NewValue)));

        private double _value;

        private void OnDoubleSidedChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                var margin = ValueRect.Margin;
                ValueRect.Margin = new Thickness(margin.Left,margin.Top, this.ActualWidth / 2, margin.Bottom);
            }
        }
        
        private void OnValueChanged(double oldValue, double newValue)
        {
            ValueLabel.Content = String.Format("{0:0.####}", newValue);
            UpdateTimeLine(newValue);
            UpdateValueRect();
        }

        public void SetValue(double value)
        {
            Value = value;
            ValueLabel.Content = String.Format("{0:0.####}", value);
            UpdateTimeLine(value);
            UpdateValueRect();
        }

        private void UpdateTimeLine(double value)
        {
            _timeLineDrawer.Update((value - Min) / (Max - Min));
            TimeLineImage.Source = _timeLineDrawer.Output;
        }

        public String Caption
        {
            get { return (String)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public double Value
        {
            get { return _value; }
            set
            {
                var old = _value;
                _value = value;
                OnValueChanged(old, value);
            }
        }

        public double Min
        {
            get { return (double)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }

        public double Max
        {
            get { return (double)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }

        public bool DoubleSisded
        {
            get { return (bool)GetValue(DoubleSidedProperty); }
            set { SetValue(DoubleSidedProperty, value); }
        }

        public RangedProperty()
        {
            InitializeComponent();
            _timeLineDrawer = new TimeLineDrawer();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateValueRect();
        }

        private void UpdateValueRect()
        {
            var w = (Value - Min) / (Max - Min);
            if (DoubleSisded)
            {
                var center = Min + (Max - Min) / 2.0;
                w = Math.Abs(0.5 - w);
                //w = (Math.Abs(Value - center) - Min) / (Max - Min);
                if (Value > center)
                {
                    ValueRect.Margin = new Thickness(ActualWidth * 0.5,0,0,0);
                }
                else
                {
                    ValueRect.Margin = new Thickness(ActualWidth * (0.5 - w), 0, 0, 0);
                }
            }

            if (w < 0) w = 0;
            if (w > 1) w = 1;
            ValueRect.Width = w * this.ActualWidth;  
        }
    }
}
