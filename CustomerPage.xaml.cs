using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Invoice.ViewModels;
using Invoice.ViewModels.Invoice.ViewModels;
using ModernWpf;
using MySqlConnector;


namespace Invoice
{
    /// <summary>
    /// Customer.xaml の相互作用ロジック
    /// </summary>
    public partial class CustomerPage : Page
    {
        CustomerViewModel vm;
        private bool isEditing = false;
        private bool isPageInitialized = false;
        private CustomerFilterParam filterParam = new();
        public CustomerPage(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            vm = mainWindowViewModel.CustomerVM;
            this.Loaded += PageLoaded;
            DataContext = mainWindowViewModel;
            vm.CustomerCollectionViewSource = new CollectionViewSource();
            vm.CustomerCollectionViewSource.Source = vm.CustomerClassList;
            vm.PropertyChanged += CustomerVM_PropertyChanged;


        }

        private void CustomerFilter()
        {
            var param = filterParam;
            var source = vm.CustomerCollectionViewSource;
            source.Filter += (sender,e) =>
            {
            if (e.Item is CustomerClass customer) 
                {
                    if ((customer.CustomerId != 0) && 
                        (param.CustomerId == null || customer.CustomerId == param.CustomerId) &&
                        (string.IsNullOrWhiteSpace(param.CustomerName) || customer.CustomerName == param.CustomerName) &&
                        (string.IsNullOrWhiteSpace(param.CustomerKana) || customer.CustomerKana == param.CustomerKana) &&
                        (param.CustomerBalance == null || customer.CustomerBalance == param.CustomerBalance) &&
                        (vm.ShowAllCustomer == true || customer.CustomerVisible == true))
                        e.Accepted = true;
                    else
                        e.Accepted = false;
                }
            };
        }


