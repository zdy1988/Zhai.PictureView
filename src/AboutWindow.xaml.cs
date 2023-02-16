using System;
using System.Reflection;
using System.Windows;
using Zhai.Famil.Common.ExtensionMethods;
using Zhai.Famil.Controls;

namespace Zhai.PictureView
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : FamilWindow
    {
        public AboutWindow()
        {
            InitializeComponent();

            Assembly assembly = Assembly.GetExecutingAssembly();

            this.TextBlock_ApplicationIntPtrSize.Text = Application.Current.GetIntPtrSize().ToString();
            this.TextBlock_Name.Text = assembly.GetProduct();
            this.TextBlock_Copyright.Text = $"2022 - {DateTime.Now.Year}";
            this.TextBlock_Version.Text = assembly.GetFileVersion();
            this.TextBlock_Description.Text = assembly.GetDescription();
        }
    }
}
