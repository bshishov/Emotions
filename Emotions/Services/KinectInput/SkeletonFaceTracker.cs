using System;
using System.Diagnostics;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;

namespace Emotions.Services.KinectInput
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
            if (this._faceTracker != null)
            {
                this._faceTracker.Dispose();
                this._faceTracker = null;
            }
        }

        /// <summary>
        /// Updates the face tracking information for this skeleton
        /// </summary>
        internal void OnFrameReady(KinectSensor kinectSensor, ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage, Skeleton skeletonOfInterest)
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
                    _faceTracker = new FaceTracker(kinectSensor);
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

            if (this._faceTracker != null)
            {
                var frame = this._faceTracker.Track(colorImageFormat, colorImage, depthImageFormat, depthImage, skeletonOfInterest);
                _lastFaceTrackSucceeded = frame.TrackSuccessful;
                if (_lastFaceTrackSucceeded)
                {
                    if (TrackSucceed != null)
                        TrackSucceed(this, frame, skeletonOfInterest);
                }
            }
        }

        
    }

    public delegate void TrackEvent(object sender, FaceTrackFrame frame, Skeleton skeleton);
}
