using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Kinect.Toolkit.FaceTracking;
using Point = System.Windows.Point;

namespace Emotions
{
    public class FaceDrawer : Adorner, IDisposable
    {
        private readonly SkeletonFaceTracker _tracker;
        private EnumIndexableCollection<FeaturePoint, PointF> _facePoints;
        private Brush _brush = Brushes.Cyan;
        private Pen _pen = null;
        private const float Radius = 1f;

        public SkeletonFaceTracker Tracker { get { return _tracker; } }

        public FaceDrawer(UIElement adornedElement, SkeletonFaceTracker tracker) : base(adornedElement)
        {
            _tracker = tracker;
            _tracker.TrackSucceed += TrackerOnTrackSucceed;
        }

        private void TrackerOnTrackSucceed(object sender, FaceTrackFrame frame)
        {
            _facePoints = frame.GetProjected3DShape();
            InvalidateVisual();
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            if (_facePoints != null)
            {
                foreach (var p in _facePoints)
                    drawingContext.DrawGeometry(_brush, _pen, new EllipseGeometry(new Point(p.X + 0.5f, p.Y + 0.5f), Radius, Radius));
            }
            base.OnRender(drawingContext);
        }

        public void Dispose()
        {
            _tracker.TrackSucceed -= TrackerOnTrackSucceed;
        }
    }
}