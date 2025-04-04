using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Collections.Specialized;
using System.Windows;
using Invoice.ViewModels.Invoice.ViewModels;

namespace Invoice.ViewModels
{
    public partial class InvoiceViewModel : INotifyPropertyChanged
    {
        public InvoiceViewModel()
        {

            var mainWindow = Application.Current.MainWindow as MainWindow;
            var dataContext = mainWindow.DataContext as MainWindowViewModel;
            var customerVM = dataContext.CustomerVM;
            var settingsVM = dataContext.SettingsVM;
            ItemClassList = settingsVM.ItemClassList;
            TaxTypeClassList = settingsVM.TaxTypeClassList;
            InvoiceStatusClassList = settingsVM.InvoiceStatusClassList;
            TransactionTypeClassList = settingsVM.TransactionTypeClassList;
            InvoiceItemClassList = new ObservableCollection<InvoiceItemClass>();
            InvoiceItemClassList.CollectionChanged += InvoiceItems_CollectionChanged;
            var balances = BalanceClass.GetAllBalances();
            BalanceClassList = new ObservableCollection<BalanceClass>(balances);

            var invoiceList = InvoiceClass.GetInvoices();
            InvoiceClassList = new ObservableCollection<InvoiceClass>(invoiceList);
            DepositFromInvoicePage = false;

            var customers = customerVM.CustomerClassList;
            foreach (var invoice in InvoiceClassList)
            {
                invoice.PropertyChanged += Invoice_PropertyChanged;
                invoice.CustomerName = customers.FirstOrDefault(customer => customer.CustomerId == invoice.CustomerId).CustomerName;
                invoice.InvoiceStatus = InvoiceStatusClassList.FirstOrDefault(status => status.InvoiceStatusId == invoice.InvoiceStatusId).InvoiceStatus;
            }
            InvoiceClassList.CollectionChanged += InvoiceList_CollectionChanged;
            CurrentInvoice = new InvoiceClass();
        }
        private bool _DepositFromInvoicePage = false;
        public bool DepositFromInvoicePage
        {
            get { return _DepositFromInvoicePage; }
            set
            {
                if (_DepositFromInvoicePage != value) 
                {
                    _DepositFromInvoicePage = value;
                }
            }
        }

        private ObservableCollection<InvoiceClass> _InvoiceClassList = [];

        public ObservableCollection<InvoiceClass> InvoiceClassList
        {
            get { return _InvoiceClassList; }
            set
            {
                _InvoiceClassList = value;
                OnPropertyChanged(nameof(InvoiceClassList));
            }
        }

        
        public CollectionViewSource InvoiceCollectionViewSource { get; set; }

        
        private ObservableCollection<ItemClass> _ItemClassList = [];
        public ObservableCollection<ItemClass> ItemClassList
        {
            get 
            {
                return _ItemClassList; 
            }
            set
            {
                _ItemClassList = value;
                OnPropertyChanged(nameof(ItemClassList));
            }
        }
       
        
        private static ObservableCollection<TaxTypeClass> _TaxTypeClassList = [];
        public static ObservableCollection<TaxTypeClass> TaxTypeClassList
        {
            get { return _TaxTypeClassList; }
            set
            {
                _TaxTypeClassList = value;
            }
        }

        
        private ObservableCollection<TransactionTypeClass> _TransactionTypeClassList = [];
        public ObservableCollection<TransactionTypeClass> TransactionTypeClassList
        {
            get { return _TransactionTypeClassList; }
            set
            {
                _TransactionTypeClassList = value;
                OnPropertyChanged(nameof(TransactionTypeClassList));
            }
        }


        private static ObservableCollection<BalanceClass> _BalanceClassList = [];

        public static ObservableCollection<BalanceClass> BalanceClassList
        {
            get { return _BalanceClassList; }
            set
            {
                _BalanceClassList = value;
            }
        }

        private ObservableCollection<InvoiceStatusClass> _InvoiceStatusClassList = [];
        public ObservableCollection<InvoiceStatusClass> InvoiceStatusClassList
        {
            get { return _InvoiceStatusClassList; }
            set
            {
                _InvoiceStatusClassList = value;
                OnPropertyChanged(nameof(InvoiceStatusClassList));
            }
        }


