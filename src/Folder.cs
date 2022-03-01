using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zhai.PictureView
{
    internal class Folder : ObservableCollection<Picture>, IClean
    {
        public String FolderPath { get; }

        public Folder(string filename)
        {
            FolderPath = filename;

            var files = new DirectoryInfo(filename).EnumerateFiles().Where(file => (file.Attributes & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Temporary)) == 0 && PictureSupport.IsSupported(file.FullName));

            if (files.Any())
            {
                foreach (var file in files)
                {
                    Add(new Picture(file.FullName));
                }

                LoadThumbnails();
            }
        }

        public async void LoadThumbnails()
        {
            await Task.Run(() =>
            {
                foreach (var pic in this)
                {
                    pic.DrawThumb();
                }

            }).ConfigureAwait(false);
        }


        public void Clean()
        {
            if (this.Any())
            {
                foreach (var item in this)
                {
                    item.Clean();
                }

                this.Clear();
            }
        }
    }
}
