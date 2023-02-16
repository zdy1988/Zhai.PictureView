﻿using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Zhai.Famil.Common.Mvvm;

namespace Zhai.PictureView
{
    internal class Picture : ViewModelBase
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

        private BitmapSource thumbSource = PictureStateResources.ImageLoading;
        public BitmapSource ThumbSource
        {
            get => thumbSource;
            set => Set(() => ThumbSource, ref thumbSource, value);
        }

        private PictureState thumbState = PictureState.Failed;
        public PictureState ThumbState
        {
            get => thumbState;
            set => Set(() => ThumbState, ref thumbState, value);
        }

        private BitmapSource pictureSource = PictureStateResources.ImageLoading;
        public BitmapSource PictureSource
        {
            get => pictureSource;
            set => Set(() => PictureSource, ref pictureSource, value);
        }

        private PictureState pictureState = PictureState.Failed;
        public PictureState PictureState
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

        public bool IsLoaded
        {
            get
            {
                if (PictureSource == null) return false;

                return PictureState == PictureState.Loaded;
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
        {
            var file = new FileInfo(filename);

            Name = file.Name;

            Size = file.Length;

            PicturePath = filename;
        }



        public void DrawThumb()
        {
            if (ThumbState != PictureState.Failed) 
                return;

            ThumbState = PictureState.Loading;
            ThumbSource = PictureStateResources.ImageLoading;

            if (!string.IsNullOrWhiteSpace(PicturePath))
            {
                try
                {
                    var thumbSource = ImageDecoder.GetThumb(PicturePath);

                    if (thumbSource != null)
                    {
                        ThumbSource = thumbSource;
                        ThumbState = PictureState.Loaded;
                    }
                    else
                    {
                        ThumbSource = PictureStateResources.ImageFailed;
                        ThumbState = PictureState.Failed;
                    }
                }
                catch
                {
                    ThumbSource = PictureStateResources.ImageFailed;
                    ThumbState = PictureState.Failed;
                }
            }
        }

        public async Task<BitmapSource> DrawAsync()
        {
            try
            {
                if (PictureSource == null || PictureState == PictureState.Failed)
                {
                    PictureState = PictureState.Loading;

                    var imageSource = await Task.Run(async () => await ImageDecoder.GetBitmapSourceAsync(PicturePath));

                    if (imageSource != null)
                    {
                        PictureSource = imageSource;
                        PictureState = PictureState.Loaded;
                    }
                    else
                    {
                        PictureSource = PictureStateResources.ImageFailed;
                        PictureState = PictureState.Failed;
                    }
                }
                else
                {
                    PictureState = PictureState.Loaded;
                }
            }
            catch
            {
                PictureSource = PictureStateResources.ImageFailed;
                PictureState = PictureState.Failed;
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
