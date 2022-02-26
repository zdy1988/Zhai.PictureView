using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Zhai.PictureView.Controls
{
    public class LinkButton : Button
    {
        static LinkButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LinkButton), new FrameworkPropertyMetadata(typeof(LinkButton)));
        }

        public static readonly DependencyProperty HoverForegroundProperty = DependencyProperty.Register(nameof(HoverForeground), typeof(Brush), typeof(LinkButton));

        public Brush HoverForeground
        {
            get => (Brush)GetValue(HoverForegroundProperty);
            set => SetValue(HoverForegroundProperty, value);
        }

        public static readonly DependencyProperty NavigateUriProperty = DependencyProperty.Register(nameof(NavigateUri), typeof(string), typeof(LinkButton), new PropertyMetadata(OnNavigateUriChanged));

        public string NavigateUri
        {
            get => (string)GetValue(NavigateUriProperty);
            set => SetValue(NavigateUriProperty, value);
        }

        private static void OnNavigateUriChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is LinkButton linkButton)
            {
                if (!string.IsNullOrWhiteSpace(linkButton.NavigateUri))
                {
                    if (linkButton.Content == null)
                    {
                        linkButton.Content = linkButton.NavigateUri;
                    }

                    linkButton.Command = null;
                    linkButton.CommandParameter = null;
                    linkButton.Click -= LinkButton_Click;
                    linkButton.Click += LinkButton_Click;
                }
            }
        }

        private static void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is LinkButton linkButton)
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = linkButton.NavigateUri.Trim(),
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
                }
                catch
                {

                }
            }

            e.Handled = true;
        }
    }
}
