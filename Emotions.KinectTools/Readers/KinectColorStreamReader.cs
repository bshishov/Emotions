using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using AForge.Video.FFMPEG;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;
using Microsoft.Kinect;

namespace Emotions.KinectTools.Readers
{
    class KinectColorStreamReader : IReader<ColorFrame>
    {
        private VideoFileReader _reader;
        private readonly KinectSourceInfo _info;
        private int _frameNumber;
        private int _frameDelay;
        private string _path;
        private long _startTime;


        public KinectColorStreamReader(KinectSourceInfo info)
        {
            _info = info;
        }

        public bool IsEnded { get; private set; }

        public void Open(string path)
        {
            _path = path;

            if (_info.ColorImageFormat == ColorImageFormat.RgbResolution640x480Fps30)
                _frameDelay = 1000 / 30;
            else
                _frameDelay = 1000 / 15;

            Init();
        }

        private void Init()
        {
            _frameNumber = 0;
            IsEnded = false;
            _reader = new VideoFileReader();
            _reader.Open(_path);
            if (_reader.Width != _info.ColorFrameWidth || _reader.Height != _info.ColorFrameHeight)
                throw new Exception("Videostream has wrong format");
        }

        public void Reset()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader.Dispose();
            }

            Init();
        }

        public ColorFrame Read()
        {
            var frame = _reader.ReadVideoFrame();
            var rect = new Rectangle(0, 0, _info.ColorFrameWidth, _info.ColorFrameHeight);
            var bmpData = frame.LockBits(rect, ImageLockMode.ReadOnly, frame.PixelFormat);
            
            var bytesCount = bmpData.Stride * frame.Height;
            var bytes = new byte[bytesCount];
            Marshal.Copy(bmpData.Scan0, bytes, 0, bytesCount);
            frame.UnlockBits(bmpData);
            frame.Dispose();

            _frameNumber++;
            if (_frameNumber >= _reader.FrameCount)
                IsEnded = true;

            return new ColorFrame(bytes, _info.ColorImageFormat, _frameNumber, _startTime + _frameNumber * 30);
        }

        public void Close()
        {
            _reader.Close();
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}