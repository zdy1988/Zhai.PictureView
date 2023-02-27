using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Zhai.Famil.Converters;

namespace Zhai.PictureView.Converters
{
    internal class VideoDurationToStringConverter : ConverterMarkupExtensionBase<VideoDurationToStringConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var seconds = (double)value;

                return TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss");
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
