using Invoice.ViewModels.Invoice.ViewModels;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Invoice.ViewModels
{
    public partial class PaymentViewModel : INotifyPropertyChanged
    {
        public PaymentViewModel()
        {
            var payments = PaymentClass.GetAllPayments();
            PaymentClassList = new ObservableCollection<PaymentClass>(payments);

        }

        private CollectionViewSource _paymentListViewSource;
        public CollectionViewSource PaymentListViewSource
        {
            get { return _paymentListViewSource; }
            set
            {
                _paymentListViewSource = value;
                OnPropertyChanged(nameof(PaymentListViewSource));
            }
        }

        private CollectionViewSource _invoiceListForPayment;
        public CollectionViewSource InvoiceListForPayment
        {
            get { return _invoiceListForPayment; }
            set
            {
                _invoiceListForPayment = value;
                OnPropertyChanged(nameof(InvoiceListForPayment));
            }
        }

        private ObservableCollection<PaymentClass> _paymentClassList;
        public ObservableCollection<PaymentClass> PaymentClassList
        {
            get { return _paymentClassList; }
            set
            {
                _paymentClassList = value;
            }
        }

        private PaymentClass _paymentDetailData;
        public PaymentClass PaymentDetailData
        {
            get { return _paymentDetailData; }
            set
            {
                _paymentDetailData = value;
                OnPropertyChanged(nameof(PaymentDetailData));
            }
        }

        private string _saveButtonText = "保存";
        public string SaveButtonText
        {
            get { return _saveButtonText; }
            set
            {
                _saveButtonText = value;
                OnPropertyChanged(nameof(SaveButtonText));
            }
        }

        private string _paneTitle = "入金記録作成";
        public string PaneTitle
        {
            get { return _paneTitle; }
            set
            {
                _paneTitle = value;
                OnPropertyChanged(nameof(PaneTitle));
            }
        }

        private string _ViewDate = "";
        public string ViewDate
        {
            get { return _ViewDate; }
            set
            {
                _ViewDate = value;
                OnPropertyChanged(nameof(ViewDate));
            }
        }
        private string _InvoiceViewDate = "";
        public string InvoiceViewDate
        {
            get { return _InvoiceViewDate; }
            set
            {
                _InvoiceViewDate = value;
                OnPropertyChanged(nameof(InvoiceViewDate));
            }
        }

        public void PaymentListReset(List<PaymentClass> payments)
        {
            PaymentClassList.Clear();
            foreach (var payment in payments )
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                var dataContext = mainWindow.DataContext as MainWindowViewModel;
                var customerVM = dataContext.CustomerVM;
                payment.CustomerName = customerVM.CustomerClassList.FirstOrDefault(c => c.CustomerId == payment.CustomerId).CustomerName;
                PaymentClassList.Add(payment);
            }
        }

        public void ReloadPaymentList()
        {
            var payments = PaymentClass.GetAllPayments();
            PaymentClassList.Clear();
            foreach (var payment in payments)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                var dataContext = mainWindow.DataContext as MainWindowViewModel;
                var customerVM = dataContext.CustomerVM;
                payment.CustomerName = customerVM.CustomerClassList.FirstOrDefault(c => c.CustomerId == payment.CustomerId).CustomerName;
                PaymentClassList.Add(payment);
            }

        }

        private InvoiceStatusClass _InvoiceStatusFilterValue;
        public InvoiceStatusClass InvoiceStatusFilterValue
        {
            get { return _InvoiceStatusFilterValue; }
            set
            {
                _InvoiceStatusFilterValue = value;
                OnPropertyChanged(nameof(InvoiceStatusFilterValue));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
