using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Phone.Controls;

namespace PageControlSample
{
    public partial class MainPage : PhoneApplicationPage
    {
        // 构造函数
        public MainPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            var list = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(i+"");
            }
            ThePageControl.ItemsSource = list;
        }

        private void ApplicationBarIconButton_NextOnClick(object sender, EventArgs e)
        {
            ThePageControl.PageNext();
        }

        private void ApplicationBarIconButton_PreOnClick(object sender, EventArgs e)
        {
            ThePageControl.PagePre();
        }

    }
}