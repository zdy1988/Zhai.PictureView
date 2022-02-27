using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Zhai.PictureView
{
    internal class Picture : BaseViewModel
    {
        public string Name { get; }

        public long Size { get; }

        public String PicturePath { get; }

        private double pixelWidth;
        public double PixelWidth
        {
            get => pixelWidth;
            set => SetProperty(ref pixelWidth, value);
        }

        private double pixelHeight;
        public double PixelHeight
        {
            get => pixelHeight;
            set => SetProperty(ref pixelHeight, value);
        }

        private BitmapSource thumbSource;
        public BitmapSource ThumbSource
        {
            get => thumbSource;
            set => SetProperty(ref thumbSource, value);
        }

        private PictureState thumbState = PictureState.Failed;
        public PictureState ThumbState
        {
            get => thumbState;
            set => SetProperty(ref thumbState, value);
        }

        private BitmapSource pictureSource;
        public BitmapSource PictureSource
        {
            get => pictureSource;
            set => SetProperty(ref pictureSource, value);
        }

        private PictureState pictureState = PictureState.Failed;
        public PictureState PictureState
        {
            get => pictureState;
            set => SetProperty(ref pictureState, value);
        }

        public PictureExif PictureExif { get; private set; }

        public bool IsAnimation
        {
            get
            {
                if (PictureSource == null) return false;

                return Path.GetExtension(PicturePath).ToUpperInvariant() == ".GIF";
            }
        }

        public Picture(string filename)
        {
            var file = new FileInfo(filename);

            Name = file.Name;

            Size = file.Length;

            PicturePath = filename;

            DrawThumb();
        }



        public void DrawThumb()
        {
            if (ThumbState != PictureState.Failed) 
                return;

            ThumbState = PictureState.Loading;

            ThumbSource = PictureStateResources.ImageLoading;

            if (!string.IsNullOrWhiteSpace(PicturePath))
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        var thumbSource = ImageDecoder.GetThumb(PicturePath);

                        if (thumbSource != null)
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                ThumbSource = thumbSource;
                                ThumbState = PictureState.Loaded;
                            });
                        }
                        else
                        {
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                ThumbSource = PictureStateResources.ImageFailed;
                                ThumbState = PictureState.Failed;
                            });
                        }
                    }
                    catch
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            ThumbSource = PictureStateResources.ImageFailed;
                            ThumbState = PictureState.Failed;
                        });
                    }
                });
            }
        }

        public async Task<BitmapSource> DrawAsync()
        {
            try
            {
                if (PictureSource == null || PictureState == PictureState.Failed)
                {
                    PictureState = PictureState.Loading;

                    var imageSource = await Task.Run(async () => await ImageDecoder.GetBitmapSource(PicturePath));

                    if (imageSource != null&&imageSource.Item1!=null)
                    {
                        PictureSource = imageSource.Item1;
                        PictureExif = imageSource.Item2;
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

        public Stream ToStream()
        {
            if (PictureSource == null) return null;

            var ms = new MemoryStream();

            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(PictureSource));
            encoder.Save(ms);

            return ms;
        }



        public override void Clean()
        {
            ThumbSource = null;
            PictureSource = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
