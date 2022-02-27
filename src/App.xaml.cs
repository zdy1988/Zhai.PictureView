using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
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

            HandleException();

            // 打开图片
            if (e.Args.Length == 1)
            {
                var filename = e.Args[0];

                if (File.Exists(filename))
                {
                    App.PictureWindow.OpenPicture(filename);
                }
            }
        }

        internal static MainWindow PictureWindow => App.Current.MainWindow as MainWindow;

        internal static PictureWindowViewModel PictureWindowViewModel => PictureWindow.DataContext as PictureWindowViewModel;

        private static void HandleException(Action<object, UnhandledExceptionEventArgs> centralizedExceptionHander = null)
        {
            if (centralizedExceptionHander == null)
            {
                centralizedExceptionHander = (sender, e) =>
                {
                    if (e.ExceptionObject is Exception ex)
                    {
                        Debug.WriteLine($"{nameof(Application)} - {ex.GetType().Name} : {ex.Message}");
                    }
                };
            }

            App.Current.DispatcherUnhandledException += (sender, e) =>
            {
                e.Handled = true;
                centralizedExceptionHander?.Invoke(sender, new UnhandledExceptionEventArgs(e.Exception, false));
            };

            App.Current.Dispatcher.UnhandledException += (sender, e) =>
            {
                e.Handled = true;
                centralizedExceptionHander?.Invoke(sender, new UnhandledExceptionEventArgs(e.Exception, false));
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                centralizedExceptionHander?.Invoke(sender, e);
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                e.SetObserved();
                centralizedExceptionHander?.Invoke(sender, new UnhandledExceptionEventArgs(e.Exception, false));
            };
        }
    }
}
