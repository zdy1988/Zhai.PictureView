using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Zhai.Famil.Common.Threads;

namespace Zhai.PictureView.XamlServices
{
    internal class PictureService
    {
        public static readonly DependencyProperty PictureProperty = DependencyProperty.RegisterAttached("Picture", typeof(Picture), typeof(PictureService), new PropertyMetadata(new PropertyChangedCallback(OnImageSourcePropertyChangedCallback)));
        public static Picture GetPicture(DependencyObject obj) => (Picture)obj.GetValue(PictureProperty);
        public static void SetPicture(DependencyObject obj, object value) => obj.SetValue(PictureProperty, value);


        private static readonly AsyncTaskQueue taskQueue = new();

        private static void OnImageSourcePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is Image image)
            {
                if (image.IsLoaded)
                {
                    return;
                }

                var pic = GetPicture(image);

                taskQueue.Run(() => pic.DrawThumb());
            }
        }
    }
}
