using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Emotions
{
    /// <summary>
    /// Interaction logic for RangedProperty.xaml
    /// </summary>
    public partial class RangedProperty : UserControl
    {
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption",
            typeof(string), typeof(RangedProperty), new PropertyMetadata(
                default(string), (o, args) => ((RangedProperty) o).OnCaptionChanged((string) args.OldValue, (string) args.NewValue)));

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

        private void OnDoubleSidedChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                var margin = ValueRect.Margin;
                ValueRect.Margin = new Thickness(margin.Left,margin.Top, this.ActualWidth / 2, margin.Bottom);
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(double),
            typeof(RangedProperty),
            new PropertyMetadata(
                default(double), (o, args) => ((RangedProperty)o).OnValueChanged((double)args.OldValue, (double)args.NewValue)));

        private void OnValueChanged(double oldValue, double newValue)
        {
            ValueLabel.Content = String.Format("{0:0.####}", newValue);
            UpdateValueRect();
        }

        public String Caption
        {
            get { return (String)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
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
