using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using AForge.Video.FFMPEG;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;

namespace Emotions.KinectTools.Writers
{
    public class KinectDepthStreamWriter : IWriter<DepthFrame>
    {
        private VideoFileWriter _writer;
        private readonly KinectSourceInfo _info;
        private readonly PixelFormat _pixelFormat;
        private ColorPalette _pallete;

        public KinectDepthStreamWriter(KinectSourceInfo info)
        {
            _info = info;
            _pixelFormat = PixelFormat.Format24bppRgb;
        }

        public string Path { get; private set; }

        public void Open(string path)
        {
            _writer = new VideoFileWriter();
            _writer.Open(path, _info.DepthFrameWidth, _info.DepthFrameHeight, 30, VideoCodec.MSMPEG4v3, 120 * 1000 * 1000);
            Path = path;
        }

        public void Write(DepthFrame inputFrame)
        {
            var image = new Bitmap(_info.DepthFrameWidth, _info.DepthFrameHeight, _pixelFormat);
            var rect = new Rectangle(0, 0, _info.DepthFrameWidth, _info.DepthFrameHeight);
            var bmpData = image.LockBits(rect, ImageLockMode.WriteOnly, image.PixelFormat);
            var bytes = inputFrame.GetRgb24Bytes();
            Marshal.Copy(bytes, 0, bmpData.Scan0, bytes.Length);
            image.UnlockBits(bmpData);
            _writer.WriteVideoFrame(image);
        }

        public void Close()
        {
            _writer.Close();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}