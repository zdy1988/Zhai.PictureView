using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Zhai.PictureView
{
    internal enum PictureState
    {
        Loading,
        Loaded,
        Failed
    }

    internal static class PictureStateResources
    {
        public static BitmapImage ImageLoading = new BitmapImage(new Uri("pack://application:,,,/Resources/image-loading.png"));

        public static BitmapImage ImageFailed = new BitmapImage(new Uri("pack://application:,,,/Resources/image-failed.png"));
    }
}
