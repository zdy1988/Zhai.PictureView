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

            var files = new DirectoryInfo(filename).EnumerateFiles().Where(file => (file.Attributes & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Temporary)) == 0 && IsSupportedFile(file.FullName));

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


        internal static bool IsSupportedFile(string filename)
        {
            return Path.GetExtension(filename).ToLower() switch
            {
                // Standards
                ".jpg" or ".jpeg" or ".jpe" or ".png" or ".bmp" or ".tif" or ".tiff" or ".gif" or ".ico" or ".jfif" or ".webp" or ".wbmp" => true,

                // Non-standards

                // Photoshop
                ".psd" or ".psb" or

                // Pfim
                ".tga" or ".dds" or

                // Vector
                ".svg" or // Maybe add svgz at some point
                ".xcf" or

                // Camera
                ".3fr" or ".arw" or ".cr2" or ".crw" or ".dcr" or ".dng" or ".erf" or ".kdc" or ".mdc" or ".mef" or ".mos" or ".mrw" or ".nef" or ".nrw" or ".orf" or
                ".pef" or ".raf" or ".raw" or ".rw2" or ".srf" or ".x3f"

                // Others
                or ".pgm" or ".hdr" or ".cut" or ".exr" or ".dib" or ".heic" or ".emf" or ".wmf" or ".wpg" or ".pcx" or ".xbm" or ".xpm"

                => true,
                // Non supported
                _ => false,
            };
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
