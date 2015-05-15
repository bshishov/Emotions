using System;
using System.Diagnostics;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace Emotions.KinectTools
{
    public class SkeletonFaceTracker : IDisposable
    {
        public event TrackEvent TrackSucceed;

        private FaceTracker _faceTracker;

        private bool _lastFaceTrackSucceeded;

        private SkeletonTrackingState _skeletonTrackingState;

        public int LastTrackedFrame { get; set; }

        public void Dispose()
        {
            if (_faceTracker != null)
            {
                _faceTracker.Dispose();
                _faceTracker = null;
            }
        }

        /// <summary>
        /// Updates the face tracking information for this skeleton
        /// </summary>
        internal void OnFrameReady(IKinectSource source, byte[] colorImage, short[] depthImage, Skeleton skeletonOfInterest)
        {
            _skeletonTrackingState = skeletonOfInterest.TrackingState;

            if (_skeletonTrackingState != SkeletonTrackingState.Tracked)
            {
                // nothing to do with an untracked skeleton.
                return;
            }

            if (_faceTracker == null)
            {
                try
                {
                    _faceTracker = new FaceTracker(
                        source.Info.ColorImageFormat,
                        source.Info.ColorFrameWidth,
                        source.Info.ColorFrameHeight,
                        source.Info.ColorFocalLength,
                        source.Info.DepthImageFormat,
                        source.Info.DepthFrameWidth,
                        source.Info.DepthFrameHeight,
                        source.Info.DepthFocalLength,
                        source.Info.Mapper);
                }
                catch (InvalidOperationException)
                {
                    // During some shutdown scenarios the FaceTracker
                    // is unable to be instantiated.  Catch that exception
                    // and don't track a face.
                    Debug.WriteLine("AllFramesReady - creating a new FaceTracker threw an InvalidOperationException");
                    _faceTracker = null;
                }
            }

            if (_faceTracker == null) return;
            
            var frame = _faceTracker.Track(source.Info.ColorImageFormat, colorImage, source.Info.DepthImageFormat, depthImage, skeletonOfInterest);
            _lastFaceTrackSucceeded = frame.TrackSuccessful;
            if (_lastFaceTrackSucceeded)
            {
                if (TrackSucceed != null)
                    TrackSucceed(this, frame, skeletonOfInterest);
            }
        }

        
    }

    public delegate void TrackEvent(object sender, FaceTrackFrame frame, Skeleton skeleton);
}
