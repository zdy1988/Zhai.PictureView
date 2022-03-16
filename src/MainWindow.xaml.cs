using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace Zhai.PictureView
{
    public partial class MainWindow : PictureWindow
    {
        PictureWindowViewModel ViewModel => this.DataContext as PictureWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();

            InitializeMainWindow();

            Loaded += MainWindow_Loaded;

            ViewModel.CurrentPictureChanged += ViewModel_CurrentPictureChanged;
            ViewModel.ScaleChanged += ViewModel_ScaleChanged;


            PictureBox.SizeChanged += PictureBox_SizeChanged;
            PictureBox.MouseLeftButtonDown += PictureBox_MouseLeftButtonDown;
            PictureBox.MouseMove += PictureBox_MouseMove;
            PictureBox.MouseLeftButtonUp += PictureBox_MouseLeftButtonUp;
            PictureBox.MouseWheel += PictureBox_MouseWheel;
            PictureBox.MouseRightButtonDown += PictureBox_MouseRightButtonDown;

            PreviewKeyDown += MainWindow_PreviewKeyDown;

            InitSyncUpdateMoveRectTimer();

            VisualStateManager.GoToElementState(this.PictureEditView, ViewModel.IsShowPictureEditView ? "PictureEditViewShow" : "PictureEditViewHide", false);
        }

        private void InitializeMainWindow()
        {
            var settings = Properties.Settings.Default;

            if (settings.StartWindowMaximized)
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var arg = Application.Current.Properties["ArbitraryArgName"];

            if (arg != null)
            {
                await OpenPicture(arg.ToString());
            }
        }

        public async Task OpenPicture(DirectoryInfo dir, string filename = null, List<DirectoryInfo> borthers = null)
        {
            var newFolder = new Folder(dir, borthers);

            if (newFolder.IsAccessed)
            {
                var oldFolder = ViewModel.Folder;

                ViewModel.Folder = newFolder;
                await ViewModel.Folder.LoadAsync();
                ViewModel.CanPictureCarouselPlay = ViewModel.Folder?.Count > 1;
                ViewModel.CurrentPicture = (filename == null ? ViewModel.Folder : ViewModel.Folder.Where(t => t.PicturePath == filename)).FirstOrDefault();

                ThreadPool.QueueUserWorkItem(_ => this.Dispatcher.Invoke(() => oldFolder?.Clean()));

                ViewModel.IsShowPictureListView = ViewModel.Folder != null && ViewModel.Folder.Count > 1;
                VisualStateManager.GoToElementState(this.PictureListView, ViewModel.IsShowPictureListView ? "PictureListViewShow" : "PictureListViewHide", false);
            }
            else
            {
                MessageBox.Show($"软件对路径：“{dir.FullName}”没有访问权限！");
            }
        }

        public Task OpenPicture(string filename)
            => OpenPicture(Directory.GetParent(filename), filename);

        private void ViewModel_ScaleChanged(object sender, double e)
        {
            var animation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(3000))
            {
                EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };

            ScaleTips.BeginAnimation(OpacityProperty, animation, HandoffBehavior.SnapshotAndReplace);
        }

        private async void ViewModel_CurrentPictureChanged(object sender, Picture picture)
        {
            var stream = XamlAnimatedGif.AnimationBehavior.GetSourceStream(Picture);

            if (stream != null)
            {
                XamlAnimatedGif.AnimationBehavior.SetSourceUri(Picture, null);
            }

            if (picture != null)
            {
                await InitPicture(picture);

                PictureList.ScrollIntoView(picture);

                if (picture.IsAnimation)
                {
                    XamlAnimatedGif.AnimationBehavior.SetSourceUri(Picture, new Uri(picture.PicturePath));
                }
            }
        }

        private double PictureOffsetX => Canvas.GetLeft(Picture);
        private double PictureOffsetY => Canvas.GetTop(Picture);
        private double MoveRectOffsetX => Canvas.GetLeft(MoveRect);
        private double MoveRectOffsetY => Canvas.GetTop(MoveRect);
        private double ViewingAreaRatio => this.PictureBox.Width / this.MoveRect.RenderSize.Width;     //获取右侧大图框与透明矩形框的尺寸比率

        private async Task InitPicture(Picture picture)
        {
            var renderedPicture = await picture.DrawAsync();

            if (picture != ViewModel.CurrentPicture) return;

            Picture.Source = renderedPicture;
            Picture.Width = picture.PixelWidth;
            Picture.Height = picture.PixelHeight;

            EffectList.SelectedItem = ViewModel.Effects.First();

            if (picture.PixelWidth >= picture.PixelHeight)
            {
                ThumbBox.Width = 160;
                ThumbBox.Height = ThumbBox.Width / picture.PixelWidth * picture.PixelHeight;
            }
            else
            {
                ThumbBox.Height = 160;
                ThumbBox.Width = ThumbBox.Height / picture.PixelHeight * picture.PixelWidth;
            }

            ResetPicture();
        }


        double pictureWidth = 0;
        double pictureHeight = 0;

        readonly double minScale = 0.5;
        readonly double maxScale = 32;

        private void ResetPicture()
        {
            ViewModel.Scale = 1.0;

            double width;
            double height;

            double sourWidth = ViewModel.CurrentPicture.PixelWidth;
            double sourHeight = ViewModel.CurrentPicture.PixelHeight;

            double destWidth = PictureBox.Width * 0.9;
            double destHeight = PictureBox.Height * 0.9;

            if (sourHeight > destHeight || sourWidth > destWidth)
            {
                if ((sourWidth * destHeight) > (sourHeight * destWidth))
                {
                    width = destWidth;
                    height = (destWidth * sourHeight) / sourWidth;
                }
                else
                {
                    height = destHeight;
                    width = (sourWidth * destHeight) / sourHeight;
                }
            }
            else
            {
                width = sourWidth;
                height = sourHeight;
            }

            pictureWidth = width;
            pictureHeight = height;
            Picture.Width = width;
            Picture.Height = height;

            Picture.SetValue(Canvas.LeftProperty, (PictureBox.Width - Picture.Width) * 0.5);
            Picture.SetValue(Canvas.TopProperty, (PictureBox.Height - Picture.Height) * 0.5);

            UpdateMoveRect();
        }


        private void ZoomPicture(double ratio, Point mousePoint = default)
        {
            double _scale = ViewModel.Scale * ratio;
            if (_scale > maxScale)
            {
                ratio = maxScale / ViewModel.Scale;
                ViewModel.Scale = maxScale;
            }
            else if (_scale < minScale)
            {
                ratio = minScale / ViewModel.Scale;
                ViewModel.Scale = minScale;
            }
            else
            {
                ViewModel.Scale = _scale;
            }

            double originWidth = Picture.Width;
            double originHeight = Picture.Height;

            double newWidth = pictureWidth * ViewModel.Scale;
            double newHeight = pictureHeight * ViewModel.Scale;

            double x = PictureOffsetX - (newWidth - originWidth) * 0.5;
            double y = PictureOffsetY - (newHeight - originHeight) * 0.5;

            if (mousePoint != default)
            {
                Point origin = new()
                {
                    X = (ratio - 1) * newWidth * 0.5,
                    Y = (ratio - 1) * newHeight * 0.5
                };

                // 计算偏移量
                x -= (ratio - 1) * (mousePoint.X - x) - origin.X;
                y -= (ratio - 1) * (mousePoint.Y - y) - origin.Y;
            }

            Picture.Width = newWidth;
            Picture.Height = newHeight;

            Picture.SetValue(Canvas.LeftProperty, x);
            Picture.SetValue(Canvas.TopProperty, y);


            UpdateMoveRect();
        }


        private void PictureBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            ViewModel.RotateAngle = 0;

            ResetPicture();
        }

        private void PictureBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double offsetX = (e.NewSize.Width - e.PreviousSize.Width) * 0.5;
            double offsetY = (e.NewSize.Height - e.PreviousSize.Height) * 0.5;

            Picture.SetValue(Canvas.LeftProperty, PictureOffsetX + offsetX);
            Picture.SetValue(Canvas.TopProperty, PictureOffsetY + offsetY);

            UpdateMoveRect(e.NewSize.Width, e.NewSize.Height, Picture.Width, Picture.Height);
        }


        private Point startPosition;

        private void PictureBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel.IsPictureMoving = false;
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (ViewModel.IsPictureMoving)
                {
                    Point currentPosition = e.GetPosition(PictureBox);

                    double offsetX = currentPosition.X - startPosition.X;
                    double offsetY = currentPosition.Y - startPosition.Y;

                    double left = double.IsNaN(PictureOffsetX) ? 0 : PictureOffsetX + offsetX;
                    double top = double.IsNaN(PictureOffsetY) ? 0 : PictureOffsetY + offsetY;

                    if ((left < 0 && Picture.Width + left < 100) || (left > 0 && PictureBox.Width - left < 100))
                    {
                        left = PictureOffsetX;
                    }

                    if ((top < 0 && Picture.Height + top < 100) || (top > 0 && PictureBox.Height - top < 100))
                    {
                        top = PictureOffsetY;
                    }


                    Canvas.SetLeft(Picture, left);
                    Canvas.SetTop(Picture, top);

                    startPosition = currentPosition;

                    UpdateMoveRect();
                }
                else
                {
                    this.DragMove();
                }
            }
        }

        private void PictureBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Image)
            {
                ViewModel.IsPictureMoving = true;

                startPosition = e.GetPosition(PictureBox);

                if (e.ClickCount == 2)
                {
                    double scale = 4.0;

                    if (ViewModel.Scale >= scale)
                    {
                        ZoomPicture(1.0 / ViewModel.Scale, e.GetPosition(PictureBox));
                    }
                    else
                    {
                        ZoomPicture(scale, e.GetPosition(PictureBox));
                    }
                }
            }
        }

        private void PictureBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            double ratio = e.Delta < 0 ? 1 / 1.1 : 1.1;

            if (e.OriginalSource is Image)
            {
                ZoomPicture(ratio, e.GetPosition(PictureBox));
            }
            else
            {
                ZoomPicture(ratio);
            }
        }


        private Timer syncUpdateMoveRectTimer;

        private void InitSyncUpdateMoveRectTimer()
        {
            syncUpdateMoveRectTimer = new Timer(100);

            syncUpdateMoveRectTimer.Elapsed += (sender, e) =>
            {
                App.Current.Dispatcher.Invoke(() => UpdateMoveRect());

                syncUpdateMoveRectTimer.Enabled = false;
            };
        }

        private void ExecuteSyncUpdateMoveRect()
        {
            if (!syncUpdateMoveRectTimer.Enabled)
            {
                syncUpdateMoveRectTimer.Enabled = true;
            }
            else
            {
                syncUpdateMoveRectTimer.Enabled = false;
                syncUpdateMoveRectTimer.Enabled = true;
            }
        }

        private void UpdateMoveRect(double pictureBoxWidth = default, double pictureBoxHeight = default, double pictureDisplayedWidth = default, double pictureDisplayedHeight = default)
        {
            pictureBoxWidth = pictureBoxWidth == default ? PictureBox.Width : pictureBoxWidth;
            pictureBoxHeight = pictureBoxHeight == default ? PictureBox.Height : pictureBoxHeight;

            pictureDisplayedWidth = pictureDisplayedWidth == default ? pictureBoxWidth : pictureDisplayedWidth;
            pictureDisplayedHeight = pictureDisplayedHeight == default ? pictureBoxHeight : pictureDisplayedHeight;

            if (pictureBoxWidth <= 0 || pictureBoxHeight <= 0) return;

            if (PictureOffsetX > 0)
            {
                pictureDisplayedWidth = pictureBoxWidth - PictureOffsetX;

                MoveRect.SetValue(Canvas.LeftProperty, 0.0);
            }
            else
            {
                MoveRect.SetValue(Canvas.LeftProperty, -PictureOffsetX / ViewingAreaRatio);
            }

            if (PictureOffsetY > 0)
            {
                pictureDisplayedHeight = pictureBoxHeight - PictureOffsetY;

                MoveRect.SetValue(Canvas.TopProperty, 0.0);
            }
            else
            {
                MoveRect.SetValue(Canvas.TopProperty, -PictureOffsetY / ViewingAreaRatio);
            }

            double newWidth = pictureDisplayedWidth / Picture.Width * ThumbBox.Width;
            double newHeight = pictureDisplayedHeight / Picture.Height * ThumbBox.Height;

            MoveRect.Width = newWidth >= 0 ? newWidth : 0;
            MoveRect.Height = newHeight >= 0 ? newHeight : 0;


            ThumbBoxMask.RowDefinitions[0].Height = new GridLength(MoveRectOffsetY >= 0 ? MoveRectOffsetY : 0, GridUnitType.Pixel);
            ThumbBoxMask.RowDefinitions[1].Height = new GridLength(MoveRect.RenderSize.Height >= 0 ? MoveRect.RenderSize.Height : 0, GridUnitType.Pixel);
            ThumbBoxMask.ColumnDefinitions[0].Width = new GridLength(MoveRectOffsetX >= 0 ? MoveRectOffsetX : 0, GridUnitType.Pixel);
            ThumbBoxMask.ColumnDefinitions[1].Width = new GridLength(MoveRect.RenderSize.Width >= 0 ? MoveRect.RenderSize.Width : 0, GridUnitType.Pixel);

            ExecuteSyncUpdateMoveRect();
        }

        private void MoveRect_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateMoveRect();

                //计算鼠标在X轴的移动距离
                double deltaH = e.GetPosition(MoveRect).X - this.MoveRect.RenderSize.Width * 0.5;
                //计算鼠标在Y轴的移动距离
                double deltaV = e.GetPosition(MoveRect).Y - this.MoveRect.RenderSize.Height * 0.5;

                //得到图片Top新位置
                double newTop = deltaV + MoveRectOffsetY;
                //得到图片Left新位置
                double newLeft = deltaH + MoveRectOffsetX;

                //边界的判断
                if (newLeft <= 0)
                {
                    newLeft = 0;
                }

                //左侧图片框宽度 - 半透明矩形框宽度
                if (newLeft >= (this.ThumbBox.Width - this.MoveRect.RenderSize.Width))
                {
                    newLeft = this.ThumbBox.Width - this.MoveRect.RenderSize.Width;
                }

                if (newTop <= 0)
                {
                    newTop = 0;
                }

                //左侧图片框高度度 - 半透明矩形框高度度
                if (newTop >= this.ThumbBox.Height - this.MoveRect.RenderSize.Height)
                {
                    newTop = this.ThumbBox.Height - this.MoveRect.RenderSize.Height;
                }

                MoveRect.SetValue(Canvas.TopProperty, newTop);
                MoveRect.SetValue(Canvas.LeftProperty, newLeft);

                //计算和设置原图在大图框中的Canvas.Left 和 Canvas.Top
                if (MoveRect.RenderSize.Width < ThumbBox.Width)
                {
                    Picture.SetValue(Canvas.LeftProperty, -MoveRectOffsetX * ViewingAreaRatio);
                }

                if (MoveRect.RenderSize.Height < ThumbBox.Height)
                {
                    Picture.SetValue(Canvas.TopProperty, -MoveRectOffsetY * ViewingAreaRatio);
                }
            }
        }


        #region Contorllers

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = PictureSupport.Filter
            };

            if (dialog.ShowDialog() is true)
                await OpenPicture(dialog.FileName);
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            ZoomPicture(1.1);
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            ZoomPicture(1 / 1.1);
        }

        private void ZoomQuickButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            if (ViewModel.Scale == 1.0)
            {
                ZoomPicture(ViewModel.CurrentPicture.PixelWidth / Picture.Width);
            }
            else
            {
                ResetPicture();
            }
        }

        private void RotateLeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            ResetPicture();

            ViewModel.RotateAngle += 90;
        }

        private void RotateRightButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            ResetPicture();

            ViewModel.RotateAngle -= 90;
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Folder == null || !ViewModel.Folder.Any()) return;

            var index = ViewModel.CurrentPictureIndex + 1;

            if (index <= ViewModel.Folder.Count - 1)
            {
                ViewModel.CurrentPictureIndex = index;
            }
            else
            {
                var canNextFolder = ViewModel.Folder.GetNext(out DirectoryInfo next);

                if (canNextFolder)
                {
                    var navWindow = new NavWindow("Next")
                    {
                        Owner = App.Current.MainWindow,
                        DataContext = ViewModel.Folder.Current
                    };

                    if (navWindow.ShowDialog() == true)
                    {
                        await OpenPicture(next, null, ViewModel.Folder.Borthers);

                        return;
                    }
                }

                ViewModel.CurrentPictureIndex = 0;
            }
        }

        private async void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Folder == null || !ViewModel.Folder.Any()) return;

            var index = ViewModel.CurrentPictureIndex - 1;

            if (index >= 0)
            {
                ViewModel.CurrentPictureIndex = index;
            }
            else
            {
                var canPrevFolder = ViewModel.Folder.GetPrev(out DirectoryInfo prev);

                if (canPrevFolder)
                {
                    var navWindow = new NavWindow("Prev")
                    {
                        Owner = App.Current.MainWindow,
                        DataContext = ViewModel.Folder.Current
                    };

                    if (navWindow.ShowDialog() == true)
                    {
                        await OpenPicture(prev, null, ViewModel.Folder.Borthers);

                        return;
                    }
                }

                ViewModel.CurrentPictureIndex = ViewModel.Folder.Count - 1;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            if (MessageBox.Show("确定删除此图片？", "删除", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                var deletePicture = ViewModel.CurrentPicture;

                var deletePath = deletePicture.PicturePath;

                var index = ViewModel.CurrentPictureIndex + 1;

                if (index > ViewModel.Folder.Count - 1)
                {
                    index = ViewModel.CurrentPictureIndex - 1;
                }

                if (index >= 0 && index <= ViewModel.Folder.Count - 1)
                {
                    ViewModel.CurrentPictureIndex = index;
                }
                else
                {
                    Picture.Source = null;
                }

                ViewModel.Folder.Remove(deletePicture);

                deletePicture.Clean();

                try
                {
                    File.Delete(deletePath);
                }
                catch { }
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            using (var p = new System.Diagnostics.Process())
            {
                p.StartInfo.FileName = ViewModel.CurrentPicture.PicturePath;
                p.StartInfo.Verb = "print";
                p.StartInfo.UseShellExecute = true;
                p.Start();
            }
        }

        private void AutoPlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Folder == null || ViewModel.Folder.Count <= 1) return;

            if (ViewModel.CanPictureCarouselPlay)
            {
                ViewModel.IsPictureCarouselPlaing = !ViewModel.IsPictureCarouselPlaing;
            }
        }

        private void AdjustButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Folder == null || ViewModel.Folder.Count <= 0) return;

            if (ViewModel.CurrentPicture != null)
            {
                ViewModel.IsShowPictureEditView = !ViewModel.IsShowPictureEditView;

                VisualStateManager.GoToElementState(this.PictureEditView, ViewModel.IsShowPictureEditView ? "PictureEditViewShow" : "PictureEditViewHide", true);
            }
            else
            {
                VisualStateManager.GoToElementState(this.PictureEditView, "PictureEditViewHide", false);
            }
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            var window = new ExifWindow
            {
                Owner = App.Current.MainWindow,
                DataContext = ViewModel.CurrentPicture
            };

            window.ShowDialog();
        }

        private void PictureListViewToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Folder == null || ViewModel.Folder.Count <= 0) return;

            if (ViewModel.CurrentPicture != null)
            {
                ViewModel.IsShowPictureListView = !ViewModel.IsShowPictureListView;

                VisualStateManager.GoToElementState(this.PictureListView, ViewModel.IsShowPictureListView ? "PictureListViewShow" : "PictureListViewHide", true);
            }
            else
            {
                VisualStateManager.GoToElementState(this.PictureListView, "PictureListViewHide", false);
            }
        }

        #endregion

        #region Shortcuts

        public void PressShortcutKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                PrevButton_Click(null, null);
            }
            else if (e.Key == Key.Right)
            {
                NextButton_Click(null, null);
            }
            else if (e.Key == Key.Up || e.Key == Key.OemPlus)
            {
                ZoomInButton_Click(null, null);
            }
            else if (e.Key == Key.Down || e.Key == Key.OemMinus)
            {
                ZoomOutButton_Click(null, null);
            }
            else if (e.Key == Key.OemPeriod)
            {
                RotateRightButton_Click(null, null);
            }
            else if (e.Key == Key.OemComma)
            {
                RotateLeftButton_Click(null, null);
            }
            else if (e.Key == Key.Back)
            {
                ResetPicture();
            }
            else if (e.Key == Key.Tab)
            { 
                ViewModel.IsShowGallery = !ViewModel.IsShowGallery;
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            FocusManager.SetIsFocusScope(this, true);
            FocusManager.SetFocusedElement(this, this);

            PressShortcutKey(sender, e);
        }

        #endregion

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AboutWindow
            {
                Owner = App.Current.MainWindow
            };

            window.ShowDialog();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new SettingsWindow
            {
                Owner = App.Current.MainWindow
            };

            window.ShowDialog();
        }

        private async void PictureWindow_Drop(object sender, DragEventArgs e)
        {
            var filename = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();

            if (File.Exists(filename) && PictureSupport.IsSupported(filename))
            {
                await OpenPicture(filename);
            }
        }
    }
}
