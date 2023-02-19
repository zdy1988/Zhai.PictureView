using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Zhai.Famil.Common.Mvvm;
using Zhai.PictureView;

namespace Zhai.VideoView
{
    internal class PictureThumbBase : ViewModelBase
    {
        private BitmapSource thumbSource = PictureThumbStateResources.ImageLoading;
        public BitmapSource ThumbSource
        {
            get => thumbSource;
            set => Set(() => ThumbSource, ref thumbSource, value);
        }

        private PictureThumbState thumbState = PictureThumbState.Failed;
        public PictureThumbState ThumbState
        {
            get => thumbState;
            set => Set(() => ThumbState, ref thumbState, value);
        }

        private string _filename;

        public PictureThumbBase(string filename)
        {
            _filename = filename;
        }

        public void DrawThumb()
        {
            if (ThumbState != PictureThumbState.Failed)
                return;

            ThumbState = PictureThumbState.Loading;
            ThumbSource = PictureThumbStateResources.ImageLoading;

            if (!string.IsNullOrWhiteSpace(_filename))
            {
                try
                {
                    var thumbSource = ImageDecoder.GetThumb(_filename);

                    if (thumbSource != null)
                    {
                        ThumbSource = thumbSource;
                        ThumbState = PictureThumbState.Loaded;
                    }
                    else
                    {
                        ThumbSource = PictureThumbStateResources.ImageFailed;
                        ThumbState = PictureThumbState.Failed;
                    }
                }
                catch
                {
                    ThumbSource = PictureThumbStateResources.ImageFailed;
                    ThumbState = PictureThumbState.Failed;
                }
            }
        }
    }
}
