using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Zhai.Famil.Converters;

namespace Zhai.PictureView.Converters
{
    internal class KeywordToListItemVisibilityConverter : ConverterMarkupExtensionBase<KeywordToListItemVisibilityConverter>, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2)
            {
                string keyword = values[0] == null ? "" : values[0].ToString();
                string name = values[1] == null ? "" : values[1].ToString();

                if (!string.IsNullOrWhiteSpace(keyword) && !string.IsNullOrWhiteSpace(name))
                {
                    if (name.IndexOf(keyword) != -1)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
