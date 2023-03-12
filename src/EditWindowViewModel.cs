using ImageMagick;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Zhai.Famil.Common.ExtensionMethods;
using Zhai.Famil.Common.Mvvm;

namespace Zhai.PictureView
{
    internal class EditWindowViewModel : ViewModelBase
    {
        #region Properties

        private BitmapSource pictureSource;
        public BitmapSource PictureSource
        {
            get => pictureSource;
            set
            {
                if (Set(() => PictureSource, ref pictureSource, value))
                {
                    PictureStream = null;

                    //PreviewSource = value;
                    UpdatePreviewSource();

                    PictureSourceChanged?.Invoke(this, value);
                }
            }
        }

        private Stream pictureStream;
        public Stream PictureStream
        {
            set => pictureStream = value;
            get
            {
                pictureStream ??= PictureSource.ToStream();

                return pictureStream;
            }
        }

        private BitmapSource previewSource = PictureThumbStateResources.ImageLoading;
        public BitmapSource PreviewSource
        {
            get => previewSource;
            set
            {
                if (Set(() => PreviewSource, ref previewSource, value))
                {
                    
                }
            }
        }

        public byte[] PreviewBytes { get; set; }

        #endregion

        public ImageModulate Modulate { get; }

        public EditWindowViewModel(Picture picture)
        {
            Modulate = new ImageModulate(this);

            this.PictureSource = new BitmapImage(new Uri("E:\\Photo\\2016.08.21 香山\\GOPR3520.jpg"));
        }

        private void UpdatePreviewSource()
        {
            using (var magickImage = new MagickImage())
            {
                magickImage.ReadAsync(PictureStream);

                if (PictureSource.PixelWidth > 720 || PictureSource.PixelHeight > 720)
                {
                    if (PictureSource.PixelWidth > PictureSource.PixelHeight)
                    {
                        magickImage.Resize(720, 0);
                    }
                    else
                    {
                        magickImage.Resize(0, 720);
                    }
                }

                PreviewBytes = magickImage.ToByteArray();
                PreviewSource = magickImage.ToBitmapSource();
            }
        }

        public void ImageModulate()
        {
            using var magickImage = new MagickImage(PreviewBytes);
            {
                magickImage.Modulate(new Percentage(Modulate.Brightness), new Percentage(Modulate.Saturation + 100), new Percentage(Modulate.Hue + 100));
                magickImage.BrightnessContrast(new Percentage(Modulate.Exposure), new Percentage(Modulate.Contrast));
                magickImage.ContrastStretch(new Percentage(Modulate.ContrastBlack), new Percentage(Modulate.ContrastWhite));

                PreviewSource = magickImage.ToBitmapSource();
            }
        }

        public event EventHandler<BitmapSource> PictureSourceChanged;
    }

    internal class ImageModulate : ViewModelBase
    {
        EditWindowViewModel Owner;

        public ImageModulate(EditWindowViewModel owner)
        {
            Owner = owner;
        }

        private int hue = 0;
        /// <summary>
        /// 色相
        /// </summary>
        public int Hue
        {
            get => hue;
            set
            {
                if (Set(() => Hue, ref hue, value))
                {
                    Owner.ImageModulate();
                }
            }
        }

        private int saturation = 0;
        /// <summary>
        /// 饱和度
        /// </summary>
        public int Saturation
        {
            get => saturation;
            set
            {
                if (Set(() => Saturation, ref saturation, value))
                {
                    Owner.ImageModulate();
                }
            }
        }

        private int brightness = 100;
        /// <summary>
        /// 明度
        /// </summary>
        public int Brightness
        {
            get => brightness;
            set
            {
                if (Set(() => Brightness, ref brightness, value))
                {
                    Owner.ImageModulate();
                }
            }
        }

        private int contrast = 0;
        /// <summary>
        /// 对比度
        /// </summary>
        public int Contrast
        {
            get => contrast;
            set
            {
                if (Set(() => Contrast, ref contrast, value))
                {
                    Owner.ImageModulate();
                }
            }
        }

        private int exposure = 0;
        /// <summary>
        /// 曝光
        /// </summary>
        public int Exposure
        {
            get => exposure;
            set
            {
                if (Set(() => Exposure, ref exposure, value))
                {
                    Owner.ImageModulate();
                }
            }
        }

        private int contrastWhite;
        /// <summary>
        /// 对比度(白色)
        /// </summary>
        public int ContrastWhite
        {
            get => contrastWhite;
            set
            {
                if (Set(() => ContrastWhite, ref contrastWhite, value))
                {
                    Owner.ImageModulate();
                }
            }
        }

        private int contrastBlack = 0;
        /// <summary>
        /// 对比度(黑色)
        /// </summary>
        public int ContrastBlack
        {
            get => contrastBlack;
            set
            {
                if (Set(() => ContrastBlack, ref contrastBlack, value))
                {
                    Owner.ImageModulate();
                }
            }
        }
    }
}
