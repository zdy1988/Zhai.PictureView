using ImageMagick;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Zhai.Famil.Win32.Common;
using Zhai.Famil.Win32.NativeMethods;

namespace Zhai.PictureView
{
    internal static partial class ImageDecoder
    {
        internal static BitmapSource ToBitmapSource(this Bitmap bitmap)
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
                    Gdi32.DeleteObject(handle);
            }

            return bitmapSource;
        }

        internal static Stream ToStream(this BitmapSource bitmapSource)
        {
            Stream stream = new MemoryStream();

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(stream);

            stream.Position = 0;

            return stream;
        }

        internal static byte[] ToByteArray(this BitmapSource bitmapSource)
        {
            byte[] buffer = null;

            Stream stream = bitmapSource.ToStream();

            if (stream.Length > 0)
            {
                using (var br = new BinaryReader(stream))
                {
                    buffer = br.ReadBytes((int)stream.Length);
                }
            }

            stream.Close();

            return buffer;
        }

        public static BitmapImage ToBitmapImage(this byte[] byteArray)
        {
            BitmapImage bmp;

            try
            {
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(byteArray);
                bmp.EndInit();
            }
            catch
            {
                bmp = null;
            }

            return bmp;
        }


        internal static BitmapSource GetThumb(string filename)
        {
            if (File.Exists(filename))
            {
                return (Path.GetExtension(filename).ToUpperInvariant()) switch
                {
                    ".JPG" or ".JPEG" or ".JPE" or ".PNG" or ".BMP" or ".GIF" or ".ICO" or ".JFIF" => GetWindowsThumbnail(filename),
                    ".MP4" or ".AVI" or ".WMV" => GetWindowsThumbnail(filename),
                    _ => GetMagickThumbnail(filename),
                };
            }

            return null;
        }

        internal static BitmapSource GetWindowsThumbnail(string filename)
        {
            try
            {
                var thumb = Zhai.Famil.Win32.ThumbnailProvider.GetThumbnail(filename);

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



        internal static async Task<BitmapSource> GetBitmapSourceAsync(string filename)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filename);

                var extension = fileInfo.Extension.ToLowerInvariant();

                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg":
                    case ".jpe":
                    case ".png":
                    case ".bmp":
                    case ".gif":
                    case ".jfif":
                    case ".ico":
                    case ".webp":
                    case ".wbmp":
                        return await GetWriteableBitmapAsync(fileInfo).ConfigureAwait(false);

                    case ".tga":
                        return await Task.FromResult(GetDefaultBitmapSource(fileInfo, true)).ConfigureAwait(false);

                    case ".svg":
                        return await GetTransparentBitmapSourceAsync(fileInfo, MagickFormat.Svg).ConfigureAwait(false);

                    case ".b64":
                        return await Base64StringToBitmap(fileInfo).ConfigureAwait(false);

                    default:
                        return await Task.FromResult(GetDefaultBitmapSource(fileInfo)).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Trace.WriteLine($"{nameof(GetBitmapSourceAsync)} {filename} exception, \n {e.Message}");
#endif
                return null;
            }
        }

        /// <summary>
        /// Create MagickImage and make sure its transparent, return it as BitmapSource
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="magickFormat"></param>
        /// <returns></returns>
        private static async Task<BitmapSource> GetTransparentBitmapSourceAsync(FileInfo fileInfo, MagickFormat magickFormat)
        {
            FileStream? filestream = null;
            MagickImage magickImage = new()
            {
                Quality = 100,
                ColorSpace = ColorSpace.Transparent,
                BackgroundColor = MagickColors.Transparent,
                Format = magickFormat,
            };
            try
            {
                filestream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true);
                byte[] data = new byte[filestream.Length];
                await filestream.ReadAsync(data.AsMemory(0, (int)filestream.Length)).ConfigureAwait(false);

                magickImage.Read(data);
                magickImage.Settings.Format = magickFormat;
                magickImage.Settings.BackgroundColor = MagickColors.Transparent;
                magickImage.Settings.FillColor = MagickColors.Transparent;
            }
            catch (Exception e)
            {
                filestream?.Dispose();
                magickImage?.Dispose();
#if DEBUG
                Trace.WriteLine($"{nameof(GetTransparentBitmapSourceAsync)} {fileInfo.Name} exception, \n {e.Message}");
#endif
                return null;
            }

            await filestream.DisposeAsync().ConfigureAwait(false);

            var bitmap = magickImage.ToBitmapSource();
            magickImage.Dispose();
            bitmap.Freeze();
            return bitmap;
        }

        private static async Task<WriteableBitmap> GetWriteableBitmapAsync(FileInfo fileInfo)
        {
            try
            {
                using (var stream = File.OpenRead(fileInfo.FullName))
                {
                    var data = new byte[stream.Length];
                    await stream.ReadAsync(data.AsMemory(0, (int)stream.Length)).ConfigureAwait(false);
                    var sKBitmap = SKBitmap.Decode(data);
                    if (sKBitmap is null)
                    {
                        return null;
                    }

                    var skPic = sKBitmap.ToWriteableBitmap();
                    skPic.Freeze();
                    sKBitmap.Dispose();
                    return skPic;
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Trace.WriteLine($"{nameof(GetWriteableBitmapAsync)} {fileInfo.Name} exception, \n {e.Message}");
#endif
                return null;
            }
        }

        private static BitmapSource GetDefaultBitmapSource(FileInfo fileInfo, bool autoOrient = false)
        {
            var magick = new MagickImage();
            try
            {
                magick.Read(fileInfo);
                if (autoOrient)
                {
                    magick.AutoOrient();
                }

                magick.Quality = 100;

                var pic = magick.ToBitmapSource();
                magick.Dispose();
                pic.Freeze();

                return pic;
            }
            catch (Exception e)
            {
#if DEBUG
                Trace.WriteLine($"{nameof(GetDefaultBitmapSource)} {fileInfo.Name} exception, \n {e.Message}");
#endif
                return null;
            }
        }


        #region Base64

        /// <summary>
        /// Converts string from base64 value to BitmapSource
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        internal static Task<BitmapSource> Base64StringToBitmap(string base64String) => Task.Run(async () =>
        {
            byte[] binaryData = Convert.FromBase64String(base64String);
            return await Task.FromResult(Base64FromBytes(binaryData)).ConfigureAwait(false);
        });

        internal static Task<BitmapSource> Base64StringToBitmap(FileInfo fileInfo) => Task.Run(async () =>
        {
            var text = await File.ReadAllTextAsync(fileInfo.FullName).ConfigureAwait(false);
            byte[] binaryData = Convert.FromBase64String(text);
            return await Task.FromResult(Base64FromBytes(binaryData)).ConfigureAwait(false);
        });

        private static BitmapSource Base64FromBytes(byte[] binaryData)
        {
            using MagickImage magick = new MagickImage();
            var mrs = new MagickReadSettings
            {
                Density = new Density(300, 300),
                BackgroundColor = MagickColors.Transparent,
            };

            try
            {
                magick.Read(new MemoryStream(binaryData), mrs);
            }
            catch (MagickException)
            {
                return null;
            }
            // Set values for maximum quality
            magick.Quality = 100;
            magick.ColorSpace = ColorSpace.Transparent;

            var pic = magick.ToBitmapSource();
            pic.Freeze();
            return pic;
        }

        internal static bool IsBase64String(string base64)
        {
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }

        #endregion


        #region EXIF

        internal static async Task<PictureExif> GetExifAsync(string filename)
        {
            var exif = new PictureExif
            {
                { "图片名称", Path.GetFileName(filename) },
                { "图片路径", filename }
            };

            if (File.Exists(filename))
            {
                using (var magickImage = new MagickImage())
                {
                    await magickImage.ReadAsync(filename);

                    var profile = magickImage.GetExifProfile();

                    if (profile != null)
                    {
                        exif.Add("图片宽度", magickImage.Width.ToString());
                        exif.Add("图片高度", magickImage.Height.ToString());

                        foreach (var value in profile.Values)
                        {
                            if (TryGetExifValue(value, out string name, out string description))
                            {
                                if (!exif.ContainsKey(name))
                                {
                                    exif.Add(name, description);
                                }
                            }
                        }
                    }
                }
            }

            return exif;
        }

        internal static bool TryGetExifValue(IExifValue value, out string name, out string description)
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

        #endregion

        #region Save

        /// <summary>
        /// 压缩图片至目标大小
        /// </summary>
        /// <param name="img">图片</param>
        /// <param name="format">图片格式</param>
        /// <param name="targetLen">压缩后大小</param>
        /// <param name="srcLen">原始大小</param>
        /// <returns>压缩后的图片</returns>
        public static Image CompressImageToTargetSize(Image img, ImageFormat format, long targetLen)
        {
            const long nearlyLen = 10240;

            var ms = new MemoryStream();

            long srcLen = 0;

            if (0 == srcLen)
            {
                img.Save(ms, format);
                srcLen = ms.Length;
            }

            targetLen *= 1024;

            if (targetLen > srcLen)
            {
                ms.SetLength(0);
                ms.Position = 0;
                img.Save(ms, format);
                img = Image.FromStream(ms);
                return img;
            }

            var exitLen = targetLen - nearlyLen;

            var quality = (long)Math.Floor(100.00 * targetLen / srcLen);

            var parms = new EncoderParameters(1);

            //获取编码器信息
            ImageCodecInfo formatInfo = null;
            var encoders = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo icf in encoders)
            {
                if (icf.FormatID == format.Guid)
                {
                    formatInfo = icf;
                    break;
                }
            }

            //使用二分法进行查找 最接近的质量参数
            long startQuality = quality;
            long endQuality = 100;
            quality = (startQuality + endQuality) / 2;

            while (true)
            {
                //设置质量
                parms.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

                //清空内存流 然后保存图片
                ms.SetLength(0);
                ms.Position = 0;
                img.Save(ms, formatInfo, parms);

                //若压缩后大小低于目标大小，则满足条件退出
                if (ms.Length >= exitLen && ms.Length <= targetLen)
                {
                    break;
                }
                else if (startQuality >= endQuality) //区间相等无需再次计算
                {
                    break;
                }
                else if (ms.Length < exitLen) //压缩过小,起始质量右移
                {
                    startQuality = quality;
                }
                else //压缩过大 终止质量左移
                {
                    endQuality = quality;
                }

                //重新设置质量参数 如果计算出来的质量没有发生变化，则终止查找。这样是为了避免重复计算情况{start:16,end:18} 和 {start:16,endQuality:17}
                var newQuality = (startQuality + endQuality) / 2;

                if (newQuality == quality)
                {
                    break;
                }

                quality = newQuality;
            }

            img = Image.FromStream(ms);

            return img;
        }

        public static async Task<bool> SaveImageAsync(byte[] bytes, string targetPath, int? quality = null, int? width = null, int? height = null, bool? isIgnoreRatio = false)
        {
            try
            {
                if (bytes is null)
                {
                    return false;
                }

                bool isMultiframe = Path.GetExtension(targetPath) == ".GIF";

                if (isMultiframe)
                {
                    using (var magickImageCollection = new MagickImageCollection(bytes))
                    {
                        magickImageCollection.Coalesce();

                        foreach (var magickImage in magickImageCollection)
                        {
                            if (quality is not null)
                            {
                                magickImage.Quality = quality.Value;
                            }

                            if (width is not null && height is not null)
                            {
                                //if (isIgnoreRatio == true)
                                //    magickImage.LiquidRescale(width.Value, height.Value);
                                //else
                                magickImage.Resize(width.Value, height.Value);
                            }
                            else if (width is not null && height is null)
                            {
                                //if (isIgnoreRatio == true)
                                //    magickImage.LiquidRescale(width.Value, 0);
                                //else
                                magickImage.Resize(width.Value, 0);
                            }
                            else if (height is not null && width is null)
                            {
                                //if (isIgnoreRatio == true)
                                //    magickImage.LiquidRescale(0, height.Value);
                                //else
                                magickImage.Resize(0, height.Value);
                            }
                        }

                        await Task.Run(async () => await magickImageCollection.WriteAsync(targetPath));
                    }
                }
                else
                {
                    using (var magickImage = new MagickImage(bytes))
                    {
                        if (quality is not null)
                        {
                            magickImage.Quality = quality.Value;
                        }

                        if (width is not null && height is not null)
                        {
                            //if (isIgnoreRatio == true)
                            //    magickImage.LiquidRescale(width.Value, height.Value);
                            //else
                            magickImage.Resize(width.Value, height.Value);
                        }
                        else if (width is not null && height is null)
                        {
                            //if (isIgnoreRatio == true)
                            //    magickImage.LiquidRescale(width.Value, 0);
                            //else
                            magickImage.Resize(width.Value, 0);
                        }
                        else if (height is not null && width is null)
                        {
                            //if (isIgnoreRatio == true)
                            //    magickImage.LiquidRescale(0, height.Value);
                            //else
                            magickImage.Resize(0, height.Value);
                        }

                        await Task.Run(async () => await magickImage.WriteAsync(targetPath));
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Trace.WriteLine($"{nameof(SaveImageAsync)} {targetPath} exception, \n {e.Message}");
#endif

                return false;
            }

            return true;
        }

        public static async Task<bool> SaveImageAsync(byte[] bytes, string targetPath, long targetLen, int? quality = null, int? width = null, int? height = null, bool? isIgnoreRatio = false)
        {
            try
            {
                if (bytes is null)
                {
                    return false;
                }

                using (var magickImage = new MagickImage(bytes))
                {
                    if (quality is not null)
                    {
                        magickImage.Quality = quality.Value;
                    }

                    if (width is not null && height is not null)
                    {
                        //if (isIgnoreRatio == true)
                        //    magickImage.LiquidRescale(width.Value, height.Value);
                        //else
                        magickImage.Resize(width.Value, height.Value);
                    }
                    else if (width is not null && height is null)
                    {
                        //if (isIgnoreRatio == true)
                        //    magickImage.LiquidRescale(width.Value, 0);
                        //else
                        magickImage.Resize(width.Value, 0);
                    }
                    else if (height is not null && width is null)
                    {
                        //if (isIgnoreRatio == true)
                        //    magickImage.LiquidRescale(0, height.Value);
                        //else
                        magickImage.Resize(0, height.Value);
                    }

                    await Task.Run(() =>
                    {
                        var image = magickImage.ToBitmap();

                        CompressImageToTargetSize(image, ImageFormat.Png, targetLen).Save(targetPath);
                    });
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Trace.WriteLine($"{nameof(SaveImageAsync)} {targetPath} exception, \n {e.Message}");
#endif

                return false;
            }

            return true;
        }

        #endregion

        /// <summary>
        /// 裁剪
        /// </summary>
        public static bool Crop(byte[] bytes, Rect rect, bool isMultiframe, out byte[] result)
        {
            try
            {
                if (bytes is null)
                {
                    result = null;

                    return false;
                }

                if (isMultiframe)
                {
                    using (var magickImageCollection = new MagickImageCollection(bytes))
                    {
                        magickImageCollection.Coalesce();

                        foreach (var magickImage in magickImageCollection)
                        {
                            magickImage.Quality = 100;
                            magickImage.Crop(new MagickGeometry((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));
                        }

                        result = magickImageCollection.ToByteArray();
                    }
                }
                else
                {
                    using (var magickImage = new MagickImage(bytes))
                    {
                        magickImage.Quality = 100;
                        magickImage.Crop(new MagickGeometry((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));

                        result = magickImage.ToByteArray();
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Trace.WriteLine($"{nameof(Crop)} exception, \n {e.Message}");
#endif
                result = null;

                return false;
            }

            return true;
        }

        /// <summary>
        /// 水平翻转
        /// </summary>
        public static bool Flop(byte[] bytes, bool isMultiframe, out byte[] result)
        {
            try
            {
                if (bytes is null)
                {
                    result = null;

                    return false;
                }

                if (isMultiframe)
                {
                    using (var magickImageCollection = new MagickImageCollection(bytes))
                    {
                        magickImageCollection.Coalesce();

                        foreach (var magickImage in magickImageCollection)
                        {
                            magickImage.Quality = 100;
                            magickImage.Flop();
                        }

                        result = magickImageCollection.ToByteArray();
                    }
                }
                else
                {
                    using (var magickImage = new MagickImage(bytes))
                    {
                        magickImage.Quality = 100;
                        magickImage.Flop();

                        result = magickImage.ToByteArray();
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Trace.WriteLine($"{nameof(Flop)} exception, \n {e.Message}");
#endif
                result = null;

                return false;
            }

            return true;
        }

        /// <summary>
        /// 垂直翻转
        /// </summary>
        public static bool Flip(byte[] bytes, bool isMultiframe, out byte[] result)
        {
            try
            {
                if (bytes is null)
                {
                    result = null;

                    return false;
                }

                if (isMultiframe)
                {
                    using (var magickImageCollection = new MagickImageCollection(bytes))
                    {
                        magickImageCollection.Coalesce();

                        foreach (var magickImage in magickImageCollection)
                        {
                            magickImage.Quality = 100;
                            magickImage.Flip();
                        }

                        result = magickImageCollection.ToByteArray();
                    }
                }
                else
                {
                    using (var magickImage = new MagickImage(bytes))
                    {
                        magickImage.Quality = 100;
                        magickImage.Flip();

                        result = magickImage.ToByteArray();
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Trace.WriteLine($"{nameof(Flip)} exception, \n {e.Message}");
#endif
                result = null;

                return false;
            }

            return true;
        }

        /// <summary>
        /// 向左旋转
        /// </summary>
        public static bool RotateRight(byte[] bytes, double degrees, bool isMultiframe, out byte[] result)
        {
            try
            {
                if (bytes is null)
                {
                    result = null;

                    return false;
                }

                if (isMultiframe)
                {

                    using (var magickImageCollection = new MagickImageCollection(bytes))
                    {
                        magickImageCollection.Coalesce();

                        foreach (var magickImage in magickImageCollection)
                        {
                            magickImage.Quality = 100;
                            magickImage.Rotate(degrees);
                        }

                        result = magickImageCollection.ToByteArray();
                    }
                }
                else
                {
                    using (var magickImage = new MagickImage(bytes))
                    {
                        magickImage.Quality = 100;
                        magickImage.Rotate(degrees);

                        result = magickImage.ToByteArray();
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Trace.WriteLine($"{nameof(RotateRight)} exception, \n {e.Message}");
#endif
                result = null;

                return false;
            }

            return true;
        }

        /// <summary>
        /// 向右旋转
        /// </summary>
        public static bool RotateLeft(byte[] bytes, double degrees, bool isMultiframe, out byte[] result)
        {
            try
            {
                if (bytes is null)
                {
                    result = null;

                    return false;
                }

                if (isMultiframe)
                {
                    using (var magickImageCollection = new MagickImageCollection(bytes))
                    {
                        magickImageCollection.Coalesce();

                        foreach (var magickImage in magickImageCollection)
                        {
                            magickImage.Quality = 100;
                            magickImage.Rotate(-degrees);
                        }

                        result = magickImageCollection.ToByteArray();
                    }
                }
                else
                {
                    using (var magickImage = new MagickImage(bytes))
                    {
                        magickImage.Quality = 100;
                        magickImage.Rotate(-degrees);

                        result = magickImage.ToByteArray();
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Trace.WriteLine($"{nameof(RotateLeft)} exception, \n {e.Message}");
#endif
                result = null;

                return false;
            }

            return true;
        }
    }
}
