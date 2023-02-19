using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Data;
using Zhai.Famil.Converters;

namespace Zhai.PictureView.Converters
{
    internal class FolderChildNumberConverter : ConverterMarkupExtensionBase<FolderChildNumberConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DirectoryInfo dir && dir.Exists)
            {
                var files = dir.EnumerateFiles().Where(PictureSupport.PictureSupportExpression);

                return files.Count();
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
