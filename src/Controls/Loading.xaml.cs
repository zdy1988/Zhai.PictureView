using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Zhai.PictureView.Controls
{
    public partial class Loading : UserControl
    {
        private readonly DispatcherTimer animationTimer;

        public Loading()
        {
            InitializeComponent();

            animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher)
            {
                Interval = new TimeSpan(0, 0, 0, 0, 75)
            };
        }

        private void Start()
        {
            animationTimer.Tick += HandleAnimationTick;
            animationTimer.Start();
        }

        private void Stop()
        {
            animationTimer.Stop();
            animationTimer.Tick -= HandleAnimationTick;
        }

        private void HandleAnimationTick(object sender, EventArgs e)
        {
            SpinnerRotate.Angle = (SpinnerRotate.Angle + 36) % 360;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            const double offset = Math.PI;
            const double step = Math.PI * 2 / 10.0;

            SetPosition(C0, offset, 0.0, step);
            SetPosition(C1, offset, 1.0, step);
            SetPosition(C2, offset, 2.0, step);
            SetPosition(C3, offset, 3.0, step);
            SetPosition(C4, offset, 4.0, step);
            SetPosition(C5, offset, 5.0, step);
            SetPosition(C6, offset, 6.0, step);
            SetPosition(C7, offset, 7.0, step);
            SetPosition(C8, offset, 8.0, step);
        }

        //设置动画中小圆的位置
        private void SetPosition(Ellipse ellipse, double offset,
            double posOffSet, double step)
        {
            ellipse.SetValue(Canvas.LeftProperty, 50.0
                + Math.Sin(offset + posOffSet * step) * 50.0);

            ellipse.SetValue(Canvas.TopProperty, 50
                + Math.Cos(offset + posOffSet * step) * 50.0);
        }

        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        //显示控件就启动动画，隐藏(不显示）就停止动画
        private void HandleVisibleChanged(object sender,
            DependencyPropertyChangedEventArgs e)
        {
            bool isVisible = (bool)e.NewValue;

            if (isVisible)
                Start();
            else
                Stop();
        }
    }
}
