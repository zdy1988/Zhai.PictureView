using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zhai.PictureView.NativeMethods;

namespace Zhai.PictureView
{
    internal class SettingsWindowViewModel: BaseViewModel
    {
        private bool isStartWindowMaximized = Properties.Settings.Default.IsStartWindowMaximized;
        public bool IsStartWindowMaximized
        {
            get => isStartWindowMaximized;
            set
            {
                if (SetProperty(ref isStartWindowMaximized, value))
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
                if (SetProperty(ref isWindowDarked, value))
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
                if (SetProperty(ref isWindowTransparency, value))
                {
                    Properties.Settings.Default.IsWindowTransparency = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        public List<PictureSupportedItem> AllSupported { get; }

        public SettingsWindowViewModel()
        {
            AllSupported = new List<PictureSupportedItem>(PictureSupport.All.Select(t => new PictureSupportedItem(t)));
        }


        public override void Clean()
        {
            
        }
    }

    internal class PictureSupportedItem : BaseViewModel
    {
        private string ext;
        public string Ext { get => ext; }

        private bool isSupported;
        public bool IsSupported
        {
            get => isSupported;
            set
            {
                if (SetProperty(ref isSupported, value))
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
                FileAssociator.CreateInstance(ext).Create("ZDY.PICTURE", null, null, new ExecApplication(appPath), null);
            }
            else
            {
                FileAssociator.CreateInstance(ext).Delete();
            }
        }

        public override void Clean()
        {

        }
    }
}
