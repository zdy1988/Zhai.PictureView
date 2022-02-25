using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Zhai.PictureView
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 当前计算机上的处理器数量
            int processorCount = Environment.ProcessorCount;

            ThreadPool.SetMinThreads(processorCount * 4, processorCount * 2);
        }
    }
}
