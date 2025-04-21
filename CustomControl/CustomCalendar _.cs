using System;
using System.Collections.Generic;
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

namespace Invoice.CustomControl
{
    public class CustomCalendar_ : Calendar
    {
        public delegate void MonthSelectedEventHandler(object sender, CalendarDateChangedEventArgs e);
        public event  MonthSelectedEventHandler? MonthSelected;

        private int? _returnDate;
        public int? ReturnDate
        {
            get => _returnDate; set => _returnDate = value;
        }
        static CustomCalendar_()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomCalendar_), new FrameworkPropertyMetadata(typeof(CustomCalendar_)));
        }
        public CustomCalendar_()
        {
            this.DisplayDateChanged -= CustomCalendar_DisplayDateChanged;
            this.DisplayDateChanged += CustomCalendar_DisplayDateChanged;
            this.DisplayModeChanged -= CustomCalendar_DisplayModeChanged;
            this.DisplayModeChanged += CustomCalendar_DisplayModeChanged;
            Loaded += CustomCalendar_Loaded;
            DisplayMode = CalendarMode.Year;
        }

        private void CustomCalendar_DisplayModeChanged(object? sender, CalendarModeChangedEventArgs e)
        {
            if (DisplayMode == CalendarMode.Month)
            {
                DisplayMode = CalendarMode.Year;
                var displayYear = DisplayDate.Year;
                var displayMonth = DisplayDate.Month;

                // 月の日数に基づいて一時的に調整
                int daysInMonth = DateTime.DaysInMonth(displayYear, displayMonth);
                int validDate = Math.Min(daysInMonth, ReturnDate ?? 1);

                // DisplayDate と SelectedDate を更新
                //DisplayDate = new DateTime(displayYear, displayMonth, validDate);
                //SelectedDate = new DateTime(displayYear, displayMonth, validDate);
                OnMonthSelected(sender, null);
            }
        }

        private void CustomCalendar_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayMode = CalendarMode.Year;
        }

        private static readonly Func<DateTime> GetCurrentDate = () => DateTime.Now;

        private void CustomCalendar_DisplayDateChanged(object? sender, CalendarDateChangedEventArgs e)
        {
            if (e.AddedDate.HasValue && e.RemovedDate.HasValue)
            {
                var addedDate = e.AddedDate.Value;
                var removedDate = e.RemovedDate.Value;
                if (addedDate.Year != removedDate.Year || addedDate.Month != removedDate.Month)
                {
                    var displayYear = DisplayDate.Year;
                    var displayMonth = DisplayDate.Month;

                    // 月の日数に基づいて一時的に調整
                    int daysInMonth = DateTime.DaysInMonth(displayYear, displayMonth);
                    int validDate = Math.Min(daysInMonth, ReturnDate ?? 1);

                    // DisplayDate と SelectedDate を更新
                    DisplayDate = new DateTime(displayYear, displayMonth, validDate);
                    //SelectedDate = new DateTime(displayYear, displayMonth, validDate);

                    // DisplayMode を Year に戻す
                    DisplayMode = CalendarMode.Year;

                    // MonthSelected イベントを発火
                    OnMonthSelected(sender, e);
                }
            }
        }


        private static object CorectReturnDate(DependencyObject d, object value)
        {
            if (value is int newValue)
            {
                return Math.Max(1, Math.Min(newValue, 31));
            }
            return 1;
        }

        private static void OnReturnDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
                    calendar.SelectedDate = new DateTime(year, month, validDate);
                }
            }
        }


        protected virtual void OnMonthSelected(object sender, CalendarDateChangedEventArgs? e)
        {
            MonthSelected?.Invoke(this, e);

        }
    }
}
