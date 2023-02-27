using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Zhai.Famil.Common.Mvvm;
using Zhai.PictureView.NativeMethods;

namespace Zhai.PictureView
{
    internal class SettingsWindowViewModel : ViewModelBase
    {
        private bool isStartWindowMaximized = Properties.Settings.Default.IsStartWindowMaximized;
        public bool IsStartWindowMaximized
        {
            get => isStartWindowMaximized;
            set
            {
                if (Set(() => IsStartWindowMaximized, ref isStartWindowMaximized, value))
                {
                    Properties.Settings.Default.IsStartWindowMaximized = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private bool isWindowDarked = Properties.Settings.Default.IsWindowDarked;
        public bool IsWindowDarked
        {
            get => isWindowDarked;
            set
            {
                if (Set(() => IsWindowDarked, ref isWindowDarked, value))
                {
                    Properties.Settings.Default.IsWindowDarked = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private bool isWindowTransparency = Properties.Settings.Default.IsWindowTransparency;
        public bool IsWindowTransparency
        {
            get => isWindowTransparency;
            set
            {
                if (Set(() => IsWindowTransparency, ref isWindowTransparency, value))
                {
                    Properties.Settings.Default.IsWindowTransparency = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private int autoPlayInterval = Properties.Settings.Default.AutoPlayInterval;
        public int AutoPlayInterval
        {
            get => autoPlayInterval;
            set
            {
                if (Set(() => AutoPlayInterval, ref autoPlayInterval, value))
                {
                    Properties.Settings.Default.AutoPlayInterval = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        public List<PictureSupportedItem> AllSupported { get; }

        public SettingsWindowViewModel()
        {
            AllSupported = new List<PictureSupportedItem>(PictureSupport.All.Select(t => new PictureSupportedItem(t)));
        }
    }

    internal class PictureSupportedItem : ViewModelBase
    {
        private string ext;
        public string Ext { get => ext; }

        private bool isSupported;
        public bool IsSupported
        {
            get => isSupported;
            set
            {
                if (Set(() => IsSupported, ref isSupported, value))
                {
                    SetAssociation(value);
                }
            }
        }

        public PictureSupportedItem(string ext)
        {
            this.ext = ext;
          
            this.isSupported = FileAssociator.CreateInstance(ext).Exists;
        }

        public void SetAssociation(bool isSet)
        {
            var appPath = Process.GetCurrentProcess().MainModule.FileName;

            if (isSet)
            {
                FileAssociator.CreateInstance(ext).Create(Properties.Settings.Default.AppName, null, null, new ExecApplication(appPath), null);
            }
            else
            {
                FileAssociator.CreateInstance(ext).Delete();
            }
        }
    }
}
