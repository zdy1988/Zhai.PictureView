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
        private bool startWindowMaximized = Properties.Settings.Default.StartWindowMaximized;
        public bool StartWindowMaximized
        {
            get => startWindowMaximized;
            set
            {
                if (SetProperty(ref startWindowMaximized, value))
                {
                    Properties.Settings.Default.StartWindowMaximized = value;
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
