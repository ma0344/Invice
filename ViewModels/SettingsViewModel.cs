using Invoice.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice.ViewModels
{
    public partial class SettingsViewModel : INotifyPropertyChanged
    {
        public SettingsViewModel()
        {
            slipNumbers = new();
            SlipNumberInfo = slipNumbers.GetSlipNumber();
            var taxList = TaxTypeClass.GetTaxes();
            TaxTypeClassList = new ObservableCollection<TaxTypeClass>(taxList);
            var transactionType = TransactionTypeClass.GetTransactionTypes();
            TransactionTypeClassList = new ObservableCollection<TransactionTypeClass>(transactionType);
            var items = ItemClass.GetItems();
            ItemClassList = new ObservableCollection<ItemClass>(items);
            var statusInfos = InvoiceStatusClass.GetInvoiceStatuses();
            foreach (var inf in statusInfos)
            {
                inf.StatusChanged += InvoiceStatus_Changed;
            }
            InvoiceStatusClassList = new ObservableCollection<InvoiceStatusClass>(statusInfos);
            SelectedItem = new ItemClass();
            ReloadItems();
            DefaultItemsList = new ObservableCollection<DefaultItemsClass>(DefaultItemsClass.GetDefaultItems());
            foreach (var item in DefaultItemsList)
            {
                item.ItemName = ItemClassList.Where(x => x.ItemId == item.ItemId).FirstOrDefault().ItemName;
                item.ItemCode = ItemClassList.Where(x => x.ItemId == item.ItemId).FirstOrDefault().ItemCode;
                item.TaxTypeName = TaxTypeClassList.Where(x => x.TaxTypeId == item.TaxTypeId).FirstOrDefault().TaxTypeName;
                item.SelectedTax = TaxTypeClassList.Where(x => x.TaxTypeId == item.TaxTypeId).FirstOrDefault();
            }

        }
        private void InvoiceStatus_Changed(object sender, PropertyChangedEventArgs e)
        {
        
        }

        private SlipNumberClass _slipNumberInfo;
        public SlipNumberClass SlipNumberInfo
        {
            get { return _slipNumberInfo; }
            set
            {
                _slipNumberInfo = value;
                OnPropertyChanged(nameof(SlipNumberInfo));
            }
        }

        private SlipNumbers _slipNumbers = new();
        public SlipNumbers slipNumbers
        {
            get { return _slipNumbers; }
            set
            {
                _slipNumbers = value;
                OnPropertyChanged(nameof(slipNumbers));
            }
        }

        public void SlipnumberInfoReload()
        {
            slipNumbers = new();
        }


        private ObservableCollection<TaxTypeClass> _TaxTypeClassList;
        public ObservableCollection<TaxTypeClass> TaxTypeClassList
        {
            get { return _TaxTypeClassList; }
            set
            {
                _TaxTypeClassList = value;
                OnPropertyChanged(nameof(TaxTypeClassList));
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


        private ObservableCollection<DefaultItemsClass> _DefaultItemsList = [];
        public ObservableCollection<DefaultItemsClass> DefaultItemsList
        {
            get => _DefaultItemsList;
            set
            {
                _DefaultItemsList = value;
                OnPropertyChanged(nameof(DefaultItemsList));
            }
        }

        private ItemClass _selectedItem = new();
        public ItemClass SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public void ReloadItems()
        {
            ItemClassList.Clear();
            var items = ItemClass.GetItems();
            foreach (var item in items)
            {
                item.TaxTypeName = TaxTypeClassList.Where(x => x.TaxTypeId == item.TaxTypeId).FirstOrDefault().TaxTypeName;
                ItemClassList.Add(item);
            }
        }

        public void ReloadDefaultItems()
        {
            DefaultItemsList.Clear();
            var items = DefaultItemsClass.GetDefaultItems();
            foreach (var item in items)
            {
                item.ItemName = ItemClassList.Where(x => x.ItemId == item.ItemId).FirstOrDefault().ItemName;
                item.ItemCode = ItemClassList.Where(x => x.ItemId == item.ItemId).FirstOrDefault().ItemCode;
                item.TaxTypeName = TaxTypeClassList.Where(x => x.TaxTypeId == item.TaxTypeId).FirstOrDefault().TaxTypeName;
                DefaultItemsList.Add(item);
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

        private string _paneTitle = "新規項目登録";
        public string PaneTitle
        {
            get { return _paneTitle; }
            set
            {
                _paneTitle = value;
                OnPropertyChanged(nameof(PaneTitle));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}