using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Zhai.PictureView
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 当前计算机上的处理器数量
            int processorCount = Environment.ProcessorCount;

            ThreadPool.SetMinThreads(processorCount * 4, processorCount * 2);

            // 打开图片
            if (e.Args.Length == 1)
            {
                MainWindow mainWindow = this.MainWindow as MainWindow;

                var filename = e.Args[0];

                if (File.Exists(filename))
                {
                    mainWindow.OpenPicture(filename);
                }
            }
        }
    }
}
