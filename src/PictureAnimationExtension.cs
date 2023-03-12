using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Zhai.PictureView
{
    internal static class PictureAnimationExtension
    {
        public static void CleanAnimation(this Image image)
        {
            var stream = XamlAnimatedGif.AnimationBehavior.GetSourceStream(image);

            if (stream != null)
            {
                XamlAnimatedGif.AnimationBehavior.SetSourceUri(image, null);
            }
        }

        public static void RunAnimation(this Image image, Picture picture)
        {
            XamlAnimatedGif.AnimationBehavior.SetSourceUri(image, new Uri(picture.PicturePath));
        }

        public static void RunAnimation(this Image image, Stream stream)
        {
            XamlAnimatedGif.AnimationBehavior.SetSourceStream(image, stream);
        }
    }
}
