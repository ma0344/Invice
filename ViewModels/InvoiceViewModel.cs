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
using Invoice.Classes;

namespace Invoice.ViewModels
{
    public partial class InvoiceViewModel : INotifyPropertyChanged
    {
        event EventHandler? UpdateTotalAmountEvent;
        public delegate void PropertyChangedHandler(object? sender, PropertyChangedEventArgs e);
        public PropertyChangedHandler? PropertyChangedEvent;
        private readonly CustomerViewModel customerVM;
        private readonly SettingsViewModel settingsVM;
        public InvoiceViewModel()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            var dataContext = mainWindow!.DataContext as MainWindowViewModel;
            customerVM = dataContext!.CustomerVM;
            settingsVM = dataContext.SettingsVM;
            ItemClassList = settingsVM.ItemClassList;
            TaxTypeClassList = settingsVM.TaxTypeClassList;
            InvoiceStatusClassList = settingsVM.InvoiceStatusClassList;
            TransactionTypeClassList = settingsVM.TransactionTypeClassList;
            var invoiceItems = InvoiceItemClass.GetInvoiceItems();
            InvoiceItemClassList = [..invoiceItems];
            //var invoiceList = InvoiceClass.GetAllInvoice();
            InvoiceClassList = [];
            DepositFromInvoicePage = false;

            var customers = customerVM.CustomerClassList;
            foreach (var invoice in InvoiceClassList)
            {
                invoice.PropertyChanged += Invoice_PropertyChanged;
                invoice.CustomerName = customers.FirstOrDefault(customer => customer.CustomerId == invoice.CustomerId)!.CustomerName;
                invoice.InvoiceStatus = InvoiceStatusClassList.FirstOrDefault(status => status.InvoiceStatusId == invoice.InvoiceStatusId)!.InvoiceStatus;
            }
            //InvoiceClassList.CollectionChanged += InvoiceList_CollectionChanged;
            CurrentInvoice = new InvoiceClass();
            CurrentInvoice.PropertyChanged += CurrentInvoice_PropertyChanged;
            CurrentInvoice.InvoiceItems.CollectionChanged += InvoiceItems_CollectionChanged;
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

        
        public CollectionViewSource? InvoiceCollectionViewSource { get; set; }

        public static ObservableCollection<CustomerClass> CustomerCollectionViewSource
        {
            get
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                var dataContext = mainWindow!.DataContext as MainWindowViewModel;
                var customerVM = dataContext!.CustomerVM;
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

        private int _DepositAmount = 0;
        public int DepositAmount
        {
            get
            {
                return _DepositAmount;
            }
            set
            {
                _DepositAmount = value;
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
                
                OnPropertyChanged(nameof(ItemsTotalAmount));
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
                    _InvoiceItemClassList = value;
                    OnPropertyChanged(nameof(InvoiceItemClassList));
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

        //private InvoiceStatusClass _SelectedStatus;
        //public InvoiceStatusClass SelectedStatus
        //{
        //    get => _SelectedStatus;
        //    set { _SelectedStatus = value; }
        //}
        public void InvoiceListReset(List<InvoiceClass> invoices)
        {
            InvoiceClassList.Clear();
            foreach(var invoice in invoices)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                var dataContext = mainWindow!.DataContext as MainWindowViewModel;
                var customerVM = dataContext!.CustomerVM;
                invoice.CustomerName = customerVM.CustomerClassList.FirstOrDefault(c => c.CustomerId == invoice.CustomerId)!.CustomerName;
                invoice.InvoiceStatus = InvoiceStatusClassList.FirstOrDefault(s => s.InvoiceStatusId == invoice.InvoiceStatusId)!.InvoiceStatus;
                InvoiceClassList.Add(invoice);
            }
        }

        public void ReloadInvoiceList()
        {
            // InvoiceItemClassList の更新
            var invoiceItems = InvoiceItemClass.GetInvoiceItems();
            InvoiceItemClassList.Clear();
            invoiceItems.ForEach(item => InvoiceItemClassList.Add(item));

            var invoices = InvoiceClass.GetAllInvoice();
            InvoiceClassList.Clear();
            foreach (var invoice in invoices)
            {
                var id = invoice.InvoiceId;
                if(id == 34)
                {

                }
                invoice.InvoiceItems.Clear();
                invoice.CustomerName = customerVM.CustomerClassList.FirstOrDefault(c => c.CustomerId == invoice.CustomerId)!.CustomerName;
                invoice.InvoiceStatus = InvoiceStatusClassList.FirstOrDefault(s => s.InvoiceStatusId == invoice.InvoiceStatusId)!.InvoiceStatus;
                var items = invoiceItems.Where(i => i.InvoiceId == invoice.InvoiceId).ToList();
                items.Sort((a, b) => a.ItemOrder - b.ItemOrder);
                items.ForEach(item => invoice.InvoiceItems.Add(item));
                invoice.PropertyChanged += Invoice_PropertyChanged;
                InvoiceClassList.Add(invoice);

            }

            // InvoiceCollectionViewSource の更新
            InvoiceCollectionViewSource!.Source = InvoiceClassList;


        }

        private void InvoiceItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
            if (e.NewItems != null || e.OldItems != null)
                UpdateTotalAmount();
        }

        private void InvoiceItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InvoiceItemClass.ItemTotal) && CurrentInvoice is InvoiceClass invoice)
            {
                ItemsTotalAmount = CurrentInvoice.InvoiceItems.Sum(item => item.ItemTotal);

                var item = sender as InvoiceItemClass;
                if (invoice.TransactionTypeId == 2)
                {
                    invoice.PaydByDeposit = invoice.DepositUntilIssueDate - invoice.ItemsTotal >= 0 ? invoice.ItemsTotal : invoice.DepositUntilIssueDate;
                    DepositAmount = invoice.DepositUntilIssueDate - invoice.PaydByDeposit;
                }
                else
                {
                    invoice.PaydByDeposit = 0;
                }

                UpdateTotalAmount();
            }
            //UpdateTotalAmount();
        }

        private void UpdateTotalAmount()
        {
            //ItemsTotalAmount = CurrentInvoice.InvoiceItems.Sum(item => item.ItemTotal);
            //if (CurrentInvoice == null) return;
            //CurrentInvoice.PaydByDeposit = CurrentInvoice.DepositUntilIssueDate - CurrentInvoice.ItemsTotal >= 0 ? CurrentInvoice.ItemsTotal : CurrentInvoice.DepositUntilIssueDate;
            UpdateTotalAmountEvent?.Invoke(CurrentInvoice, EventArgs.Empty);

        }

        private void Invoice_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InvoiceClass.InvoiceItems) && CurrentInvoice is InvoiceClass invoice)
            {
                ItemsTotalAmount = CurrentInvoice.InvoiceItems.Sum(item => item.ItemTotal);

                var item = sender as InvoiceItemClass;
                if (invoice.TransactionTypeId == 2)
                {
                    invoice.PaydByDeposit = invoice.DepositUntilIssueDate - invoice.ItemsTotal >= 0 ? invoice.ItemsTotal : invoice.DepositUntilIssueDate;
                    DepositAmount = invoice.DepositUntilIssueDate - invoice.PaydByDeposit;

                }
                else
                {
                    invoice.PaydByDeposit = 0;
                }

                UpdateTotalAmount();
            }

        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
