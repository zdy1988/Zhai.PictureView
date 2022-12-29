using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Zhai.FamilTheme.Converters;

namespace Zhai.PictureView.Converters
{
    internal class ScaleTipsConverter : ConverterMarkupExtensionBase<ScaleTipsConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double scale)
            {
                return $"{System.Convert.ToInt32(Math.Round(scale, 1) * 100)}%";
            }

            return "100%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
