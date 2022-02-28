using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Zhai.PictureView.Controls
{
    public class ListDetailView : ListBox
    {
        public static readonly DependencyProperty ItemDetailTemplateProperty = DependencyProperty.Register(nameof(ItemDetailTemplate), typeof(DataTemplate), typeof(ListDetailView), new FrameworkPropertyMetadata(null));

        public DataTemplate ItemDetailTemplate
        {
            get => (DataTemplate)GetValue(ItemDetailTemplateProperty);
            set => SetValue(ItemDetailTemplateProperty, value);
        }


        static ListDetailView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListDetailView), new FrameworkPropertyMetadata(typeof(ListDetailView)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var scrollViewer = GetTemplateChild("ItemsScrollViewer") as ScrollViewer;
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;

            ItemDetail = GetTemplateChild("ItemDetail") as FrameworkElement;
            ItemDetail.SizeChanged += ItemDetail_SizeChanged;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateItemDetailPanel();
        }

        private void ItemDetail_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ItemDetailSize = e.NewSize;

            UpdateItemDetailPanel();
        }

        FrameworkElement ItemDetail;

        Size ItemDetailSize;

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (e.RemovedItems.Count > 0)
            {
                var item = this.ItemContainerGenerator.ContainerFromItem(e.RemovedItems[0]) as ListBoxItem;

                if (item.Tag != null)
                {
                    item.Margin = (Thickness)item.Tag;
                }
            }

            if (e.AddedItems.Count > 0)
            {
                UpdateItemDetailPanel(e.AddedItems[0]);
            }
        }

        private void UpdateItemDetailPanel(object currentItem = null)
        {
            ListBoxItem item;

            if (currentItem == null)
            {
                if (SelectedItem == null) return;

                item = this.ItemContainerGenerator.ContainerFromItem(SelectedItem) as ListBoxItem;
            }
            else
            {
                item = this.ItemContainerGenerator.ContainerFromItem(currentItem) as ListBoxItem;
            }

            if (item.Tag == null)
            {
                item.Tag = item.Margin;
            }

            ItemDetail.Visibility = Visibility.Collapsed;

            var detailHeight = ItemDetailSize.Height;

            var margin = (Thickness)item.Tag;

            item.Margin = new Thickness(margin.Left, margin.Right, margin.Top, margin.Bottom + detailHeight);

            item.UpdateLayout();

            var offset = item.TransformToVisual(this).Transform(new Point(0, 0));

            var height = item.RenderSize.Height;

            ItemDetail.Margin = new Thickness(0, offset.Y + height, 0, 0);

            ItemDetail.Visibility = Visibility.Visible;
        }
    }
}
