using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Emotions.Services.KinectInput;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using Point = System.Windows.Point;

namespace Emotions.Utilities
{
    public class KinectDrawer : Adorner, IDisposable
    {
        private readonly SkeletonFaceTracker _tracker;
        private EnumIndexableCollection<FeaturePoint, PointF> _facePoints;
        private FaceTriangle[] _triangles;
        private readonly Brush _brush = Brushes.Cyan;
        private readonly Pen _pen = new Pen(Brushes.Cyan, 1);
        private Skeleton _skeleton;

        public SkeletonFaceTracker Tracker { get { return _tracker; } }

        public KinectDrawer(UIElement adornedElement, SkeletonFaceTracker tracker) : base(adornedElement)
        {
            _tracker = tracker;
            _tracker.TrackSucceed += TrackerOnTrackSucceed;
        }

        private void TrackerOnTrackSucceed(object sender, FaceTrackFrame frame, Skeleton skeleton)
        {
            _facePoints = frame.GetProjected3DShape();
            _triangles = frame.GetTriangles();
            _skeleton = skeleton;
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
                

                DrawJoints(drawingContext, _skeleton.Joints[JointType.Head], _skeleton.Joints[JointType.ShoulderCenter]);

                DrawJoints(drawingContext, _skeleton.Joints[JointType.ShoulderCenter], _skeleton.Joints[JointType.ShoulderLeft]);
                DrawJoints(drawingContext, _skeleton.Joints[JointType.ShoulderCenter], _skeleton.Joints[JointType.ShoulderRight]);

                DrawJoints(drawingContext, _skeleton.Joints[JointType.ShoulderLeft], _skeleton.Joints[JointType.ElbowLeft]);
                DrawJoints(drawingContext, _skeleton.Joints[JointType.ElbowLeft], _skeleton.Joints[JointType.WristLeft]);
                DrawJoints(drawingContext, _skeleton.Joints[JointType.WristLeft], _skeleton.Joints[JointType.HandLeft]);

                DrawJoints(drawingContext, _skeleton.Joints[JointType.ShoulderRight], _skeleton.Joints[JointType.ElbowRight]);
                DrawJoints(drawingContext, _skeleton.Joints[JointType.ElbowRight], _skeleton.Joints[JointType.WristRight]);
                DrawJoints(drawingContext, _skeleton.Joints[JointType.WristRight], _skeleton.Joints[JointType.HandRight]);
            }

            base.OnRender(drawingContext);
        }

        private void DrawJoints(DrawingContext context, Joint a, Joint b)
        {
            var jointGroup = new GeometryGroup();
            jointGroup.Children.Add(new EllipseGeometry(Project(a.Position), 2, 2));
            jointGroup.Children.Add(new EllipseGeometry(Project(b.Position), 2, 2));
            jointGroup.Children.Add(new LineGeometry(Project(a.Position), Project(b.Position)));
            context.DrawGeometry(_brush, _pen, jointGroup);
        }
        
        public void Dispose()
        {
            _tracker.TrackSucceed -= TrackerOnTrackSucceed;
        }

        private Point Project(SkeletonPoint point)
        {
            const double xf = 0.8f;
            const double yf = 1.1f;
            var x = point.X * (xf / point.Z);
            var y = point.Y * (yf / point.Z);

            return new Point(
                 x * ActualWidth + ActualWidth / 2,
                -y * ActualHeight + ActualHeight / 2);
        }
    }
}