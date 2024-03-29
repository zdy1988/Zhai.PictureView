﻿using System;
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
    /// ExifWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExifWindow : FamilWindow
    {
        public ExifWindow()
        {
            InitializeComponent();

            this.Loaded += ExifWindow_Loaded;
        }

        private async void ExifWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await (this.DataContext as Picture).LoadExif();
        }
    }
}
