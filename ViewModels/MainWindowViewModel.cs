using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice.ViewModels
{
    namespace Invoice.ViewModels
    {
        public class MainWindowViewModel : INotifyPropertyChanged
        {
            public SettingsViewModel SettingsVM { get; set; }
            public CustomerViewModel CustomerVM { get; set; }
            private Lazy<InvoiceViewModel> _invoiceVM;
            public InvoiceViewModel InvoiceVM => _invoiceVM.Value;

            private Lazy<PaymentViewModel> _paymentVM;
            public PaymentViewModel PaymentVM => _paymentVM.Value;

            public MainWindowViewModel()
            {
                CustomerVM = new CustomerViewModel();
                SettingsVM = new SettingsViewModel();
                _invoiceVM = new Lazy<InvoiceViewModel>(() => new InvoiceViewModel());
                _paymentVM = new Lazy<PaymentViewModel>(() => new PaymentViewModel());
            }
            // INotifyPropertyChangedの実装
            public event PropertyChangedEventHandler? PropertyChanged;

            protected void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
