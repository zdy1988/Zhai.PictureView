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
            var newIndex = App.PictureWindowViewModel.CurrentPictureIndex + 1;

            if (newIndex > App.PictureWindowViewModel.Folder.Count - 1)
            {
                newIndex = 0;
            }

            App.PictureWindowViewModel.CurrentPictureIndex = newIndex;
        }

        public void Play(double interval = 2000)
        {
            if (interval == 0) return;

            timer.IsEnabled = true;
            timer.Interval = TimeSpan.FromMilliseconds(interval);
            timer.Start();
        }

        public void Stop()
        {
            if (timer.IsEnabled)
            {
                timer.IsEnabled = false;
                timer.Stop();
            }
        }
    }
}
