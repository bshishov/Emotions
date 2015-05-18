﻿using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using Emotions.Controls;
using Emotions.KinectTools;
using Emotions.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using Emotions.Services.Engine;

namespace Emotions.Services.KinectInput
{
    [Export(typeof(IEditorProvider))]
    [Export(typeof(IKinectInputService))]
    class KinectInputService : IKinectInputService, IEditorProvider
    {
        private readonly IEngineService _engine;
        private readonly ILog _log = LogManager.GetLog(typeof(KinectInputService));
        private readonly IKinectSource _kinectSource;

        public KinectInputService()
        {
            _engine = IoC.Get<IEngineService>();

            try
            {
                _kinectSource = new RealKinectSource();
                _log.Info("Sensor initialized");
            }
            catch (Exception ex)
            {
                _log.Warn("Failed to initialize kinect");
                _log.Error(ex);
                _log.Warn("Running without sensor");
            }
        }

        public bool IsKinectAvailable
        {
            get { return _kinectSource != null; }
        }

        public IKinectSource GetKinect()
        {
            return _kinectSource;
        }

        public void Dispose()
        {
            _engine.Stop();

            if(_kinectSource != null)
                _kinectSource.Stop();

        }
        
        public IKinectSource LoadRecording(string path)
        {
            try
            {
                _log.Info("Loading recording {0}", path);
                var source = new KinectPlayer(path);
                source.Stopped += SourceOnPlaybackEnded;
                source.Start();
                return source;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            return null;
        }

        private void SourceOnPlaybackEnded(IKinectSource source)
        {
            _log.Info("Playback ended");
        }

        public bool Handles(string path)
        {
            return true;
        }

        public IDocument Create(string path)
        {
            var player = LoadRecording(path);
            if(player != null)
                return new KinectOutputViewModel(player);
            return null;
        }
    }
}
