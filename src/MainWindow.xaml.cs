using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Zhai.Famil.Controls;
using MessageBox = Zhai.Famil.Dialogs.MessageBox;
using ConfirmBox = Zhai.Famil.Dialogs.ConfirmBox;
using SkiaSharp;

namespace Zhai.PictureView
{
    public partial class MainWindow : GlassesWindow
    {
        PictureWindowViewModel ViewModel => this.DataContext as PictureWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();

            var settings = Properties.Settings.Default;

            if (settings.IsStartWindowMaximized)
            {
                this.WindowState = WindowState.Maximized;
            }

            Loaded += MainWindow_Loaded;

            ViewModel.CurrentPictureChanged += ViewModel_CurrentPictureChanged;
            ViewModel.CurrentFolderChanged += ViewModel_CurrentFolderChanged;
            ViewModel.CurrentPictureSourceUpdated += ViewModel_CurrentPictureSourceUpdated;

            PictureElement.MouseRightButtonDown += PictureElement_MouseRightButtonDown;
            VideoElement.MouseRightButtonDown += VideoElement_MouseRightButtonDown;
            VideoElement.MediaOpened += VideoElement_MediaOpened;
            VideoElement.MediaElementReady += VideoElement_MediaElementReady;

            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var arg = Application.Current.Properties["ArbitraryArgName"];

            if (arg != null)
            {
                await ViewModel.OpenPictureAsync(arg.ToString());
            }
        }

        private async void ViewModel_CurrentPictureChanged(object sender, Picture picture)
        {
            this.PictureElement.ImageElement.CleanAnimation();

            if (picture != null)
            {
                StopVideo();

                if (picture.IsVideo)
                {
                    InitVideo(picture);
                }
                else
                {
                    await InitPicture(picture);

                    if (picture.IsAnimation)
                    {
                        this.PictureElement.ImageElement.RunAnimation(picture);
                    }

                    PictureList.ScrollIntoView(picture);
                }
            }
        }

        private void ViewModel_CurrentFolderChanged(object sender, DirectoryInfo e)
        {
            FolderList.ScrollIntoView(e);
        }

        private void ViewModel_CurrentPictureSourceUpdated(object sender, Picture picture)
        {
            this.PictureElement.ImageElement.CleanAnimation();

            this.PictureElement.Source = picture.PictureSource;

            this.PictureElement.Reset();

            if (picture.IsAnimation)
            {
                this.PictureElement.ImageElement.RunAnimation(new MemoryStream(picture.PictureBytes));
            }
        }

        #region Picture View

