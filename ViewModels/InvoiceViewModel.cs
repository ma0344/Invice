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
using System.Diagnostics;

namespace Invoice.ViewModels
{
    public partial class InvoiceViewModel : INotifyPropertyChanged
    {
        event EventHandler UpdateTotalAmountEvent;
        public delegate void PropertyChangedHandler(object sender, PropertyChangedEventArgs e);
        public PropertyChangedHandler PropertyChangedEvent;
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

            var invoiceList = InvoiceClass.GetAllInvoice();
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
            CurrentInvoice.PropertyChanged += CurrentInvoice_PropertyChanged; ;
        }

        private void CurrentInvoice_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {

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

        public ObservableCollection<CustomerClass> CustomerCollectionViewSource
        {
            get
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                var dataContext = mainWindow.DataContext as MainWindowViewModel;
                var customerVM = dataContext.CustomerVM;
                var customerList = customerVM.CustomerClassList.ToList();
                ObservableCollection<CustomerClass> filterdCustomerList = [];
                foreach (CustomerClass customer in customerList)
                {
                    if (customer.CustomerVisible) filterdCustomerList.Add(customer);
                }
                
                return filterdCustomerList;   
            }
        }
        

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


        private ObservableCollection<BalanceClass> _BalanceClassList = [];
        public ObservableCollection<BalanceClass> BalanceClassList
        {
            get { return _BalanceClassList; }
            set
            {
                _BalanceClassList = value;
            }
        }


        private int _DepositAmount = 0;
        public int DepositAmount
        {
            get { return _DepositAmount; }
            set
            {
                _DepositAmount = value;
                AfterSettleUpAmount = ItemsTotalAmount - _DepositAmount > 0 ? ItemsTotalAmount - _DepositAmount : 0;
                AfterSettleUpDeposit = _DepositAmount - ItemsTotalAmount > 0 ? _DepositAmount - ItemsTotalAmount : 0;
                OnPropertyChanged(nameof(DepositAmount));
            }
        }


        private int _ItemsTotalAmount = 0;
        public int ItemsTotalAmount
        {
            get { return _ItemsTotalAmount; }
            set
            {
                _ItemsTotalAmount = value;
                AfterSettleUpAmount = ItemsTotalAmount - _DepositAmount > 0 ? ItemsTotalAmount - _DepositAmount : 0;
                AfterSettleUpDeposit = _DepositAmount - ItemsTotalAmount > 0 ? _DepositAmount - ItemsTotalAmount : 0;
                OnPropertyChanged(nameof(ItemsTotalAmount));
            }
        }

        private int _AfterSettleUpDeposit = 0;
        public int AfterSettleUpDeposit
        {
            get { return _AfterSettleUpDeposit; }
            set
            {
                _AfterSettleUpDeposit = value;
                OnPropertyChanged(nameof(AfterSettleUpDeposit));
            }
        }

        private int _AfterSettleUpAmount = 0;
        public int AfterSettleUpAmount
        {
            get { return _AfterSettleUpAmount; }
            set
            {
                _AfterSettleUpAmount = value;
                OnPropertyChanged(nameof(AfterSettleUpAmount));
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
                            item.PropertyChanged -= InvoiceItem_PropertyChanged;
                            item.PropertyChanged += InvoiceItem_PropertyChanged;
                        }
                    }
                    UpdateTotalAmount();
                }
                OnPropertyChanged(nameof(InvoiceItemClassList));
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

        private InvoiceStatusClass _SelectedStatus;
        public InvoiceStatusClass SelectedStatus
        {
            get => _SelectedStatus;
            set { _SelectedStatus = value; }
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
            var invoices = InvoiceClass.GetAllInvoice();
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

        public void ReloadBalanceList()
        {
            var balances = BalanceClass.GetAllBalances();
            BalanceClassList = new ObservableCollection<BalanceClass>(balances);
        }

        private void InvoiceItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (InvoiceItemClass item in e.NewItems)
                {
                    item.PropertyChanged -= InvoiceItem_PropertyChanged;
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
            if(e.NewItems != null || e.OldItems != null) 
                UpdateTotalAmount();
        }

        private void InvoiceItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InvoiceItemClass.ItemTotal))
            {
                ItemsTotalAmount = InvoiceItemClassList.Sum(item => item.ItemTotal);

                var item = sender as InvoiceItemClass;
                CurrentInvoice.ItemsTotal = ItemsTotalAmount;
                if(CurrentInvoice.TransactionTypeId == 2)
                {
                    CurrentInvoice.PaydByDeposit = DepositAmount - ItemsTotalAmount >= 0 ? ItemsTotalAmount : DepositAmount;
                    CurrentInvoice.InvoiceTotal = ItemsTotalAmount - CurrentInvoice.PaydByDeposit;
                    CurrentInvoice.ItemsTotal = ItemsTotalAmount;
                }
                else
                {
                    CurrentInvoice.PaydByDeposit = 0;
                    CurrentInvoice.InvoiceTotal = ItemsTotalAmount;
                    CurrentInvoice.ItemsTotal = ItemsTotalAmount;
                }
                    Debug.WriteLine($"InvoiceTotal:{CurrentInvoice.InvoiceTotal}, ItemsTotal:{CurrentInvoice.ItemsTotal},PaydByDeposit:{CurrentInvoice.PaydByDeposit}, DepositAmount{DepositAmount}, TransactionTypeId:{CurrentInvoice.TransactionTypeId}, {ItemsTotalAmount}");
                UpdateTotalAmount();
            }
            //UpdateTotalAmount();
        }

        private void UpdateTotalAmount()
        {
            ItemsTotalAmount = InvoiceItemClassList.Sum(item => item.ItemTotal);
            if (CurrentInvoice == null) return;
            Debug.WriteLine(ItemsTotalAmount);
            UpdateTotalAmountEvent?.Invoke(CurrentInvoice, EventArgs.Empty);

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
