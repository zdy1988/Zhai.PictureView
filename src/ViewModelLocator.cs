using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zhai.PictureView
{
    internal class ViewModelLocator
    {
        public PictureWindowViewModel PictureWindow { get; } = new PictureWindowViewModel();

        public SettingsWindowViewModel SettingsWindow { get; } = new SettingsWindowViewModel();
    }
}
