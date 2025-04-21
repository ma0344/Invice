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

            private Lazy<CustomerViewModel> _CustomerVM;
            public CustomerViewModel CustomerVM => _CustomerVM.Value;
            private Lazy<InvoiceViewModel> _invoiceVM;
            public InvoiceViewModel InvoiceVM => _invoiceVM.Value;

            private Lazy<PaymentViewModel> _paymentVM;
            public PaymentViewModel PaymentVM => _paymentVM.Value;

            public MainWindowViewModel()
            {
                SettingsVM = new SettingsViewModel();
                _CustomerVM = new Lazy<CustomerViewModel>(() => new CustomerViewModel());
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
