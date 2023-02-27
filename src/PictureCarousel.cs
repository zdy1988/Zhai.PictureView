using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Zhai.PictureView
{
    internal class PictureCarousel
    {
        readonly DispatcherTimer timer;

        public PictureCarousel()
        {
            timer = new DispatcherTimer();

            timer.Tick += Timer_Tick;
        }

        internal static PictureCarousel Instance = new();

        private void Timer_Tick(object sender, EventArgs e)
        {
            var newIndex = App.ViewModelLocator.PictureWindow.CurrentPictureIndex + 1;

            if (newIndex > App.ViewModelLocator.PictureWindow.Folder.Count - 1)
            {
                newIndex = 0;
            }

            var folder = App.ViewModelLocator.PictureWindow.Folder;

            void IgnoreVideo()
            {
                if (folder != null && folder[newIndex] != null && folder[newIndex].IsVideo)
                {
                    newIndex += 1;

                    IgnoreVideo();
                }
            }

            IgnoreVideo();

            App.ViewModelLocator.PictureWindow.CurrentPictureIndex = newIndex;
        }

        public void Play(double? interval = null)
        {
            if (interval == null || interval == 0)
            {
                interval = Properties.Settings.Default.AutoPlayInterval * 1000.0;
            }

            timer.Interval = TimeSpan.FromMilliseconds(interval.Value);
            timer.IsEnabled = true;
            timer.Start();
        }

        public void Stop()
        {
            timer.IsEnabled = false;
            timer.Stop();
        }
    }
}
