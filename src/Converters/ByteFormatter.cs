using System;
using System.Globalization;
using System.Windows.Data;

namespace Zhai.PictureView.Converters
{
    internal class ByteFormatter : IValueConverter
    {
        /// <summary>
        /// Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="format">The format.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>The converted value</returns>
        public object Convert(object value, Type targetType, object format, CultureInfo culture)
        {
            const double minKiloByte = 1024;
            const double minMegaByte = 1024 * 1024;
            const double minGigaByte = 1024 * 1024 * 1024;

            var byteCount = System.Convert.ToDouble(value);

            var suffix = "B";
            var output = 0d;

            if (byteCount >= minKiloByte)
            {
                suffix = "KB";
                output = Math.Round(byteCount / minKiloByte, 2);
            }

            if (byteCount >= minMegaByte)
            {
                suffix = "MB";
                output = Math.Round(byteCount / minMegaByte, 2);
            }

            if (byteCount >= minGigaByte)
            {
                suffix = "GB";
                output = Math.Round(byteCount / minGigaByte, 2);
            }

            if (suffix == "B")
                return $"{output:0} {suffix}";

            return $"{output:0.00} {suffix}";
        }

        /// <summary>
        /// Converts the back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="culture">The culture.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
