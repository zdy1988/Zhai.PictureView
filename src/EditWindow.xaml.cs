using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Zhai.Famil.Common.Mvvm.Command;
using Zhai.Famil.Controls;

namespace Zhai.PictureView
{
    /// <summary>
    /// EditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditWindow : FamilWindow
    {
        private EditWindowViewModel ViewModel => this.DataContext as EditWindowViewModel;

        public EditWindow()
        {
            InitializeComponent();

            var picture = new Picture("E:\\Photo\\2016.08.21 香山\\GOPR3520.jpg");

            this.DataContext = new EditWindowViewModel(picture);
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var element = sender as FrameworkElement;
            var point = new Point(element.ActualWidth / 2.0, element.ActualHeight / 2.0);
            //滚轮滚动时控制 放大的倍数,没有固定的值，可以根据需要修改。
            double scale = e.Delta * 0.002;

            ZoomImage(ImageContainer.RenderTransform as TransformGroup, point, scale);
        }

        private void ZoomImage(TransformGroup group, Point point, double scale)
        {
            Point pointToContent = group.Inverse.Transform(point);

            ScaleTransform scaleT = group.Children[0] as ScaleTransform;

            if (scaleT.ScaleX + scale <= 0.5) return;

            scaleT.ScaleX += scale;

            scaleT.ScaleY += scale;

            TranslateTransform translateT = group.Children[1] as TranslateTransform;

            translateT.X = -1 * ((pointToContent.X * scaleT.ScaleX) - point.X);

            translateT.Y = -1 * ((pointToContent.Y * scaleT.ScaleY) - point.Y);
        }

        private void Slider2_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Slider2 slider)
            {
                slider.Value = 0;
            }
        }
    }
}
