using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Zhai.Famil.Common.Threads;
using Zhai.VideoView;

namespace Zhai.PictureView.XamlServices
{
    internal class PictureThumbService
    {
        private static readonly AsyncTaskQueue taskQueue = new();



        public static readonly DependencyProperty PictureProperty = DependencyProperty.RegisterAttached("Picture", typeof(Picture), typeof(PictureThumbService), new PropertyMetadata(new PropertyChangedCallback(OnPicturePropertyChangedCallback)));
        public static Picture GetPicture(DependencyObject obj) => (Picture)obj.GetValue(PictureProperty);
        public static void SetPicture(DependencyObject obj, object value) => obj.SetValue(PictureProperty, value);
        private static void OnPicturePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is Image image)
            {
                if (image.IsLoaded)
                {
                    return;
                }

                var pic = GetPicture(image);

                if (pic != null)
                {
                    var binding = new Binding()
                    {
                        Source = pic,
                        Path = new PropertyPath("ThumbSource")
                    };

                    image.SetBinding(Image.SourceProperty, binding);

                    taskQueue.Run(() => pic.DrawThumb());
                }
            }
        }



        public static readonly DependencyProperty PictureDirectoryProperty = DependencyProperty.RegisterAttached("PictureDirectory", typeof(DirectoryInfo), typeof(PictureThumbService), new PropertyMetadata(new PropertyChangedCallback(OnPictureDirectoryPropertyChangedCallback)));
        public static DirectoryInfo GetPictureDirectory(DependencyObject obj) => (DirectoryInfo)obj.GetValue(PictureDirectoryProperty);
        public static void SetPictureDirectory(DependencyObject obj, object value) => obj.SetValue(PictureDirectoryProperty, value);
        private static void OnPictureDirectoryPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is Image image)
            {
                if (image.IsLoaded)
                {
                    return;
                }

                var directory = GetPictureDirectory(image);

                if (directory == null)
                {
                    image.Source = PictureThumbStateResources.ImageFailed;
                    return;
                }

                var filename = FindCover(directory.FullName, directory.Name);

                if (string.IsNullOrEmpty(filename))
                {
                    var file = directory.EnumerateFiles().Where(PictureSupport.PictureSupportExpression).FirstOrDefault();

                    if (file != null)
                        filename = file.FullName;
                }

                if (!string.IsNullOrEmpty(filename))
                {
                    var source = new PictureThumbBase(filename);

                    var binding = new Binding()
                    {
                        Source = source,
                        Path = new PropertyPath("ThumbSource")
                    };

                    image.SetBinding(Image.SourceProperty, binding);

                    source.DrawThumb();
                }
            }
        }

        private static string FindCover(string directory, string name)
        {
            string[] reservePaths = new string[] { $"{directory}\\cover", $"{directory}\\{name}" };

            foreach (var path in reservePaths)
            {
                foreach (var format in new string[] { "jpg", "jpeg", "png" })
                {
                    string cover = $"{path}.{format}";

                    if (File.Exists(cover))
                    {
                        return cover;
                    }
                }
            }

            return string.Empty;
        }
    }
}
