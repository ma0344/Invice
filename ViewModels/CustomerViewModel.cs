using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Collections.Specialized;
using Invoice.ViewModels.Invoice.ViewModels;

namespace Invoice.ViewModels
{
    public partial class CustomerViewModel : INotifyPropertyChanged
    {
        public CustomerViewModel()
        {
            var customers = CustomerClass.GetCustomers();
            CustomerClassList = new ObservableCollection<CustomerClass>(customers);
        }
        enum SaveMode
        {
            Add = 0,
            Edit = 1,
        }

        private CollectionViewSource _customerCollectionViewSource;
        public CollectionViewSource CustomerCollectionViewSource
        {
            get { return _customerCollectionViewSource; }
            set
            {
                _customerCollectionViewSource = value;
                OnPropertyChanged(nameof(CustomerCollectionViewSource));
            }
        }

        private bool _showAllCustomer;
        public bool ShowAllCustomer
        {
            get { return _showAllCustomer; }
            set
            {
                _showAllCustomer = value;
                OnPropertyChanged(nameof(ShowAllCustomer));
                //ReloadCustomers(_showAllCustomer);
                //RefreshCustomerList();
            }
        }

        private ObservableCollection<CustomerClass> _CustomerClassList = [];
        public ObservableCollection<CustomerClass> CustomerClassList
        {
            get { return _CustomerClassList; }
            set
            {
                _CustomerClassList = value;
                OnPropertyChanged(nameof(CustomerClassList));
            }
        }

        private ObservableCollection<BalanceClass> _BalanceList = [];
        public ObservableCollection<BalanceClass> BalanceList
        {
            get { return _BalanceList; }
            set
            {
                _BalanceList = value;
                OnPropertyChanged(nameof(BalanceList));
            }
        }

        private void CustomerFilter(object sender, FilterEventArgs e)
        {
            if (ShowAllCustomer)
            {
                e.Accepted = true;
            }
            else
            {
                if (e.Item is CustomerClass customer)
                {
                    e.Accepted = customer.CustomerVisible;
                }
                else
                {
                    e.Accepted = false;
                }
            }
        }

        private void CustomerList_CollectionChanged(object sender,NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach(CustomerClass customer in e.NewItems)
                {
                    customer.PropertyChanged += Customer_PropertyChanged;

                }
            }
            if (e.OldItems != null)
            {
                foreach(CustomerClass customer in e.OldItems)
                {
                    customer.PropertyChanged += Customer_PropertyChanged;
                }
            }
        }

        private void Customer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CustomerClass.CustomerVisible))
            {
                RefreshCustomerList();
            }
        }

        public void SaveCustomerChanges()
        {
            if (SelectedCustomer != null)
            {
                SelectedCustomer.UpdateCustomerInDatabase();
                OnPropertyChanged(nameof(CustomerClassList));
            }
        }
        private bool CanSaveCustomer(CustomerClass info, SaveMode mode)
        {

            if (info == null)
                return false;

            // 必須項目のチェック
            if (string.IsNullOrWhiteSpace(info.CustomerName))
                return false;

            if (string.IsNullOrWhiteSpace(info.CustomerKana))
                return false;

            // その他の検証ルールを追加可能

            return true;
        }
        public void ReloadCustomers(bool selectAll)
        {
            var customers = CustomerClass.GetCustomers();
            CustomerClassList.Clear();
            foreach (var customer in customers)
            {
                CustomerClassList.Add(customer);
                customer.PropertyChanged += Customer_PropertyChanged;
            }
            OnPropertyChanged(nameof(CustomerClassList));
            //RefreshCustomerList();
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

        private string _paneTitle = "利用者新規登録";
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
            if (propertyName == nameof(CustomerClassList))
            {
                
                var view = CollectionViewSource.GetDefaultView(CustomerClassList);
                view.Refresh();
            }
            if (propertyName == nameof(ShowAllCustomer))
            {
                
            }
        }

        private CustomerClass _selectedCustomer;
        public CustomerClass SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
            }
        }

        public void CustomerListReset(List<CustomerClass> customers)
        {
            CustomerClassList.Clear();
            foreach (var customer in customers)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                var dataContext = mainWindow.DataContext as MainWindowViewModel;
                var customerVM = dataContext.CustomerVM;
                CustomerClassList.Add(customer);
            }
        }
        public void ReloadCustomerList()
        {
            var customers = CustomerClass.GetCustomers();
            CustomerClassList.Clear();
            foreach (var customer in customers)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                var dataContext = mainWindow.DataContext as MainWindowViewModel;
                var customerVM = dataContext.CustomerVM;
                CustomerClassList.Add(customer);
            }
        }
        public void RefreshCustomerList()
        {
            CustomerCollectionViewSource.View.Refresh();
            //ColumnsWidth.UpdateColumnsWidth();
        }

    }

}
