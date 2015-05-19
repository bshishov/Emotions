using System;
using System.Collections.Generic;
using System.Windows;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;
using Microsoft.Kinect;

namespace Emotions.KinectTools.Tracking
{
    public class SkeletonTracker : IDisposable
    {
        private const uint MaxMissedFrames = 100;
        private readonly IKinectSource _sensor;

        private readonly Dictionary<int, SkeletonFaceTracker> _trackedSkeletons =
            new Dictionary<int, SkeletonFaceTracker>();
        
        private ColorImageFormat _colorImageFormat = ColorImageFormat.Undefined;
        private DepthImageFormat _depthImageFormat = DepthImageFormat.Undefined;

        public SkeletonTracker(IKinectSource sensor)
        {
            if (sensor == null)
                throw new ArgumentException("Kinect sensor is null");

            _sensor = sensor;

            sensor.FramesReady += (s, e) => Application.Current.Dispatcher.Invoke(() => OnAllFramesReady(s, e));
        }

        public void Dispose()
        {
            _sensor.FramesReady -= OnAllFramesReady;
            //GC.SuppressFinalize(this);
        }

        public event SkeletonTracked SkeletonTracked;
        public event SkeletonTracked SkeletonUnTracked;

        private void OnSkeletonTracked(int trackingId)
        {
           
        }

        ~SkeletonTracker()
        {
            Dispose();
        }

        private void OnAllFramesReady(object sender, FramesContainer e)
        {
            var source = sender as IKinectSource;
            

            if (source == null ||
                e.ColorFrame == null || 
                e.DepthFrame == null || 
                e.SkeletonFrame == null)
            {
                return;
            }

            // Check for image format changes.  The FaceTracker doesn't
            // deal with that so we need to reset.
            if (_depthImageFormat != source.Info.DepthImageFormat)
            {
                ResetFaceTracking();
                _depthImageFormat = source.Info.DepthImageFormat;
            }

            if (_colorImageFormat != source.Info.ColorImageFormat)
            {
                ResetFaceTracking();
                _colorImageFormat = source.Info.ColorImageFormat;
            }

            // Update the list of trackers and the trackers with the current frame information
            foreach (var skeleton in e.SkeletonFrame.Skeletons)
            {
                if (skeleton.TrackingState == SkeletonTrackingState.Tracked
                    || skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                {
                    // We want keep a record of any skeleton, tracked or untracked.
                    if (!_trackedSkeletons.ContainsKey(skeleton.TrackingId))
                    {
                        var tracker = new SkeletonFaceTracker();
                        _trackedSkeletons.Add(skeleton.TrackingId, tracker);

                        SkeletonTracked handler = SkeletonTracked;
                        if (handler != null) handler(this, new SkeletonTrackArgs(skeleton.TrackingId, tracker));
                    }
                        
                    // Give each tracker the upated frame.
                    SkeletonFaceTracker skeletonFaceTracker;
                    if (_trackedSkeletons.TryGetValue(skeleton.TrackingId, out skeletonFaceTracker))
                    {
                        skeletonFaceTracker.OnFrameReady(_sensor, e.ColorFrame.Data, e.DepthFrame.Data, skeleton);
                        skeletonFaceTracker.LastTrackedFrame = e.SkeletonFrame.FrameNumber;
                    }
                }
            }

            RemoveOldTrackers(e.SkeletonFrame.FrameNumber);
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

    public delegate void SkeletonTracked(object sender, SkeletonTrackArgs args);

    public class SkeletonTrackArgs
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