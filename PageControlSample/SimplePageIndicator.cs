using System.Windows;

namespace PageControlSample
{
    public class SimplePageIndicator : PageIndicatorBase
    {

        public SimplePageIndicator()
        {
            DefaultStyleKey = typeof (SimplePageIndicator);
        }

        public static readonly DependencyProperty ShowTextProperty = DependencyProperty.Register(
            "ShowText", typeof (string), typeof (SimplePageIndicator), new PropertyMetadata(default(string)));

        public string ShowText
        {
            get { return (string) GetValue(ShowTextProperty); }
            private set { SetValue(ShowTextProperty, value); }
        }

        protected override void OnCountChanged(int oldValue, int newValue)
        {
            ShowText = string.Format("{0}/{1}", SelectedIndex + 1, newValue);
        }

        protected override void OnSelectedIndexChanged(int oldValue, int newValue)
        {
            ShowText = string.Format("{0}/{1}", newValue + 1, Count);
        }
    }
}
