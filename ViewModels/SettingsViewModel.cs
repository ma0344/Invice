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
            SlipNumberInfo = SlipNumberClass.GetSlipNumberInfo();
            var taxList = TaxTypeClass.GetTaxes();
            TaxTypeClassList = new ObservableCollection<TaxTypeClass>(taxList);
            var transactionType = TransactionTypeClass.GetTransactionTypes();
            TransactionTypeClassList = new ObservableCollection<TransactionTypeClass>(transactionType);
            var paymentMethods = PaymentMethodClass.GetPaymentMethods();
            PaymentMethodClassList = new ObservableCollection<PaymentMethodClass>(paymentMethods);
            var statusInfos = InvoiceStatusClass.GetInvoiceStatuses();
            foreach (var inf in statusInfos)
            {
                inf.StatusChanged += InvoiceStatus_Changed;
            }
            InvoiceStatusClassList = new ObservableCollection<InvoiceStatusClass>(statusInfos);
            SelectedItem = new ItemClass();
            ReloadItems();

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

        private ObservableCollection<PaymentMethodClass> _PaymentMethodClassList = [];
        public ObservableCollection<PaymentMethodClass> PaymentMethodClassList
        {
            get { return _PaymentMethodClassList; }
            set
            {
                _PaymentMethodClassList = value;
                OnPropertyChanged(nameof(PaymentMethodClassList));
            }
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