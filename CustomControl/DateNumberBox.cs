using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ModernWpf.Controls;
using System.ComponentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ModernWpf;
using System.Windows.Controls.Primitives;
using System.Diagnostics;

namespace Invoice.CustomControl
{
    public class DateNumberBox : NumberBox, INotifyPropertyChanged
    {
        // フィールド
        private TextBlock? _overlayTextBlock;
        private Popup _popup;
        private CustomCalendar _calendar;
        private static Func<DateTime> GetMinDate = () => DateTime.MinValue;
        private static Func<double, DateTime> GetMonth = (double value) => GetMinDate().AddMonths((int)value);
        public event PropertyChangedEventHandler? PropertyChanged;
        private bool _initializing = true;
        private bool _isUpdating = false;
        public event DateSelectedEventHandler? DateSelected;
        public delegate void DateSelectedEventHandler(object sender, CalendarDateChangedEventArgs e);
        private CalendarDateChangedEventArgs calendarDateChangedEventArgs;

        // コンストラクタ
        static DateNumberBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DateNumberBox), new FrameworkPropertyMetadata(typeof(DateNumberBox)));
        }

        public DateNumberBox()
        {
            this.IsEnabledChanged -= DateNumberBox_IsEnabledChanged;
            this.IsEnabledChanged += DateNumberBox_IsEnabledChanged;
            Loaded += DateNumberBox_Loaded;
            Value = GetValue(DateTime.Today);
        }

        // プロパティ
        public DateTime SelectedDate
        {
            get => (DateTime)GetValue(SelectedDateProperty);
            set
            {
                SetValue(SelectedDateProperty, GetAdjustedDate(value,ReturnDate ?? 1));
                OnPropertyChanged(nameof(SelectedDate));
                OnPropertyChanged(nameof(DateText));
                if (_calendar != null)
                {
                    _calendar.DisplayDate = value;
                    _calendar.SelectedDate = value;
                }
                if (_overlayTextBlock != null)
                {
                    _overlayTextBlock.Text = DateText;
                    var bindingExpression = _overlayTextBlock.GetBindingExpression(TextBlock.TextProperty);
                    if (bindingExpression != null)
                    {
                        bindingExpression.UpdateTarget(); // バインディングの更新を強制
                    }
                }
            }
        }


        public static readonly DependencyProperty SelectedDateProperty =
            DependencyProperty.Register(
                nameof(SelectedDate),
                typeof(DateTime),
                typeof(DateNumberBox),
                new PropertyMetadata(DateTime.Today, OnSelectedMonthChanged));

        public string DateText
        {
            get => _calendar.DisplayDate.ToString("ggy年M月", CultureInfo.CurrentCulture);
        }

        public bool PopupIsOpen
        {
            get { return (bool)GetValue(PopupIsOpenProperty); }
            set { SetValue(PopupIsOpenProperty, value); }
        }

        public static readonly DependencyProperty PopupIsOpenProperty =
            DependencyProperty.Register(
                nameof(PopupIsOpen),
                typeof(bool),
                typeof(DateNumberBox),
                new PropertyMetadata(false, OnPopupIsOpenChanged));

        public new double Value
        {
            get => (double)GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public static new readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(DateNumberBox),
                new PropertyMetadata(0.0));

        public int? ReturnDate
        {
            get { return (int?)GetValue(ReturnDateProperty); }
            set { SetValue(ReturnDateProperty, value); }
        }

        public static readonly DependencyProperty ReturnDateProperty =
            DependencyProperty.Register(
                nameof(ReturnDate),
                typeof(int?),
                typeof(CustomCalendar),
                new PropertyMetadata(1, null));



        // イベントハンドラ
        private static void OnSelectedMonthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DateNumberBox dateNumberBox)
            {
                // SelectedDate プロパティが変更されたときの処理
                if (e.NewValue is DateTime newDate)
                {
                    dateNumberBox.OnSelectedMonthChanged(newDate);
                }
            }
        }

        protected virtual void OnSelectedMonthChanged(DateTime newDate)
        {
            // ここで SelectedDate が変更されたときの処理を実装
            if (_isUpdating) 
            {
                return;
            }
            // 内部更新中の場合はスキップ

            _isUpdating = true;


            try
            {
                Value = GetValue(newDate);
                // カレンダーの選択状態を更新
                if (_calendar != null)
                {
                    _calendar.DisplayDate = newDate;
                    _calendar.SelectedDate = newDate;

                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private static void OnPopupIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DateNumberBox dateNumberBox)
            {
                // PopupIsOpen プロパティが変更されたときの処理
            }
        }

        private void _calendar_MonthSelected(object sender, CalendarDateChangedEventArgs e)
        {
            if (e == null)
            {
                PopupIsOpen = false;
                return;
            }
            if (e.AddedDate.HasValue && !_initializing)
            {
                var selectedDate = e.AddedDate.Value;
                _isUpdating = true;
                try
                {
                    SelectedDate = selectedDate;
                    PopupIsOpen = false;
                }
                finally
                {
                    _isUpdating = false;
                }
                DateSelected?.Invoke(sender, e);
            }
        }

        private void DateNumberBox_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeCalendar();
            InitializePopup();

            var textBox = this.FindDescendantByName("InputBox") as TextBox;
            textBox.Foreground = new SolidColorBrush(Colors.Transparent);
            textBox.IsHitTestVisible = false;
            var grid = textBox.Parent as Grid;
            if (grid != null)
            {
                InitializeOverlayTextBlock(grid);
                UpdateOverlayTextColor();
            }
            _calendar.MonthSelected += _calendar_MonthSelected;
            _popup.Visibility = Visibility.Hidden;
            PopupIsOpen = true;
            PopupIsOpen = false;
            _popup.Visibility = Visibility.Visible;
            _initializing = false;

        }

        private void DateNumberBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateOverlayTextColor();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // カスタムロジック
        private void UpdateOverlayTextColor()
        {
            if (_overlayTextBlock == null) return;
            _overlayTextBlock.Foreground = IsEnabled
                ? new SolidColorBrush(Colors.Black)
                : new SolidColorBrush(Colors.Gray);
        }

        private void InitializeCalendar()
        {
            _calendar = new CustomCalendar
            {
                DisplayMode = CalendarMode.Year,
                ReturnDay = 31
            };

            var binding = new Binding(nameof(Value))
            {
                Source = this,
                Mode = BindingMode.TwoWay,
                Converter = new DoubleToDateTimeConverter(),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            // DisplayDate のバインディング
            _calendar.SetBinding(CustomCalendar.DisplayDateProperty, binding);

            var returnDateBinding = new Binding(nameof(ReturnDate))
            {
                Source = this,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            _calendar.SetBinding(CustomCalendar.ReturnDayProperty, returnDateBinding);

        }

        private void InitializePopup()
        {
            _popup = new Popup
            {
                StaysOpen = true,
                PlacementTarget = this,
                Placement = PlacementMode.Bottom,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.None,
                Width = 250,
                Child = _calendar
            };
            var popupBinding = new Binding(nameof(PopupIsOpen))
            {
                Source = this,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            _popup.SetBinding(Popup.IsOpenProperty, popupBinding);
            _popup.MouseDown += (s, e) =>
            {
                e.Handled = true;
            };
            PopupIsOpen = true;
        }

        private void InitializeOverlayTextBlock(Grid grid)
        {
            if (_overlayTextBlock != null)
            {
                return;
            }
            _overlayTextBlock = new TextBlock
            {
                Name = "PART_OverlayText",
                FontWeight = FontWeight,
                FontFamily = FontFamily,
                FontSize = FontSize,
                Background = new SolidColorBrush(Colors.Transparent),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 0),
                IsHitTestVisible = true
                

            };
            var isEnableBinding = new Binding(nameof(IsEnabled))
            {
                Source = this,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            _overlayTextBlock.SetBinding(TextBlock.IsEnabledProperty, isEnableBinding);
            
            var binding = new Binding(nameof(DateText))
            {
                Source = this,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            _overlayTextBlock.SetBinding(TextBlock.TextProperty, binding);

            Grid.SetRow(_overlayTextBlock, 1);
            Grid.SetColumn(_overlayTextBlock, 0);
            grid.Children.Add(_overlayTextBlock);

            _overlayTextBlock.MouseDown += _overlayTextBlock_MouseDown; 

        }

        private void _overlayTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PopupIsOpen = true;
            Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
            e.Handled = true;
        }

        private static DateTime GetAdjustedDate(DateTime orgDate,int returnDate)
        {
            // ReturnDate を保持しつつ、DisplayDate と SelectedDate を更新
            int clampedValue = Math.Max(1, Math.Min(returnDate, 31));
            int year = orgDate.Year;
            int month = orgDate.Month;

            // 月の日数に基づいて一時的に調整
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int validDate = Math.Min(clampedValue, daysInMonth);

            // DisplayDate と SelectedDate を更新
            return new DateTime(year, month, validDate);

        }

        private static double GetValue(DateTime dateTime)
        {
            return (dateTime.Month + (dateTime.Year - GetMinDate().Year) * 12) - 2;
        }


    }

    public class DoubleToDateTimeConverter : IValueConverter
    {
        private static readonly DateTime MinDate = DateTime.MinValue;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                // double を DateTime に変換
                return MinDate.AddMonths((int)doubleValue);
            }
            return MinDate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTimeValue)
            {
                // DateTime を double に変換
                return (dateTimeValue.Year - MinDate.Year) * 12 + dateTimeValue.Month - 1;
            }
            return 0.0;
        }
    }
}
