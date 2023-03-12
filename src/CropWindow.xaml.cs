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
using Zhai.Famil.Controls;

namespace Zhai.PictureView
{
    /// <summary>
    /// CropWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CropWindow : FamilWindow
    {
        private CropWindowViewModel ViewModel => this.DataContext as CropWindowViewModel;

        internal CropWindow(Picture picture)
        {
            InitializeComponent();

            this.DataContext = new CropWindowViewModel(picture);

            this.ViewModel.PictureSourceChanged += ViewModel_PictureSourceChanged;

            this.CurrentImage.SizeChanged += CurrentImage_SizeChanged;
        }

        private void ViewModel_PictureSourceChanged(object sender, BitmapSource e)
        {
            ViewModel.UpdateImageSize(CurrentImage.ActualWidth, CurrentImage.ActualHeight);
        }

        private void CurrentImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel.UpdateImageSize(CurrentImage.ActualWidth, CurrentImage.ActualHeight);
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

        private void ScreenSelector_IndicatorDoubleClick(object sender, Rect region)
        {
            ViewModel.Crop(region);
        }

        private void ScreenSelectorOkButton_Click(object sender, RoutedEventArgs e)
        {
            var region = this.ScreenSelector.SelectionRegion;
            this.ScreenSelector.ClearSelectionRegion();

            ViewModel.Crop(region);
        }

        private void ScreenSelectorCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.ScreenSelector.ClearSelectionRegion();
        }

        private void TextNumberBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextNumberBox textNumberBox)
            {
                textNumberBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void FlopButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Flop();
        }

        private void FlipButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Flip();
        }

        private void RotateLeftButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RotateLeft();
        }

        private void RotateRightButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RotateRight();
        }

        private void CorpFinishButton_Click(object sender, RoutedEventArgs e)
        {
            CorpFinished?.Invoke(this, ViewModel.PictureBytes);

            this.Close();
        }

        public event EventHandler<byte[]> CorpFinished;
    }
}