        private ObservableCollection<InvoiceItemClass> _InvoiceItemClassList = [];

        public ObservableCollection<InvoiceItemClass> InvoiceItemClassList
        {
            get { return _InvoiceItemClassList; }
            set
            {
                if (_InvoiceItemClassList != value)
                {
                    if (_InvoiceItemClassList != null)
                    {
                        _InvoiceItemClassList.CollectionChanged -= InvoiceItems_CollectionChanged;
                        foreach (var item in _InvoiceItemClassList)
                        {
                            item.PropertyChanged -= InvoiceItem_PropertyChanged;
                        }
                    }
                    _InvoiceItemClassList = value;
                    OnPropertyChanged(nameof(InvoiceItemClassList));
                    if (_InvoiceItemClassList != null)
                    {
                        _InvoiceItemClassList.CollectionChanged += InvoiceItems_CollectionChanged;
                        foreach (var item in _InvoiceItemClassList)
                        {
                            item.PropertyChanged += InvoiceItem_PropertyChanged;
                        }
                    }
                    UpdateTotalAmount();
                }
            }
        }


        private InvoiceClass _CurrentInvoice = new();
        public InvoiceClass CurrentInvoice
        {
            get { return _CurrentInvoice; }
            set 
            {
                _CurrentInvoice = value;
                OnPropertyChanged(nameof(CurrentInvoice));
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

        private string _paneTitle = "新規請求書作成";
        public string PaneTitle
        {
            get { return _paneTitle; }
            set
            {
                _paneTitle = value;
                OnPropertyChanged(nameof(PaneTitle));
            }
        }

        private int _InvoiceTotalAmount = 0;
        public int InvoiceTotalAmount
        {
            get { return _InvoiceTotalAmount; }
            set
            {
                _InvoiceTotalAmount = value;
                OnPropertyChanged(nameof(InvoiceTotalAmount));
            }
        }

        public void InvoiceListReset(List<InvoiceClass> invoices)
        {
            InvoiceClassList.Clear();
            foreach(var invoice in invoices)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                var dataContext = mainWindow.DataContext as MainWindowViewModel;
                var customerVM = dataContext.CustomerVM;
                invoice.CustomerName = customerVM.CustomerClassList.FirstOrDefault(c => c.CustomerId == invoice.CustomerId).CustomerName;
                invoice.InvoiceStatus = InvoiceStatusClassList.FirstOrDefault(s => s.InvoiceStatusId == invoice.InvoiceStatusId).InvoiceStatus;
                InvoiceClassList.Add(invoice);
            }
        }

        public void ReloadInvoiceList()
        {
            InvoiceClassList.Clear();
            var invoices = InvoiceClass.GetInvoices();
            foreach (var invoice in invoices)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                var dataContext = mainWindow.DataContext as MainWindowViewModel;
                var customerVM = dataContext.CustomerVM;
                invoice.CustomerName = customerVM.CustomerClassList.FirstOrDefault(c => c.CustomerId == invoice.CustomerId).CustomerName;
                invoice.InvoiceStatus = InvoiceStatusClassList.FirstOrDefault(s => s.InvoiceStatusId == invoice.InvoiceStatusId).InvoiceStatus;
                InvoiceClassList.Add(invoice);
            }
        }

        private void InvoiceItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (InvoiceItemClass item in e.NewItems)
                {
                    item.PropertyChanged += InvoiceItem_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (InvoiceItemClass item in e.OldItems)
                {
                    item.PropertyChanged -= InvoiceItem_PropertyChanged;
                }
            }
            UpdateTotalAmount();
        }

        private void InvoiceItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InvoiceItemClass.ItemTotal))
            {
                UpdateTotalAmount();
            }
            UpdateTotalAmount();
        }

        private void UpdateTotalAmount()
        {
            InvoiceTotalAmount = InvoiceItemClassList.Sum(item => item.ItemTotal);
        }



        private void Invoice_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private void InvoiceList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
