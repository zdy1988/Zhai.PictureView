using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class NavWindow : FamilWindow
    {
        public NavWindow(string direction, DirectoryInfo dir)
        {
            InitializeComponent();

            NavigateText.Text = direction == "Next" ? "前往下一个文件夹" : "前往上一个文件夹";
            FolderName.Text = dir.Name;
        }

        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CurrentButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
