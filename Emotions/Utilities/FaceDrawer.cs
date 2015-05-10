using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Emotions.Services.KinectInput;
using Microsoft.Kinect.Toolkit.FaceTracking;
using Point = System.Windows.Point;

namespace Emotions.Utilities
{
    public class FaceDrawer : Adorner, IDisposable
    {
        private readonly SkeletonFaceTracker _tracker;
        private EnumIndexableCollection<FeaturePoint, PointF> _facePoints;
        private FaceTriangle[] _triangles;
        private readonly Brush _brush = Brushes.Cyan;
        private readonly Pen _pen = new Pen(Brushes.Cyan, 1);
        
        public SkeletonFaceTracker Tracker { get { return _tracker; } }

        public FaceDrawer(UIElement adornedElement, SkeletonFaceTracker tracker) : base(adornedElement)
        {
            _tracker = tracker;
            _tracker.TrackSucceed += TrackerOnTrackSucceed;
        }

        private void TrackerOnTrackSucceed(object sender, FaceTrackFrame frame)
        {
            _facePoints = frame.GetProjected3DShape();
            _triangles = frame.GetTriangles();
            InvalidateVisual();
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            if (_facePoints != null)
            {
                var faceModelGroup = new GeometryGroup();
                foreach (var faceTriangle in _triangles)
                {
                    var p1R = _facePoints[faceTriangle.First];
                    var p2R = _facePoints[faceTriangle.Second];
                    var p3R = _facePoints[faceTriangle.Third];
                    var p1 = new Point(p1R.X, p1R.Y);
                    var p2 = new Point(p2R.X, p2R.Y);
                    var p3 = new Point(p3R.X, p3R.Y);

                    var triangle = new GeometryGroup();
                    triangle.Children.Add(new LineGeometry(p1, p2));
                    triangle.Children.Add(new LineGeometry(p2, p3));
                    triangle.Children.Add(new LineGeometry(p3, p1));
                    faceModelGroup.Children.Add(triangle);
                }
                drawingContext.DrawGeometry(_brush, _pen, faceModelGroup);
            }

            base.OnRender(drawingContext);
        }
        
        public void Dispose()
        {
            _tracker.TrackSucceed -= TrackerOnTrackSucceed;
        }
    }
}