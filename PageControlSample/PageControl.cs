using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PageControlSample
{
    
    enum PageControlAnimeDirection
    {
        Pre = -1,
        None = 0,
        Next = 1
    }

    [TemplatePart(Name = ContainerName,Type = typeof(Canvas))]
    [TemplatePart(Name = Item0Name, Type = typeof(PageControlItem))]
    [TemplatePart(Name = Item1Name, Type = typeof(PageControlItem))]
    [TemplatePart(Name = Item2Name, Type = typeof(PageControlItem))]
    public class PageControl:ItemsControl
    {

        #region Template

        private const string ContainerName = "Container";

        private Canvas _container;

        private const string Item0Name = "Item0";
        private const string Item1Name = "Item1";
        private const string Item2Name = "Item2";

        private PageControlItem _item0,_item1,_item2;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _container = GetTemplateChild(ContainerName) as Canvas;
            _item0 = GetTemplateChild(Item0Name) as PageControlItem;
            _item1 = GetTemplateChild(Item1Name) as PageControlItem;
            _item2 = GetTemplateChild(Item2Name) as PageControlItem;
            SizeChanged += (sender, e) =>
            {
                _item0.Width = _item1.Width = _item2.Width = e.NewSize.Width;
                _item0.Height = _item1.Height = _item2.Height = e.NewSize.Height;
                SetClip(e.NewSize);
            };
            PrepareItems();
            AfterPageToCurrentPage();
        }

        #endregion

        private PageControlItem _itemPre, _itemCurrent, _itemNext;

        public PageControl()
        {
            DefaultStyleKey = typeof (PageControl);
        }

        #region 属性

        private bool GestureEnabled
        {
            get { return Items.Count > 1; }
        }

        #region 依赖属性

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof (Orientation), typeof (PageControl), new PropertyMetadata(default(Orientation)));

        public Orientation Orientation
        {
            get { return (Orientation) GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex", typeof (int), typeof (PageControl), new PropertyMetadata(default(int), OnSelectedIndexChanged));

        public int SelectedIndex
        {
            get { return (int) GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        private static void OnSelectedIndexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((PageControl) obj).PrepareItems();
            ((PageControl) obj).AfterPageToCurrentPage();
            ((PageControl) obj).UpdateIndicator();
        }

        public static readonly DependencyProperty ClipToBoundsProperty = DependencyProperty.Register(
            "ClipToBounds", typeof(bool), typeof(PageControl), new PropertyMetadata(default(bool), OnClipToBoundsChanged));

        public bool ClipToBounds
        {
            get { return (bool) GetValue(ClipToBoundsProperty); }
            set { SetValue(ClipToBoundsProperty, value); }
        }

        static void OnClipToBoundsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((PageControl) obj).SetClip(new Size(((PageControl) obj).ActualWidth, ((PageControl) obj).ActualHeight));
        }

        public static readonly DependencyProperty PageIndicatorProperty = DependencyProperty.Register(
            "PageIndicator", typeof (PageIndicatorBase), typeof (PageControl), new PropertyMetadata(null,OnPageIndicatorChanged));

        public PageIndicatorBase PageIndicator
        {
            get { return (PageIndicatorBase) GetValue(PageIndicatorProperty); }
            set { SetValue(PageIndicatorProperty, value); }
        }

        static void OnPageIndicatorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            //((PageControl)obj).UpdateIndicator();
        }

        #endregion

        #endregion

        #region Override

        #region 手势

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);
            if (!GestureEnabled)
            {
                return;
            }
            if (Orientation == Orientation.Horizontal)
            {
                var totalTranslation = e.CumulativeManipulation.Translation.X;
                if (totalTranslation > ActualWidth)
                {
                    totalTranslation = ActualWidth;
                }
                else if (totalTranslation < -ActualWidth)
                {
                    totalTranslation = -ActualWidth;
                }
                _itemCurrent.RenderTransform = new TranslateTransform {X = totalTranslation};
                _itemNext.RenderTransform = new TranslateTransform {X = ActualWidth + totalTranslation};
                _itemPre.RenderTransform = new TranslateTransform {X = -ActualWidth + totalTranslation};
            }
            else
            {
                var totalTranslation = e.CumulativeManipulation.Translation.Y;
                if (totalTranslation > ActualHeight)
                {
                    totalTranslation = ActualHeight;
                }
                else if (totalTranslation < -ActualHeight)
                {
                    totalTranslation = -ActualHeight;
                }
                _itemCurrent.RenderTransform = new TranslateTransform {Y = totalTranslation};
                _itemNext.RenderTransform = new TranslateTransform {Y = ActualHeight + totalTranslation};
                _itemPre.RenderTransform = new TranslateTransform {Y = -ActualHeight + totalTranslation};
            }
        }

        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            base.OnManipulationCompleted(e);
            if (!GestureEnabled)
            {
                return;
            }
            var direction = PageControlAnimeDirection.None;
            if (Orientation == Orientation.Horizontal)
            {
                var totalTranslation = e.TotalManipulation.Translation.X;
                if (e.IsInertial)
                {
                    totalTranslation += CalculateTranslationWithSpeed(e.FinalVelocities.LinearVelocity.X);
                }
                if (totalTranslation > ActualWidth)
                {
                    totalTranslation = ActualWidth;
                }
                else if (totalTranslation < -ActualWidth)
                {
                    totalTranslation = -ActualWidth;
                }
                if (totalTranslation*2 >= ActualWidth)
                {
                    direction = PageControlAnimeDirection.Pre;
                }
                else if (totalTranslation*2 <= -ActualWidth)
                {
                    direction = PageControlAnimeDirection.Next;
                }
            }
            else
            {
                var totalTranslation = e.TotalManipulation.Translation.Y;
                if (e.IsInertial)
                {
                    totalTranslation += CalculateTranslationWithSpeed(e.FinalVelocities.LinearVelocity.Y);
                }
                if (totalTranslation > ActualHeight)
                {
                    totalTranslation = ActualHeight;
                }
                else if (totalTranslation < -ActualHeight)
                {
                    totalTranslation = -ActualHeight;
                }

                if (totalTranslation*2 >= ActualHeight)
                {
                    direction = PageControlAnimeDirection.Pre;
                }
                else if (totalTranslation*2 <= -ActualHeight)
                {
                    direction = PageControlAnimeDirection.Next;
                }
            }
            PageToCurrentPage(direction);
        }

        #endregion

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            if (SelectedIndex >= Items.Count)
            {
                SelectedIndex = Items.Count - 1;
            }
            else
            {
                UpdateIndicator();
            }
            PrepareItems();
            AfterPageToCurrentPage();
        }

        #endregion

        #region 功能函数

        void SetClip(Size size)
        {
            if (_container == null)
            {
                return;
            }
            _container.Clip = ClipToBounds ? new RectangleGeometry { Rect = new Rect(new Point(0, 0), size) } : null;
        }

        void UpdateIndicator()
        {
            if (PageIndicator != null)
            {
                PageIndicator.Count = Items.Count;
                PageIndicator.SelectedIndex = SelectedIndex;
            }
        }

        /// <summary>
        /// 计算惯性带来的位移
        /// </summary>
        /// <param name="speed">初始速度</param>
        /// <returns>总位移</returns>
        private double CalculateTranslationWithSpeed(double speed)
        {
            const double t = 0.5;
            double a = speed/t;
            return a*t*t/2;
        }

        /// <summary>
        /// 为每个Item准备数据
        /// </summary>
        private void PrepareItems()
        {
            switch (SelectedIndex%3)
            {
                case 0:
                    _itemCurrent = _item0;
                    _itemNext = _item1;
                    _itemPre = _item2;
                    break;
                case 1:
                    _itemCurrent = _item1;
                    _itemNext = _item2;
                    _itemPre = _item0;
                    break;
                case 2:
                    _itemCurrent = _item2;
                    _itemNext = _item0;
                    _itemPre = _item1;
                    break;
            }
            Canvas.SetZIndex(_itemCurrent, 1);
            Canvas.SetZIndex(_itemNext, 0);
            Canvas.SetZIndex(_itemPre, 0);
            if (Items.Count > 0)
            {
                _itemCurrent.Visibility = _itemNext.Visibility = _itemPre.Visibility = Visibility.Visible;
                _itemCurrent.Content = Items[(SelectedIndex + Items.Count)%Items.Count];
                _itemNext.Content = Items[(SelectedIndex + Items.Count + 1)%Items.Count];
                _itemPre.Content = Items[(SelectedIndex + Items.Count - 1)%Items.Count];
            }
            else
            {
                _itemCurrent.Visibility = _itemNext.Visibility = _itemPre.Visibility = Visibility.Collapsed;
                _itemCurrent.Content = _itemNext.Content = _itemPre.Content = null;
            }
        }

        /// <summary>
        /// 完成页面切换动画
        /// </summary>
        /// <param name="direction">动画方向</param>
        private void PageToCurrentPage(PageControlAnimeDirection direction)
        {
            var targetIndex = SelectedIndex;
            var sb = new Storyboard();
            var storyBoardTargetName = Orientation == Orientation.Horizontal ? "(UIElement.RenderTransform).(TranslateTransform.X)" : "(UIElement.RenderTransform).(TranslateTransform.Y)";
            var duration = TimeSpan.FromSeconds(direction == PageControlAnimeDirection.None ? 0.2 : 0.4);
            EasingFunctionBase animeEasingFunction = new QuinticEase {EasingMode = EasingMode.EaseOut};
            var gestureEnabled = IsHitTestVisible;
            IsHitTestVisible = false;
            if (direction == PageControlAnimeDirection.Next)
            {
                targetIndex = (targetIndex + 1)%Items.Count;
                var timeline = new DoubleAnimation
                {
                    EasingFunction = animeEasingFunction,
                    To = Orientation == Orientation.Horizontal ? -ActualWidth : -ActualHeight,
                    Duration = duration
                };
                Storyboard.SetTarget(timeline, _itemCurrent);
                Storyboard.SetTargetProperty(timeline, new PropertyPath(storyBoardTargetName));
                sb.Children.Add(timeline);
                timeline = new DoubleAnimation
                {
                    EasingFunction = animeEasingFunction,
                    To = 0,
                    Duration = duration
                };
                Storyboard.SetTarget(timeline, _itemNext);
                Storyboard.SetTargetProperty(timeline, new PropertyPath(storyBoardTargetName));
                sb.Children.Add(timeline);
            }
            else if (direction == PageControlAnimeDirection.Pre)
            {
                targetIndex = (targetIndex - 1 + Items.Count)%Items.Count;
                var timeline = new DoubleAnimation
                {
                    EasingFunction = animeEasingFunction,
                    To = Orientation == Orientation.Horizontal ? ActualWidth : ActualHeight,
                    Duration = duration
                };
                Storyboard.SetTarget(timeline, _itemCurrent);
                Storyboard.SetTargetProperty(timeline, new PropertyPath(storyBoardTargetName));
                sb.Children.Add(timeline);
                timeline = new DoubleAnimation
                {
                    EasingFunction = animeEasingFunction,
                    To = 0,
                    Duration = duration
                };
                Storyboard.SetTarget(timeline, _itemPre);
                Storyboard.SetTargetProperty(timeline, new PropertyPath(storyBoardTargetName));
                sb.Children.Add(timeline);
            }
            else
            {
                var timeline = new DoubleAnimation
                {
                    EasingFunction = animeEasingFunction,
                    To = 0,
                    Duration = duration
                };
                Storyboard.SetTarget(timeline, _itemCurrent);
                Storyboard.SetTargetProperty(timeline, new PropertyPath(storyBoardTargetName));
                sb.Children.Add(timeline);
                timeline = new DoubleAnimation
                {
                    EasingFunction = animeEasingFunction,
                    To = Orientation == Orientation.Horizontal ? ActualWidth : ActualHeight,
                    Duration = duration
                };
                Storyboard.SetTarget(timeline, _itemNext);
                Storyboard.SetTargetProperty(timeline, new PropertyPath(storyBoardTargetName));
                sb.Children.Add(timeline);
                timeline = new DoubleAnimation
                {
                    EasingFunction = animeEasingFunction,
                    To = Orientation == Orientation.Horizontal ? -ActualWidth : -ActualHeight,
                    Duration = duration
                };
                Storyboard.SetTarget(timeline, _itemPre);
                Storyboard.SetTargetProperty(timeline, new PropertyPath(storyBoardTargetName));
                sb.Children.Add(timeline);
            }
            sb.Completed += (sender, e) => 
            {
                var needDoFinishJobBySelf = SelectedIndex == targetIndex;
                SelectedIndex = targetIndex;
                if (needDoFinishJobBySelf)
                {
                    PrepareItems();
                    AfterPageToCurrentPage();
                }
                IsHitTestVisible = gestureEnabled;
            };
            sb.Begin();
        }

        /// <summary>
        /// 页面切换动画后执行的函数
        /// </summary>
        private void AfterPageToCurrentPage()
        {
            _itemCurrent.RenderTransform = new TranslateTransform();
            _itemNext.RenderTransform = Orientation == Orientation.Horizontal ? new TranslateTransform {X = ActualWidth} : new TranslateTransform {Y = ActualHeight};
            _itemPre.RenderTransform = Orientation == Orientation.Horizontal ? new TranslateTransform {X = -ActualWidth} : new TranslateTransform {Y = -ActualHeight};
        }

        #endregion

    }
}
