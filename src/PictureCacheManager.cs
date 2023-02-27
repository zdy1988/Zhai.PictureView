using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zhai.PictureView
{
    internal static class PictureCacheManager
    {
        private static readonly List<Picture> ActivedPictures = new();

        public static void Managed(Picture current)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var item = ActivedPictures.Where(t => t == current).FirstOrDefault();

                if (item != null)
                {
                    ActivedPictures.Remove(item);
                }

                ActivedPictures.Insert(0, current);

                if (ActivedPictures.Count > Properties.Settings.Default.ActivedPicturesCount)
                {
                    foreach (var p in ActivedPictures.Skip(Properties.Settings.Default.ActivedPicturesCount))
                    {
                        p.PictureSource = null;
                        ActivedPictures.Remove(p);
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            });
        }
    }
}
