using System;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Invoice.Converters
{
    class InvoiceIdToSlipNumberConverter : IValueConverter
    {
        public ObservableCollection<InvoiceClass> InvoiceClassList { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int invoiceId && InvoiceClassList != null)
            {
                var invoice = InvoiceClassList.FirstOrDefault(inv => inv.InvoiceId == invoiceId);
                if (invoice != null)
                {
                    return invoice.SlipNumber;
                }
            }
            return null;

        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
