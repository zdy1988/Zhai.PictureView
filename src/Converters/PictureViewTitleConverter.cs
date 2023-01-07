using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Zhai.Famil.Converters;

namespace Zhai.PictureView.Converters
{
    internal class PictureViewTitleConverter : ConverterMarkupExtensionBase<PictureViewTitleConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string title = $" {Properties.Settings.Default.AppName}";

            if (value is Picture picture)
                return $" {picture.Name} -{title}";

            return title;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
