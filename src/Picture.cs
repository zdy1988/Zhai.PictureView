using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Zhai.Famil.Common.Mvvm;
using Zhai.VideoView;

namespace Zhai.PictureView
{
    internal class Picture : PictureThumbBase
    {
        public string Name { get; }

        public long Size { get; }

        public String PicturePath { get; }

        private double pixelWidth;
        public double PixelWidth
        {
            get => pixelWidth;
            set => Set(() => PixelWidth, ref pixelWidth, value);
        }

        private double pixelHeight;
        public double PixelHeight
        {
            get => pixelHeight;
            set => Set(() => PixelHeight, ref pixelHeight, value);
        }

        private BitmapSource pictureSource = PictureThumbStateResources.ImageLoading;
        public BitmapSource PictureSource
        {
            get => pictureSource;
            set => Set(() => PictureSource, ref pictureSource, value);
        }

        private PictureThumbState pictureState = PictureThumbState.Failed;
        public PictureThumbState PictureState
        {
            get => pictureState;
            set => Set(() => PictureState, ref pictureState, value);
        }

        private PictureExif pictureExif;
        public PictureExif PictureExif
        {
            get => pictureExif;
            set => Set(() => PictureExif, ref pictureExif, value);
        }

        public Stream PictureStream
        {
            get
            {
                if (PictureSource == null) return null;

                var ms = new MemoryStream();

                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(PictureSource));
                encoder.Save(ms);

                if (ms.CanSeek)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                }

                return ms;
            }
        }

        public bool IsAnimation
        {
            get
            {
                if (PictureSource == null) return false;

                return Path.GetExtension(PicturePath).ToUpperInvariant() == ".GIF";
            }
        }

        public bool IsVideo
        {
            get
            {
                if (string.IsNullOrEmpty(PicturePath)) return false;

                return PictureSupport.IsVideo(PicturePath);
            }
        }

        public bool IsLoaded
        {
            get
            {
                if (PictureSource == null) return false;

                return PictureState == PictureThumbState.Loaded;
            }
        }

        public string Extension
        {
            get
            {
                return Path.GetExtension(PicturePath);
            }
        }

        public Picture(string filename)
            : base(filename)
        {
            var file = new FileInfo(filename);

            Name = file.Name;

            Size = file.Length;

            PicturePath = filename;
        }

        public async Task<BitmapSource> DrawAsync()
        {
            try
            {
                if (PictureSource == null || PictureState == PictureThumbState.Failed)
                {
                    PictureState = PictureThumbState.Loading;

                    var imageSource = await Task.Run(async () => await ImageDecoder.GetBitmapSourceAsync(PicturePath));

                    if (imageSource != null)
                    {
                        PictureSource = imageSource;
                        PictureState = PictureThumbState.Loaded;
                    }
                    else
                    {
                        PictureSource = PictureThumbStateResources.ImageFailed;
                        PictureState = PictureThumbState.Failed;
                    }
                }
                else
                {
                    PictureState = PictureThumbState.Loaded;
                }
            }
            catch
            {
                PictureSource = PictureThumbStateResources.ImageFailed;
                PictureState = PictureThumbState.Failed;
            }

            PixelWidth = PictureSource.PixelWidth;

            PixelHeight = PictureSource.PixelHeight;

            return PictureSource;
        }

        public async Task<PictureExif> LoadExif()
        {
            if (IsLoaded && PictureExif == null)
            {
                try
                {
                    PictureExif = await Task.Run(async () => await ImageDecoder.GetExifAsync(PicturePath));
                }
                catch { }
            }

            return PictureExif;
        }

        public async Task<bool> SaveAsync(string? targetPath)
        {
            if (IsLoaded)
            {
                targetPath ??= PicturePath;

                return await ImageDecoder.SaveImageAsync(PictureStream, targetPath);
            }

            return false;
        }

        public bool Delete()
        {
            try
            {
                File.Delete(PicturePath);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();

            ThumbSource = null;
            PictureSource = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