        private void CustomerVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
        private void ShowAllCustomer_Toggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = sender as ModernWpf.Controls.ToggleSwitch;
            if (toggleSwitch != null)
            {
                vm.ShowAllCustomer = toggleSwitch.IsOn;
                CustomerFilter();
            }
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.NavigateToPage(label.Name);
            }
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            var customers = CustomerClass.GetCustomers();
            vm.CustomerListReset(customers);
            CustomerFilter();
        }

        public void SelectAllChecked_Checked(object sender, RoutedEventArgs e)
        {
            var dataGrid = CustomerListDataGrid;
            foreach (var item in CustomerListDataGrid.Items)
            {
                var container = dataGrid.ItemContainerGenerator.ContainerFromItem(item);
                if (container is DataGridRow row)
                {
                    row.IsSelected = true;
                    VisualTreeHelperExtensions.FindVisualChildByName<CheckBox>(row, "CustomerCheckBox").IsChecked = true;
                }
            }
        }

        public void SelectAllChecked_Unchecked(object sender, RoutedEventArgs e)
        {
            var dataGrid = CustomerListDataGrid;
            foreach (var item in CustomerListDataGrid.Items)
            {
                var container = dataGrid.ItemContainerGenerator.ContainerFromItem(item);
                if (container is DataGridRow row)
                {
                    row.IsSelected = true;
                    VisualTreeHelperExtensions.FindVisualChildByName<CheckBox>(row, "CustomerCheckBox").IsChecked = false;
                }
            }
        }

        private void SelectCheckBox_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            isEditing = false;
            EnterDetailPane();
        }

        private void CustomerListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            isEditing = true;
            EnterDetailPane();
        }

        private void EditCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            isEditing = true;
            EnterDetailPane();
        }
        
        private void EnterDetailPane()
        {
            if (!isEditing) 
            {
                vm.SelectedCustomer = new CustomerClass();
                CustomerIdTextBlock.Text = "----";
                vm.SaveButtonText = "保存";
                vm.PaneTitle = "利用者情報編集";
            }
            else
            {
                if (CustomerListDataGrid.SelectedItems.Count == 0) MessageBox.Show("編集する利用者を選択してください");
                if (CustomerListDataGrid.SelectedItem is CustomerClass selectedCustomer)
                {
                    vm.SaveButtonText = "更新";
                    vm.PaneTitle = "利用者新規登録";
                    vm.SelectedCustomer = selectedCustomer;
                }
            }
            VisibleSwitchStackPanel.Visibility = isEditing == true ? Visibility.Visible : Visibility.Hidden;
            ShowDatailPane();
        }
        
        private void ShowDatailPane()
        {

            var pane = CustomerDetailPane;

            if (CustomerPageContentGrid.ActualHeight < pane.Height)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.Height += pane.Height - CustomerPageContentGrid.ActualHeight;
            }
            CustomerPageContentGrid.IsEnabled = false;
            var renderTransform = CustomerDetailPaneTransform;
            var slideUpAnimation = new DoubleAnimation
            {
                From = pane.Height,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            renderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideUpAnimation);
        }

        private void HideDetailPane()
        {
            var renderTransform = CustomerDetailPane.RenderTransform as System.Windows.Media.TranslateTransform;
            var slideDownAnimation = new DoubleAnimation
            {
                From = 0,
                To = 300,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            renderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideDownAnimation);
            CustomerPageContentGrid.IsEnabled = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!CanAddToServer())
            {
                MessageBox.Show("未入力の項目があります\n全ての項目を入力してください");
                return;
            }

            try
            {
                if (!isEditing)
                {
                    new CustomerClass()
                    {
                        CustomerId = 0,
                        CustomerName = CustomerNameTextBox.Text,
                        CustomerKana = CustomerKanaTextBox.Text,
                        CustomerBalance = int.Parse(CustomerBalanceTextBox.Text),
                        CustomerVisible = true
                    }.AddCustomerInDatabase();
                }
                else
                {
                    vm.SaveCustomerChanges();
                    isEditing = false;
                }
            }
            catch (Exception ex)
            {
                var exception = ex as MySqlException;
                switch (exception.ErrorCode)
                {
                    case MySqlErrorCode.DuplicateKeyEntry:
                        MessageBox.Show($"同じ氏名の利用者登録があるため登録できません");
                        break;
                    default:
                        MessageBox.Show($"データベースへの保存中にエラーが発生しました: {ex.Message}");
                        break;
                }
                return;
            }
            vm.ReloadCustomers(ShowAllCustomer.IsOn);
            HideDetailPane();
        }
        private bool CanAddToServer()
        {

            if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
                return false;
            if (string.IsNullOrWhiteSpace(CustomerKanaTextBox.Text))
                return false;
            if (string.IsNullOrWhiteSpace(CustomerBalanceTextBox.Text))
                return false;
            if (!int.TryParse(CustomerBalanceTextBox.Text, out _))
                return false;
            return true;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            HideDetailPane();
        }

        private void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            //if (CustomerListView.SelectedItem is CustomerClass selectedCustomer)
                if (CustomerListDataGrid.SelectedItem is CustomerClass selectedCustomer)
                {
                    var result = MessageBox.Show("選択された顧客を削除しますか？", "確認", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var selectedItems = CustomerListDataGrid.SelectedItems;
                    //var selectedItems = CustomerListView.SelectedItems;


                    List<int> selectedItemsID = new();
                    foreach (var item in selectedItems)
                    {
                        if (item is CustomerClass customer)
                        {
                            selectedItemsID.Add(customer.CustomerId);
                        }
                    }
                    foreach (var item in selectedItemsID)
                    {
                        foreach (var customer in vm.CustomerClassList)
                        {
                            if (customer.CustomerId == item)
                            {
                                vm.SelectedCustomer = customer;
                                vm.SelectedCustomer.CustomerVisible = false;
                                vm.SelectedCustomer.UpdateCustomerInDatabase();
                                continue;
                            }
                        }
                    }
                }
            }

        }

        private void CustomerCheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var InvoiceItem = checkBox.FindAscendant<DataGridRow>();
            if (InvoiceItem != null)
            {
                InvoiceItem.IsSelected = checkBox.IsChecked == false;
            }
            e.Handled = true;

        }

    }
}