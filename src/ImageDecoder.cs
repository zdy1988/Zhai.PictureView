using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Zhai.PictureView
{
    internal static class ImageDecoder
    {
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool DeleteObject(IntPtr hObject);

        internal static BitmapSource GetBitmapSource(Bitmap bitmap)
        {
            if (bitmap == null) return null;

            IntPtr handle = IntPtr.Zero;

            BitmapSource bitmapSource;

            try
            {
                handle = bitmap.GetHbitmap();
                bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                bitmapSource.Freeze();
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    DeleteObject(handle);
            }

            return bitmapSource;
        }

        internal static BitmapSource GetThumb(string filename)
        {
            if (File.Exists(filename))
            {
                return (Path.GetExtension(filename).ToUpperInvariant()) switch
                {
                    ".JPG" or ".JPEG" or ".JPE" or ".PNG" or ".BMP" or ".GIF" or ".ICO" or ".JFIF" => GetWindowsThumbnail(filename),
                    _ => GetMagickThumbnail(filename),
                };
            }

            return null;
        }

        internal static BitmapSource GetWindowsThumbnail(string filename)
        {
            try
            {
                var thumb = Microsoft.WindowsAPICodePack.Shell.ShellFile.FromFilePath(filename).Thumbnail.BitmapSource;

                if (!thumb.IsFrozen)
                {
                    thumb.Freeze();
                }

                return thumb;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"{nameof(ImageDecoder)} : GetWindowsThumbnail returned {filename} null  : {ex.Message}");
#endif
                return null;
            }
        }

        internal static BitmapSource GetMagickThumbnail(string filename, int quality = 80, int size = 256)
        {
            try
            {
                using var magickImage = new MagickImage();
                magickImage.Quality = quality;
                magickImage.ColorSpace = ColorSpace.Transparent;
                magickImage.Read(filename);
                magickImage.AdaptiveResize(size, size);

                BitmapSource thumb = magickImage.ToBitmapSource();

                thumb.Freeze();

                return thumb;
            }
            catch (MagickException e)
            {
#if DEBUG
                Trace.WriteLine("GetThumb returned " + filename + " null, \n" + e.Message);
#endif
                return null;
            }
        }

        internal static Bitmap GetBitmap(string filename, out Dictionary<string, string> exif)
        {
            exif = null;

            try
            {
                using var magickImage = new MagickImage();
                magickImage.Read(filename);
                magickImage.Quality = 100;

                exif = GetExif(magickImage);

                Bitmap image = magickImage.ToBitmap();

                return image;
            }
            catch (Exception e)
            {
#if DEBUG
                Debug.WriteLine("GetBitmap returned " + filename + " null, \n" + e.Message);
#endif
                return null;
            }
        }

        internal static Dictionary<string, string> GetExif(MagickImage image)
        {
            var dictionary = new Dictionary<string, string>
            {
                { "图片名称", System.IO.Path.GetFileName(image.FileName) },
                { "图片路径", image.FileName }
            };

            var profile = image.GetExifProfile();

            if (profile != null)
            {
                dictionary.Add("图片宽度", image.Width.ToString());
                dictionary.Add("图片高度", image.Height.ToString());

                foreach (var value in profile.Values)
                {
                    if (TryGetExif(value, out string name, out string description))
                    {
                        if (!dictionary.ContainsKey(name))
                        {
                            dictionary.Add(name, description);
                        }
                    }
                }
            }

            return dictionary;
        }

        internal static bool TryGetExif(IExifValue value, out string name, out string description)
        {
            name = string.Empty;

            description = value.ToString();

            switch (value.Tag.ToString())
            {
                case "Model":
                    name = "相机型号";
                    break;
                case "LensModel":
                    name = "镜头类型";
                    break;
                case "DateTime":
                    name = "拍摄时间";
                    break;
                case "ImageHeight":
                    name = "照片高度";
                    break;
                case "ImageWidth":
                    name = "照片宽度";
                    break;
                case "ColorSpace":
                    name = "色彩空间";
                    break;

                case "FNumber":
                    name = "光圈";
                    break;
                case "ISOSpeedRatings":
                    name = "ISO";
                    description = ((ushort[])value.GetValue())[0].ToString();
                    break;
                case "ExposureBiasValue":
                    name = "曝光补偿";
                    break;
                case "FocalLength":
                    name = "焦距";
                    break;
                case "ExposureTime":
                    name = "曝光时间";
                    break;

                case "ExposureProgram":
                    name = "曝光程序";
                    break;
                case "MeteringMode":
                    name = "测光模式";
                    break;
                case "FlashMode":
                    name = "闪光灯";
                    break;
                case "WhiteBalanceMode":
                    name = "白平衡";
                    break;
                case "ExposureMode":
                    name = "曝光模式";
                    break;
                case "ContinuousDriveMode":
                    name = "驱动模式";
                    break;
                case "FocusMode":
                    name = "对焦模式";
                    break;

                case "Artist":
                    name = "作者";
                    break;
                case "Copyright":
                    name = "版权信息";
                    break;
                case "FileModifiedDate":
                    name = "修改时间";
                    break;
            }

            return name != String.Empty;
        }
    }
}
