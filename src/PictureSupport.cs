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
        internal static string[] JPEG { get; } = new string[] {
            ".jpg", ".jpeg"
        };

        internal static string[] PortableNetworkGraphic { get; } = new string[] {
            ".png"
        };

        internal static string[] GraphicsInterchangeFormat { get; } = new string[] {
            ".gif"
        };

        internal static string[] Icon { get; } = new string[] {
            ".ico"
        };

        internal static string[] Photoshop { get; } = new string[] {
            ".psd", ".psb"
        };

        internal static string[] Vector { get; } = new string[] {
            ".svg", ".svgz"
        };

        internal static string[] Camera { get; } = new string[] {
            ".3fr", ".arw", ".cr2", ".cr3", ".crw", ".dcr", ".dng", ".erf", ".kdc", ".mdc", ".mef", ".mos", ".mrw",
            ".nef", ".nrw", ".orf", ".pef", ".raf", ".raw", ".rw2", ".srf", ".x3f",
        };

        internal static string[] Others { get; } = new string[] {
            ".jpe", ".bmp", ".jfif", ".webp", ".wbmp",
            ".tif", ".tiff", ".dds", ".tga", ".heic", ".heif", ".hdr", ".xcf", ".jxl", ".jp2",
            ".b64",
            ".pgm", ".ppm", ".cut", ".exr", ".dib", ".emf", ".wmf", ".wpg", ".pcx", ".xbm", ".xpm",
        };

        internal static string[] All { get; } = (new List<IEnumerable<string>> { JPEG, PortableNetworkGraphic, GraphicsInterchangeFormat, Icon, Photoshop, Vector, Camera, Others }).Aggregate((x, y) => x.Concat(y)).ToArray();

        internal static string ToFilter(this IEnumerable<string> strings)
        {
            return String.Join(";", strings.Select(t => $"*{t}"));
        }

        internal static string Filter { get; } =
            $@"All Supported ({All.ToFilter()})|{All.ToFilter()}|
JPEG ({GraphicsInterchangeFormat.ToFilter()})|{Icon.ToFilter()}|
Portable Network Graphic ({PortableNetworkGraphic.ToFilter()})|{PortableNetworkGraphic.ToFilter()}|
Graphics Interchange Format ({GraphicsInterchangeFormat.ToFilter()})|{GraphicsInterchangeFormat.ToFilter()}|
Icon ({Icon.ToFilter()})|{Icon.ToFilter()}|
Photoshop ({Photoshop.ToFilter()})|{Photoshop.ToFilter()}|
Vector ({Vector.ToFilter()})|{Vector.ToFilter()}|
Camera ({Camera.ToFilter()})|{Camera.ToFilter()}|
Others ({Others.ToFilter()})|{Others.ToFilter()}|
All Files (*.*)|*.*";

        internal static bool IsSupported(string filename) => All.Contains(Path.GetExtension(filename).ToLower());

    }
}
