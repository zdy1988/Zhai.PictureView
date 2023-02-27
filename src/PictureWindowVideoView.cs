using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zhai.Famil.Common.Mvvm;

namespace Zhai.PictureView
{
    internal partial class PictureWindowViewModel : ViewModelBase
    {
        private bool isVideoPlaying;
        public bool IsVideoPlaying
        {
            get => isVideoPlaying;
            set => Set(() => IsVideoPlaying, ref isVideoPlaying, value);
        }

        private bool isVideoErrored;
        public bool IsVideoErrored
        {
            get => isVideoErrored;
            set => Set(() => IsVideoErrored, ref isVideoErrored, value);
        }

        private string videoErrorMessage;
        public string VideoErrorMessage
        {
            get => videoErrorMessage;
            set => Set(() => VideoErrorMessage, ref videoErrorMessage, value);
        }
    }
}