        private void PictureElement_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            this.PictureElement.Reset();
        }

        private async Task InitPicture(Picture picture)
        {
            var renderedPicture = await picture.DrawAsync();

            if (picture != ViewModel.CurrentPicture) return;

            this.PictureElement.Source = renderedPicture;

            this.PictureElement.Reset();

            PictureCacheManager.Managed(picture);
        }

        #endregion

        #region Picture Contorllers

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = PictureSupport.Filter
            };

            if (dialog.ShowDialog() is true)
                await ViewModel.OpenPictureAsync(dialog.FileName);
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            this.PictureElement.ZoomIn();
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            this.PictureElement.ZoomOut();
        }

        private void ZoomQuickButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            this.PictureElement.ZoomQuick();
        }

        private void RotateLeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            this.PictureElement.RotateLeft();
        }

        private void RotateRightButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            this.PictureElement.RotateRight();
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
                    var navWindow = new NavWindow("Next", next)
                    {
                        Owner = App.Current.MainWindow,
                        DataContext = ViewModel.Folder.Current
                    };

                    if (navWindow.ShowDialog() == true)
                    {
                        await ViewModel.OpenPictureAsync(next, null, ViewModel.Folder.Borthers);

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
                    var navWindow = new NavWindow("Prev", prev)
                    {
                        Owner = App.Current.MainWindow,
                        DataContext = ViewModel.Folder.Current
                    };

                    if (navWindow.ShowDialog() == true)
                    {
                        await ViewModel.OpenPictureAsync(prev, null, ViewModel.Folder.Borthers);

                        return;
                    }
                }

                ViewModel.CurrentPictureIndex = ViewModel.Folder.Count - 1;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            if (ConfirmBox.Show(this, "确定删除此图片？") == true)
            {
                var deletePicture = ViewModel.CurrentPicture;

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
                    this.PictureElement.Source = null;
                }

                ViewModel.Folder.Remove(deletePicture);

                deletePicture.Cleanup();

                deletePicture.Delete();
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

            if (ViewModel.IsCanPictureCarouselPlay)
            {
                ViewModel.IsPictureCarouselPlaying = !ViewModel.IsPictureCarouselPlaying;
            }
        }

        private void AutoPlayCloseButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.IsPictureCarouselPlaying = false;
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

        private void GalleryButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.IsShowGallery = true;
        }

        private void GalleryCloseButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.IsShowGallery = false;
        }

        private void GalleryView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel.IsShowGallery = false;
        }

        private void CloseEffectsButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.IsShowPictureEffectsView = false;
        }

        #endregion

        #region Video View

        private void VideoElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            ViewModel.CurrentPicture.PixelWidth = this.VideoElement.VideoWidth;
            ViewModel.CurrentPicture.PixelHeight = this.VideoElement.VideoHeight;
        }

        private void VideoElement_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            this.VideoElement.Reset();
        }

        private void VideoElement_MediaElementReady(object sender, RoutedEventArgs e)
        {
            VideoElement.MediaElementReady -= VideoElement_MediaElementReady;

            if (ViewModel.IsCurrentPictureIsVideo)
            {
                InitVideo(ViewModel.CurrentPicture);
            }
        }

        private void InitVideo(Picture picture)
        {
            if (this.VideoElement.IsMediaElementReady)
            {
                this.VideoElement.Source = new Uri(picture.PicturePath);

                this.VideoElement.Reset();
            }
        }

        private void StopVideo()
        {
            if (this.VideoElement.IsMediaElementReady)
            {
                this.VideoElement.Stop();
            }
        }

        #endregion

        #region Video Contorllers

        private void RotateLeftButton2_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            ViewModel.RotateAngle += 90;
        }

        private void RotateRightButton2_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            ViewModel.RotateAngle -= 90;
        }

        #endregion

        #region Folder View

        private async void FolderItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is DirectoryInfo dir)
            {
                var file = dir.EnumerateFiles().Where(PictureSupport.PictureSupportExpression).OrderBy(t => t.Name).FirstOrDefault();

                if (file != null)
                {
                    await ViewModel.OpenPictureAsync(dir, file.FullName, ViewModel.Folder.Borthers);
                }
            }
        }

        private void FolderList_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
                e.Handled = true;
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
                this.PictureElement.Reset();
            }
            else if (e.Key == Key.Tab)
            {
                ViewModel.IsShowGallery = !ViewModel.IsShowGallery;
            }
            else if (e.Key == Key.Escape)
            {
                if (ViewModel.IsPictureCarouselPlaying)
                {
                    ViewModel.IsPictureCarouselPlaying = false;
                    return;
                }
                else if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                    return;
                }
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel.CurrentPicture == null) return;

            if (e.OriginalSource is not Famil.Controls.TextBox)
            {
                FocusManager.SetIsFocusScope(this, true);
                FocusManager.SetFocusedElement(this, this);

                PressShortcutKey(sender, e);
            }
        }

        #endregion

        private async void PictureWindow_Drop(object sender, DragEventArgs e)
        {
            var filename = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();

            if (File.Exists(filename) && PictureSupport.IsSupported(filename))
            {
                await ViewModel.OpenPictureAsync(filename);
            }
        }

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
    }
}
