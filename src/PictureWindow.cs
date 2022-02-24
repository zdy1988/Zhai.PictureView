using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Zhai.PictureView
{
    public class PictureWindow : Window
    {
        public const int WM_SYSCOMMAND = 0x112;
        public HwndSource HwndSource;

        public enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        public FrameworkElement TitleBar
        {
            get { return (FrameworkElement)GetValue(TitleBarProperty); }
            set { SetValue(TitleBarProperty, value); }
        }

        public static readonly DependencyProperty TitleBarProperty = DependencyProperty.Register(nameof(TitleBar), typeof(FrameworkElement), typeof(PictureWindow), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public PictureWindow()
        {
            this.SourceInitialized += PictureWindow_SourceInitialized;
            this.Loaded += PictureWindow_Loaded;
        }

        private void PictureWindow_SourceInitialized(object sender, EventArgs e)
        {
            this.HwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
        }

        private void PictureWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var template = App.Current.Resources["PictureWindowTemplete"] as ControlTemplate;

            if (template != null)
            {
                //var TopLeft = customWindowTemplate.FindName("ResizeTopLeft", this) as Rectangle;
                //TopLeft.MouseMove += ResizePressed;
                //TopLeft.MouseDown += ResizePressed;
                //var Top = customWindowTemplate.FindName("ResizeTop", this) as Rectangle;
                //Top.MouseMove += ResizePressed;
                //Top.MouseDown += ResizePressed;
                //var TopRight = customWindowTemplate.FindName("ResizeTopRight", this) as Rectangle;
                //TopRight.MouseMove += ResizePressed;
                //TopRight.MouseDown += ResizePressed;
                //var Left = customWindowTemplate.FindName("ResizeLeft", this) as Rectangle;
                //Left.MouseMove += ResizePressed;
                //Left.MouseDown += ResizePressed;
                var Right = template.FindName("ResizeRight", this) as Rectangle;
                Right.MouseMove += ResizePressed;
                Right.MouseDown += ResizePressed;
                //var BottomLeft = customWindowTemplate.FindName("ResizeBottomLeft", this) as Rectangle;
                //BottomLeft.MouseMove += ResizePressed;
                //BottomLeft.MouseDown += ResizePressed;
                var Bottom = template.FindName("ResizeBottom", this) as Rectangle;
                Bottom.MouseMove += ResizePressed;
                Bottom.MouseDown += ResizePressed;
                var BottomRight = template.FindName("ResizeBottomRight", this) as Rectangle;
                BottomRight.MouseMove += ResizePressed;
                BottomRight.MouseDown += ResizePressed;


                var TitleBar = template.FindName("TitleBar", this) as DockPanel;
                TitleBar.MouseMove += (s, e) =>
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        this.DragMove();
                    }
                };

                var MinimizeButton = template.FindName("MinimizeButton", this) as Button;
                MinimizeButton.Click += (s, e) => SystemCommands.MinimizeWindow(this);
                var MaximizeButton = template.FindName("MaximizeButton", this) as Button;
                MaximizeButton.Click += (s, e) => SystemCommands.MaximizeWindow(this);
                var RestoreButton = template.FindName("RestoreButton", this) as Button;
                RestoreButton.Click += (s, e) => SystemCommands.RestoreWindow(this);
                var CloseButton = template.FindName("CloseButton", this) as Button;
                CloseButton.Click += (s, e) => SystemCommands.CloseWindow(this);

                var PinButton = template.FindName("PinButton", this) as Button;
                PinButton.Click += (s, e) => this.Topmost = !this.Topmost;
            }
        }

        public void ResizePressed(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            ResizeDirection direction = (ResizeDirection)Enum.Parse(typeof(ResizeDirection), element.Name.Replace("Resize", ""));

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ResizeWindow(direction);
            }
        }

        public void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(HwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
        }
    }
}
