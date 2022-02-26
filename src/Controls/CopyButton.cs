using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Zhai.PictureView.Controls
{
    public class CopyButton : Button
    {
        static CopyButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CopyButton), new FrameworkPropertyMetadata(typeof(CopyButton)));
        }

        public static readonly DependencyProperty CopyTextProperty = DependencyProperty.Register(nameof(CopyText), typeof(string), typeof(CopyButton), new PropertyMetadata(OnCopyTextChanged));

        public string CopyText
        {
            get => (string)GetValue(CopyTextProperty);
            set => SetValue(CopyTextProperty, value);
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(Geometry), typeof(CopyButton), new PropertyMetadata(null));

        public Geometry Icon
        {
            get => (Geometry)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(nameof(Size), typeof(double), typeof(CopyButton));

        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        private static void OnCopyTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CopyButton button && !string.IsNullOrWhiteSpace(button.CopyText))
            {
                button.Click += Button_Click;
            }
        }

        private static void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CopyButton button && !string.IsNullOrWhiteSpace(button.CopyText))
            {
                Clipboard.SetText(button.CopyText);
            }
        }
    }
}
