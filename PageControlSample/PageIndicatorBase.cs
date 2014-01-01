using System.Windows;
using System.Windows.Controls;

namespace PageControlSample
{
    public abstract class PageIndicatorBase : Control
    {

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register(
            "Count", typeof (int), typeof (PageIndicatorBase), new PropertyMetadata(default(int), OnCountChanged));

        public int Count
        {
            get { return (int) GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        private static void OnCountChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((PageIndicatorBase) obj).OnCountChanged((int) e.OldValue, (int) e.NewValue);
        }

        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex", typeof (int), typeof (PageIndicatorBase), new PropertyMetadata(default(int), OnSelectedIndexChanged));

        public int SelectedIndex
        {
            get { return (int) GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        private static void OnSelectedIndexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((PageIndicatorBase) obj).OnSelectedIndexChanged((int) e.OldValue, (int) e.NewValue);
        }

        protected abstract void OnCountChanged(int oldValue, int newValue);

        protected abstract void OnSelectedIndexChanged(int oldValue, int newValue);

    }
}
