using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using AForge.Video.FFMPEG;
using Emotions.KinectTools.Frames;
using Emotions.KinectTools.Sources;

namespace Emotions.KinectTools.Writers
{
    public class KinectColorStreamWriter : IWriter<ColorFrame>
    {
        private VideoFileWriter _writer;
        private readonly KinectSourceInfo _info;
        private readonly PixelFormat _pixelFormat;

        public KinectColorStreamWriter(KinectSourceInfo info)
        {
            _info = info;
            _pixelFormat = PixelFormat.Format32bppRgb;
        }

        public string Path { get; private set; }

        public void Open(string path)
        {
            _writer = new VideoFileWriter();
            _writer.Open(path, _info.ColorFrameWidth, _info.ColorFrameHeight, 30, VideoCodec.MSMPEG4v3, 120 * 1000 * 1000);
            Path = path;
        }

        public void Write(ColorFrame colorFrame)
        {
            var image = new Bitmap(_info.ColorFrameWidth, _info.ColorFrameHeight, _pixelFormat);
            var rect = new Rectangle(0, 0, _info.ColorFrameWidth, _info.ColorFrameHeight);
            var bmpData = image.LockBits(rect, ImageLockMode.WriteOnly, image.PixelFormat);

            var ptr = bmpData.Scan0;
            var bytes = bmpData.Stride * image.Height;
            Marshal.Copy(colorFrame.Data, 0, ptr, bytes);
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