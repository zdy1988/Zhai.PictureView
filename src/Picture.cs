using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Zhai.PictureView
{
    internal class Picture:IDisposable
    {
        public string Name { get; }

        public long Size { get; }

        public String PicturePath { get; }

        public double PixelWidth { get; set; }

        public double PixelHeight { get; set; }

        public BitmapSource Thumb { get; private set; }

        public Bitmap Bitmap { get; private set; }

        public Dictionary<string, string> Exif { get; private set; }

        public Boolean IsAnimation
        {
            get
            {
                if (Bitmap == null) return false;

                try
                {
                    return Bitmap.RawFormat.ToString().ToUpper().Equals("GIF");
                }
                catch
                {
                    return false;
                }
            }
        }


        public Picture(string filename)
        {
            FileInfo fi = new FileInfo(filename);

            Name = fi.Name;

            Size = fi.Length;

            PicturePath = filename;

            Thumb = ImageDecoder.GetThumb(filename);
        }

        public BitmapSource Draw()
        {
            if (Bitmap == null)
            {
                Bitmap = ImageDecoder.GetBitmap(PicturePath, out Dictionary<string, string> exif);

                Exif = exif;
            }

            var source = ImageDecoder.GetBitmapSource(Bitmap);

            PixelWidth = source.PixelWidth;

            PixelHeight = source.PixelHeight;

            return source;
        }


        #region Animation

        public delegate void FrameUpdatedEventHandler();

        public event EventHandler<BitmapSource> PictureAnimating;

        public bool isAnimated = false;

        public void StartAnimate()
        {
            isAnimated = true;

            if (ImageAnimator.CanAnimate(Bitmap))
            {
                ImageAnimator.Animate(Bitmap, OnFrameChanged);
            }
        }

        public void StopAnimate()
        {
            isAnimated = false;

            ImageAnimator.StopAnimate(Bitmap, OnFrameChanged);
        }

        public void OnFrameChanged(object sender, EventArgs e)
        {
            try
            {
                ImageAnimator.UpdateFrames(Bitmap);

                PictureAnimating?.Invoke(this, ImageDecoder.GetBitmapSource(Bitmap));
            }
            catch { }
        }

        #endregion

        public void Dispose()
        {
            if (isAnimated)
            {
                StopAnimate();
            }

            Thumb = null;
            Bitmap?.Dispose();
            Bitmap = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
