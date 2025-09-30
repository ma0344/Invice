using System.Windows;
using System.Windows.Controls;

namespace Invoice.CustomControl
{
    public class CustomCalendar : Calendar
    {
        public delegate void MonthSelectedEventHandler(object sender, CalendarDateChangedEventArgs e);
        public event  MonthSelectedEventHandler? MonthSelected;
        private bool _isHandlingEvent = false;
        public int? ReturnDay
        {
            get { return (int?)GetValue(ReturnDayProperty); }
            set { SetValue(ReturnDayProperty, value); }
        }
        static CustomCalendar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomCalendar), new FrameworkPropertyMetadata(typeof(CustomCalendar)));
        }
        public CustomCalendar()
        {
            this.DisplayDateChanged -= CustomCalendar_DisplayDateChanged;
            this.DisplayDateChanged += CustomCalendar_DisplayDateChanged;
            this.DisplayModeChanged -= CustomCalendar_DisplayModeChanged;
            this.DisplayModeChanged += CustomCalendar_DisplayModeChanged;
            Loaded += CustomCalendar_Loaded;
            DisplayMode = CalendarMode.Year;
        }

        public static readonly DependencyProperty ReturnDayProperty = DependencyProperty.Register(
                                                                            nameof(ReturnDay),
                                                                            typeof(int?),
                                                                            typeof(CustomCalendar),
                                                                            new PropertyMetadata(1, OnReturnDayChanged, CorectReturnDate));

        private static void OnReturnDayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CustomCalendar calendar)
            {
                if (e.NewValue is int newValue)
                {
                    // ReturnDate を保持しつつ、DisplayDate と SelectedDate を更新
                    int clampedValue = Math.Max(1, Math.Min(newValue, 31));
                    int year = calendar.DisplayDate.Year;
                    int month = calendar.DisplayDate.Month;

                    // 月の日数に基づいて一時的に調整
                    int daysInMonth = DateTime.DaysInMonth(year, month);
                    int validDate = Math.Min(clampedValue, daysInMonth);

                    // DisplayDate と SelectedDate を更新
                    calendar.DisplayDate = new DateTime(year, month, validDate);
                }
            }
        }
        private static object CorectReturnDate(DependencyObject d, object value)
        {
            if (value is int newValue && newValue > 0 && newValue <= 31)
            {
                return Math.Max(1, Math.Min(newValue, 31));
            }
            return 1;
        }

        private void CustomCalendar_DisplayModeChanged(object? sender, CalendarModeChangedEventArgs e)
        {
            if (_isHandlingEvent) return;
            _isHandlingEvent = true;
            if (DisplayMode == CalendarMode.Month)
            {
                DisplayMode = CalendarMode.Year;
                OnMonthSelected(sender, null);
            }
            _isHandlingEvent = false;
        }

        private void CustomCalendar_Loaded(object? sender, RoutedEventArgs e)
        {
            //DisplayMode = CalendarMode.Year;
        }

        private void CustomCalendar_DisplayDateChanged(object? sender, CalendarDateChangedEventArgs e)
        {
                    // DisplayMode を Year に戻す
                    DisplayMode = CalendarMode.Year;

            // MonthSelected イベントを発火
                    OnMonthSelected(sender, e);
        }

        protected virtual void OnMonthSelected(object? sender, CalendarDateChangedEventArgs? e)
        {
            MonthSelected?.Invoke(this, e);
        }
    }
}
