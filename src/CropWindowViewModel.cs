using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using Zhai.Famil.Common.Mvvm;
using Zhai.Famil.Common.ExtensionMethods;
using System.IO;

namespace Zhai.PictureView
{
    internal class CropWindowViewModel : ViewModelBase
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
                    PictureSourceChanged?.Invoke(this, value);
                }
            }
        }

        private byte[] pictureBytes;
        public byte[] PictureBytes
        {
            get
            {
                if (pictureBytes == null)
                {
                    pictureBytes = currentPicture.ReadAllBytes();
                }

                return pictureBytes;
            }
            set { pictureBytes = value; }
        }

        private int cropWidth;
        public int CropWidth
        {
            get { return cropWidth; }
            set
            {
                if (value > PictureSource.PixelWidth)
                {
                    value = (int)PictureSource.PixelWidth;
                }
                else if (value < minWidth)
                {
                    value = (int)minWidth;
                }

                if (Set(() => CropWidth, ref cropWidth, value))
                {
                    var height = Math.Max(cropRegion.Height, minHeight);
                    var width = imageWidth / (double)PictureSource.PixelWidth * (double)value;
                    width = Math.Max(width, minWidth);

                    var x = (imageWidth - width) / 2.0;
                    var y = (imageHeight - height) / 2.0;

                    if (x + width >= imageWidth)
                    {
                        x = 0;
                        width = imageWidth;
                    }


                    if (y + height >= imageHeight)
                    {
                        y = 0;
                        height = imageHeight;
                    }


                    CropRegion = new Rect(x, y, width, height);
                }
            }
        }

        private int cropHeight;
        public int CropHeight
        {
            get { return cropHeight; }
            set
            {
                if (value > PictureSource.PixelHeight)
                {
                    value = (int)PictureSource.PixelHeight;
                }
                else if (value < minHeight)
                {
                    value = (int)minHeight;
                }

                if (Set(() => CropHeight, ref cropHeight, value))
                {
                    var width = Math.Max(cropRegion.Width, minWidth);
                    var height = imageHeight / (double)PictureSource.PixelHeight * (double)value;
                    height = Math.Max(height, minHeight);

                    var x = (imageWidth - width) / 2.0;
                    var y = (imageHeight - height) / 2.0;

                    if (x + width >= imageWidth)
                    {
                        x = 0;
                        width = imageWidth;
                    }

                    if (y + height >= imageHeight)
                    {
                        y = 0;
                        height = imageHeight;
                    }

                    CropRegion = new Rect(x, y, width, height);
                }
            }
        }

        private Rect cropRegion;
        public Rect CropRegion
        {
            get { return cropRegion; }
            set
            {
                if (value != Rect.Empty && IsLockedCropRatio)
                {
                    var ratio = GetCropRatio();

                    if (ratio.HasValue)
                    {
                        value.Height = value.Width / ratio.Value;
                    }
                }

                if (Set(() => CropRegion, ref cropRegion, value))
                {
                    var width = value.Width / imageWidth * (double)PictureSource.PixelWidth;
                    var height = value.Height / imageHeight * (double)PictureSource.PixelHeight;

                    cropWidth = width.IsNormalNumber() ? (int)width : PictureSource.PixelWidth;
                    cropHeight = height.IsNormalNumber() ? (int)height : PictureSource.PixelHeight;

                    base.RaisePropertyChanged(nameof(CropWidth));
                    base.RaisePropertyChanged(nameof(CropHeight));
                }

                base.RaisePropertyChanged(nameof(IsCropRegionVisiable));
            }
        }

        private bool isLockedCropRatio = true;
        public bool IsLockedCropRatio
        {
            get { return isLockedCropRatio; }
            set
            {
                if (Set(() => IsLockedCropRatio, ref isLockedCropRatio, value))
                {
                    if (value && CropRegion != Rect.Empty)
                    {
                        LockCropRatio = CropRegion.Width / CropRegion.Height;
                    }
                    else
                    {
                        LockCropRatio = null;
                    }
                }
            }
        }

        private bool isLockedCropRatioEnabled = true;
        public bool IsLockedCropRatioEnabled
        {
            get { return isLockedCropRatioEnabled; }
            set { Set(() => IsLockedCropRatioEnabled, ref isLockedCropRatioEnabled, value); }
        }

        private CropRatio selectedCropRatio = CropRatio.Free;
        public CropRatio SelectedCropRatio
        {
            get => selectedCropRatio;
            set
            {
                if (Set(() => SelectedCropRatio, ref selectedCropRatio, value))
                {
                    if (value == CropRatio.Free)
                    {
                        IsLockedCropRatio = false;
                        IsLockedCropRatioEnabled = true;
                    }
                    else
                    {
                        IsLockedCropRatio = true;
                        IsLockedCropRatioEnabled = false;

                        var ratio = GetCropRatio();

                        if (ratio.HasValue)
                        {
                            double width, height;

                            if (imageWidth > imageHeight)
                            {
                                height = imageHeight * 0.5;
                                width = height * ratio.Value;
                            }
                            else
                            {
                                width = imageWidth * 0.5;
                                height = width / ratio.Value;
                            }

                            var x = (imageWidth - width) / 2.0;
                            var y = (imageHeight - height) / 2.0;

                            CropRegion = new Rect(x, y, width, height);
                        }
                    }
                }
            }
        }

        public bool IsCropRegionVisiable => CropRegion != Rect.Empty && CropRegion.Width > 0 && CropRegion.Height > 0;

        #endregion

        private readonly Picture currentPicture;

        private double minWidth = 5;
        private double minHeight = 5;

        private double imageWidth = 0;
        private double imageHeight = 0;

        public static double? LockCropRatio = null;

        public static Dictionary<CropRatio, string> CropRatios => new Dictionary<CropRatio, string>
        {
            { CropRatio.Free, "自由比例" },
            { CropRatio.Original, "原图比例" },
            { CropRatio.R11, "1 : 1" },
            { CropRatio.R43, "4 : 3" },
            { CropRatio.R34, "3 : 4" },
            { CropRatio.R169, "16 : 9" },
            { CropRatio.R916, "9 : 16" },
            { CropRatio.R32, "3 : 2" },
            { CropRatio.R23, "2 : 3" },
        };

        public enum CropRatio
        {
            Free,
            Original,
            R11,
            R43,
            R34,
            R169,
            R916,
            R32,
            R23
        }

        public CropWindowViewModel(Picture picture)
        {
            this.currentPicture = picture;
            this.PictureSource = picture.PictureSource.Clone();
            this.CropWidth = PictureSource.PixelWidth;
            this.CropHeight = PictureSource.PixelHeight;
        }

        public void UpdateImageSize(double width, double height)
        {
            imageWidth = width;
            imageHeight = height;
        }

        private double? GetCropRatio()
        {
            switch (SelectedCropRatio)
            {
                case CropRatio.Original:
                    return ((double)PictureSource.PixelWidth / (double)PictureSource.PixelHeight);
                case CropRatio.R11:
                    return 1;
                case CropRatio.R43:
                    return 4.0 / 3.0;
                case CropRatio.R34:
                    return 3.0 / 4.0;
                case CropRatio.R169:
                    return 16.0 / 9.0;
                case CropRatio.R916:
                    return 9.0 / 16.0;
                case CropRatio.R32:
                    return 3.0 / 2.0;
                case CropRatio.R23:
                    return 2.0 / 3.0;
            }

            if (SelectedCropRatio == CropRatio.Free && LockCropRatio.HasValue)
            {
                return LockCropRatio;
            }

            return null;
        }

        public void Crop(Rect region)
        {
            var x = (int)(region.X / imageWidth * (double)PictureSource.PixelWidth);
            var y = (int)(region.Y / imageHeight * (double)PictureSource.PixelHeight);
            var width = (int)(region.Width / imageWidth * (double)PictureSource.PixelWidth);
            var height = (int)(region.Height / imageHeight * (double)PictureSource.PixelHeight);

            if (ImageDecoder.Crop(PictureBytes, new Rect(x, y, width, height), currentPicture.IsAnimation, out byte[] result))
            {
                PictureSource = result.ToBitmapImage();
                PictureBytes = result;
                CropWidth = PictureSource.PixelWidth;
                CropHeight = PictureSource.PixelHeight;
            }
        }

        public void Flop()
        {
            if (ImageDecoder.Flop(PictureBytes, currentPicture.IsAnimation, out byte[] result))
            {
                PictureSource = result.ToBitmapImage();
                PictureBytes = result;
            }
        }

        public void Flip()
        {
            if (ImageDecoder.Flip(PictureBytes, currentPicture.IsAnimation, out byte[] result))
            {
                PictureSource = result.ToBitmapImage();
                PictureBytes = result;
            }
        }

        public void RotateLeft()
        {
            if (ImageDecoder.RotateLeft(PictureBytes, 90, currentPicture.IsAnimation, out byte[] result))
            {
                PictureSource = result.ToBitmapImage();
                PictureBytes = result;
            }
        }

        public void RotateRight()
        {
            if (ImageDecoder.RotateRight(PictureBytes, 90, currentPicture.IsAnimation, out byte[] result))
            {
                PictureSource = result.ToBitmapImage();
                PictureBytes = result;
            }
        }

        public event EventHandler<BitmapSource> PictureSourceChanged;
    }
}
