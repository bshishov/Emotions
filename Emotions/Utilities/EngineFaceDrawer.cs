using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Caliburn.Micro;
using Emotions.Services.Engine;

namespace Emotions.Utilities
{
    class EngineFaceDrawer : Adorner, IDisposable
    {
        private Frame _frame;
        private readonly Brush _brush = Brushes.Red;
        private readonly Pen _pen = new Pen(Brushes.Red, 1);

        public EngineFaceDrawer(UIElement adornedElement) : base(adornedElement)
        {
            IoC.Get<IEngineService>().OnUpdate += EngineOnOnUpdate;
        }

        private void EngineOnOnUpdate(object sender, EngineUpdateEventArgs args)
        {
            _frame = args.Frame;
            Dispatcher.Invoke(InvalidateVisual);
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            if (_frame != null && _frame.FeaturePoints != null)
            {
                var faceModelGroup = new GeometryGroup();
                foreach (var point in _frame.FeaturePoints)
                {
                    faceModelGroup.Children.Add(new EllipseGeometry(Project(point), 1, 1));
                }
                drawingContext.DrawGeometry(_brush, _pen, faceModelGroup);
            }

            base.OnRender(drawingContext);
        }

        public void Dispose()
        {
            IoC.Get<IEngineService>().OnUpdate -= EngineOnOnUpdate;
        }

        private Point Project(Frame.Point3 point)
        {
            const double xf = 0.75f;
            const double yf = 1.1f;
            var x = point.X * (xf / point.Z);
            var y = point.Y * (yf / point.Z);

            return new Point(
                 x * ActualWidth + ActualWidth / 2, 
                -y * ActualHeight + ActualHeight / 2);
        }
    }
}
