using System;
using System.Collections.Generic;
using Microsoft.Kinect;

namespace Emotions.Services.KinectInput
{
    internal class FaceDetector : IDisposable
    {
        private const uint MaxMissedFrames = 100;
        private readonly KinectSensor _sensor;

        private readonly Dictionary<int, SkeletonFaceTracker> _trackedSkeletons =
            new Dictionary<int, SkeletonFaceTracker>();

        private byte[] _colorImage;
        private ColorImageFormat _colorImageFormat = ColorImageFormat.Undefined;
        private short[] _depthImage;
        private DepthImageFormat _depthImageFormat = DepthImageFormat.Undefined;

        private Skeleton[] _skeletonData;

        public FaceDetector(KinectSensor sensor)
        {
            if (sensor == null)
                throw new ArgumentException("Kinect sensor is null");

            _sensor = sensor;
            sensor.AllFramesReady += OnAllFramesReady;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public event SkeletonTracked SkeletonTracked;
        public event SkeletonTracked SkeletonUnTracked;

        private void OnSkeletonTracked(int trackingId)
        {
            var tracker = new SkeletonFaceTracker();
            _trackedSkeletons.Add(trackingId, tracker);

            SkeletonTracked handler = SkeletonTracked;
            if (handler != null) handler(this, new SkeletonTrackArgs(trackingId, tracker));
        }

        ~FaceDetector()
        {
            Dispose();
        }

        private void OnAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            ColorImageFrame colorImageFrame = null;
            DepthImageFrame depthImageFrame = null;
            SkeletonFrame skeletonFrame = null;

            try
            {
                colorImageFrame = e.OpenColorImageFrame();
                depthImageFrame = e.OpenDepthImageFrame();
                skeletonFrame = e.OpenSkeletonFrame();

                if (colorImageFrame == null || depthImageFrame == null || skeletonFrame == null)
                {
                    return;
                }

                // Check for image format changes.  The FaceTracker doesn't
                // deal with that so we need to reset.
                if (_depthImageFormat != depthImageFrame.Format)
                {
                    ResetFaceTracking();
                    _depthImage = null;
                    _depthImageFormat = depthImageFrame.Format;
                }

                if (_colorImageFormat != colorImageFrame.Format)
                {
                    ResetFaceTracking();
                    _colorImage = null;
                    _colorImageFormat = colorImageFrame.Format;
                }

                // Create any buffers to store copies of the data we work with
                if (_depthImage == null)
                {
                    _depthImage = new short[depthImageFrame.PixelDataLength];
                }

                if (_colorImage == null)
                {
                    _colorImage = new byte[colorImageFrame.PixelDataLength];
                }

                // Get the skeleton information
                if (_skeletonData == null || _skeletonData.Length != skeletonFrame.SkeletonArrayLength)
                {
                    _skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                }

                colorImageFrame.CopyPixelDataTo(_colorImage);
                depthImageFrame.CopyPixelDataTo(_depthImage);
                skeletonFrame.CopySkeletonDataTo(_skeletonData);

                // Update the list of trackers and the trackers with the current frame information
                foreach (Skeleton skeleton in _skeletonData)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked
                        || skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                        // We want keep a record of any skeleton, tracked or untracked.
                        if (!_trackedSkeletons.ContainsKey(skeleton.TrackingId))
                            OnSkeletonTracked(skeleton.TrackingId);
                        
                        // Give each tracker the upated frame.
                        SkeletonFaceTracker skeletonFaceTracker;
                        if (_trackedSkeletons.TryGetValue(skeleton.TrackingId, out skeletonFaceTracker))
                        {
                            skeletonFaceTracker.OnFrameReady(_sensor, _colorImageFormat, _colorImage, _depthImageFormat,
                                _depthImage, skeleton);
                            skeletonFaceTracker.LastTrackedFrame = skeletonFrame.FrameNumber;
                        }
                    }
                }

                RemoveOldTrackers(skeletonFrame.FrameNumber);
            }
            finally
            {
                if (colorImageFrame != null)
                    colorImageFrame.Dispose();

                if (depthImageFrame != null)
                    depthImageFrame.Dispose();

                if (skeletonFrame != null)
                    skeletonFrame.Dispose();
            }
        }

        private void ResetFaceTracking()
        {
            foreach (int trackingId in new List<int>(_trackedSkeletons.Keys))
            {
                RemoveTracker(trackingId);
            }
        }

        private void RemoveTracker(int trackingId)
        {
            SkeletonTracked handler = SkeletonUnTracked;
            if (handler != null) handler(this, new SkeletonTrackArgs(trackingId, _trackedSkeletons[trackingId]));

            _trackedSkeletons[trackingId].Dispose();
            _trackedSkeletons.Remove(trackingId);
        }

        /// <summary>
        ///     Clear out any trackers for skeletons we haven't heard from for a while
        /// </summary>
        private void RemoveOldTrackers(int currentFrameNumber)
        {
            var trackersToRemove = new List<int>();

            foreach (var tracker in _trackedSkeletons)
            {
                uint missedFrames = (uint) currentFrameNumber - (uint) tracker.Value.LastTrackedFrame;
                if (missedFrames > MaxMissedFrames)
                {
                    // There have been too many frames since we last saw this skeleton
                    trackersToRemove.Add(tracker.Key);
                }
            }

            foreach (int trackingId in trackersToRemove)
            {
                RemoveTracker(trackingId);
            }
        }
    }

    internal delegate void SkeletonTracked(object sender, SkeletonTrackArgs args);

    internal class SkeletonTrackArgs
    {
        public readonly SkeletonFaceTracker SkeletonFaceTracker;
        public readonly int TrackingId;

        public SkeletonTrackArgs(int trackingId, SkeletonFaceTracker skeletonFaceTracker)
        {
            TrackingId = trackingId;
            SkeletonFaceTracker = skeletonFaceTracker;
        }
    }
}