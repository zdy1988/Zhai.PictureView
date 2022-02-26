using System;
using System.Reflection;

namespace Zhai.PictureView
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : PictureWindow
    {
        public AboutWindow()
        {
            InitializeComponent();

            this.TextBlock_ApplicationIntPtrSize.Text = ApplicationIntPtrSize;
            this.TextBlock_Name.Text = AssemblyProduct;
            this.TextBlock_Copyright.Text = DateTime.Now.Year.ToString();
            this.TextBlock_Version.Text = AssemblyVersion;
            this.TextBlock_Description.Text = AssemblyDescription;
        }

        public string ApplicationIntPtrSize
        {
            get
            {
                if (IntPtr.Size == 8)
                {
                    // 64 bit machine
                    return "64";
                }
                else if (IntPtr.Size == 4)
                {
                    // 32 bit machine
                    return "32";
                }

                return "Unknown";
            }
        }

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);

                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];

                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }

#pragma warning disable SYSLIB0012 // 类型或成员已过时
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
#pragma warning restore SYSLIB0012 // 类型或成员已过时
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);

                if (attributes.Length == 0)
                {
                    return "";
                }

                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);

                if (attributes.Length == 0)
                {
                    return "";
                }

                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

                if (attributes.Length == 0)
                {
                    return "";
                }

                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);

                if (attributes.Length == 0)
                {
                    return "";
                }

                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
    }
}
