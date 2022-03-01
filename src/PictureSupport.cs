using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zhai.PictureView
{
    internal static class PictureSupport
    {
        internal static string[] All { get; } = new string[] {

            // Standards
            ".jpg", ".jpeg", ".jpe", ".png", ".bmp", ".tif", ".tiff", ".gif", ".ico", ".jfif", ".webp", ".wbmp",

            // Non-standards

            // Photoshop
            ".psd", ".psb",

            // Pfim
            ".tga", ".dds",

            // Vector
            ".svg", // Maybe add svgz at some point
            ".xcf",

            // Camera
            ".3fr", ".arw", ".cr2", ".crw", ".dcr", ".dng", ".erf", ".kdc", ".mdc", ".mef", ".mos", ".mrw", ".nef", ".nrw", ".orf",
            ".pef", ".raf", ".raw", ".rw2", ".srf", ".x3f",

            // Others
            ".pgm", ".hdr", ".cut", ".exr", ".dib", ".heic", ".emf", ".wmf", ".wpg", ".pcx", ".xbm", ".xpm"

        };

        internal static string AllSupported = String.Join(";", All.Select(t => $"*{t}"));

        internal static string Filter { get; } =
            $@"All Supported ({AllSupported})|{AllSupported}|
JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|
Portable Network Graphic (*.png)|*.png|
Graphics Interchange Format (*.gif)|*.gif|
Icon (*.ico)|*.ico|
Photoshop (*.psd;*.psb)|*.psd;*.psb|
Pfim (*.tga;*.dds)|*.tga;*.dds|
Vector (*.svg;*.xcf)|*.svg;*.xcf|
Camera (*.3fr;*.arw;*.cr2;*.crw;*.dcr;*.dng;*.erf;*.kdc;*.mdc;*.mef;*.mos;*.mrw;*.nef;*.nrw;*.orf;*.pef;*.raf;*.raw;*.rw2;*.srf;*.x3f)|*.3fr;*.arw;*.cr2;*.crw;*.dcr;*.dng;*.erf;*.kdc;*.mdc;*.mef;*.mos;*.mrw;*.nef;*.nrw;*.orf;*.pef;*.raf;*.raw;*.rw2;*.srf;*.x3f|
Other (*.jpe;*.tif;*.jfif;*.webp;*.wbmp;*.tiff;*.wmf;*.pgm;*.hdr;*.cut;*.exr;*.dib;*.heic;*.emf;*.wpg;*.pcx;*.xbm;*.xpm)|*.jpe;*.tif;*.jfif;*.webp;*.wbmp;*.tiff;*.wmf;*.pgm;*.hdr;*.cut;*.exr;*.dib;*.heic;*.emf;*.wpg;*.pcx;*.xbm;*.xpm|
All Files (*.*)|*.*";

        internal static bool IsSupported(string filename) => All.Contains(Path.GetExtension(filename).ToLower());

    }
}
