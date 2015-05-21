using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using AForge.Video.FFMPEG;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;

namespace Emotions.KinectTools.Readers
{
    public class KinectDepthStreamReader : IReader<DepthFrame>
    {
        private VideoFileReader _reader;
        private readonly KinectSourceInfo _info;
        private int _frameNumber;
        private string _path;
        private const int FrameDelay = 1000/30;
        private long _startTime;

        public KinectDepthStreamReader(KinectSourceInfo info)
        {
            _info = info;
        }

        public bool IsEnded { get; private set; }

        public void Open(string path)
        {
            _path = path;
            Init();
        }

        private void Init()
        {
            _startTime = DateTime.UtcNow.Ticks;
            _frameNumber = 0;
            IsEnded = false;
            _reader = new VideoFileReader();
            _reader.Open(_path);
            if (_reader.Width != _info.DepthFrameWidth || _reader.Height != _info.DepthFrameHeight)
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

        public DepthFrame Read()
        {
            var frame = _reader.ReadVideoFrame();
            var rect = new Rectangle(0, 0, _info.DepthFrameWidth, _info.DepthFrameHeight);
            var bmpData = frame.LockBits(rect, ImageLockMode.WriteOnly, frame.PixelFormat);
            
            var bytesCount = bmpData.Stride * frame.Height;
            var bytes = new byte[bytesCount];
            Marshal.Copy(bmpData.Scan0, bytes, 0, bytesCount);
            frame.UnlockBits(bmpData);
            frame.Dispose();

            _frameNumber++;
            if (_frameNumber >= _reader.FrameCount)
                IsEnded = true;

            return new DepthFrame(bytes, _info.DepthImageFormat, _frameNumber, _startTime + _frameNumber * 30);
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