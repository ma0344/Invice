using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Invoice.Converters
{
    class TaxRateConverter : IValueConverter
    {
        public ObservableCollection<TaxTypeClass> TaxTypeClassList { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double taxRate && TaxTypeClassList != null)
            {
                    return $"{taxRate * 100}%";
            }
            return "";

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
