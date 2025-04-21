using Invoice.ViewModels;
using Invoice.ViewModels.Invoice.ViewModels;
using ModernWpf;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Controls = System.Windows.Controls;
using MS.Internal;
using System.Reflection;

namespace Invoice
{
    /// <summary>
    /// Invoice.xaml の相互作用ロジック
    /// </summary>
    public partial class InvoicePage : Controls.Page
    {
        private CustomerViewModel _customerVM;
        private PaymentViewModel _pVM;
        private SettingsViewModel _sVM;
        private InvoiceViewModel vm;
        private bool isEditing = false;
        private bool isInitializing = true;
        public CultureInfo cultureInfo = new("ja-JP");
        InvoiceClass CurrentInvoice;
        private InvoiceFiterParam filterParam = new();
        private MainWindow mainWindow;
        private int prevTransactionTypeId = 0;


        SlipNumberClass SlipNumberInfo = new();
        public InvoicePage(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            this.Loaded += InvoicePage_Loaded;
            vm = mainWindowViewModel.InvoiceVM;
            _customerVM = mainWindowViewModel.CustomerVM;
            _pVM = mainWindowViewModel.PaymentVM;
            _sVM = mainWindowViewModel.SettingsVM;
            this.DataContext = mainWindowViewModel;
            SlipNumberInfo = SlipNumberClass.GetSlipNumberInfo();
            cultureInfo.DateTimeFormat.Calendar = new JapaneseCalendar();
            cultureInfo.DateTimeFormat.ShortDatePattern = "ggy年M月d日";
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            vm.InvoiceCollectionViewSource = new CollectionViewSource();
            vm.InvoiceCollectionViewSource.Source = vm.InvoiceClassList;

            mainWindow = (MainWindow)Application.Current.MainWindow;
            CurrentInvoice = vm.CurrentInvoice;
            CurrentInvoice.PropertyChanged += CurrentInvoice_PropertyChanged;

            EventManager.RegisterClassHandler(typeof(MainWindow), PreviewMouseDownEvent, new MouseButtonEventHandler(MainWindow_MouseDown));

            InvoiceFilter(new InvoiceFiterParam());
            InvoiceSubject.GotKeyboardFocus += TextBox_GotKeyboardFocus;
            InvoiceSubject.PreviewLostKeyboardFocus += TextBox_PreviewLostKeyboardFocus;
            MessageTextBox.GotKeyboardFocus += TextBox_GotKeyboardFocus;
            MessageTextBox.PreviewLostKeyboardFocus += TextBox_PreviewLostKeyboardFocus;

        }
        private void InvoiceFilter(InvoiceFiterParam param)
        {
            var source = vm.InvoiceCollectionViewSource;
            source.Filter += (sender, e) =>
            {
                if (e.Item is InvoiceClass invoice)
                {
                    if ((param.CustomerId == 0 || invoice.CustomerId == param.CustomerId) &&
                        (param.InvoiceStatusId == 0 || invoice.InvoiceStatusId == param.InvoiceStatusId) &&
                        (param.TransactionTypeId == 0 || invoice.TransactionTypeId == param.TransactionTypeId) &&
                        (param.IssueDate == null || (StartOfMonth(param.IssueDate) <= invoice.IssueDate && invoice.IssueDate <= EndOfMonth(param.IssueDate))) &&
                        (param.DueDate == null || invoice.DueDate == param.DueDate) &&
                        (param.PaymentDate == null || invoice.PaymentDate == param.PaymentDate) &&
                        (string.IsNullOrWhiteSpace(param.Subject) || invoice.Subject == param.Subject) &&
                        (param.InvoiceId == 0 || invoice.InvoiceId == param.InvoiceId))
                        e.Accepted = true;
                    else
                        e.Accepted = false;
                }
            };
        }
        
        private DateTime StartOfMonth(DateTime? date)
        {
            if (date.HasValue) return new DateTime(date.Value.Year, date.Value.Month, 1);
            else
                return DateTime.MinValue;
            
        }

        private DateTime EndOfMonth(DateTime? date)
        {
            if (date.HasValue) return new DateTime(date.Value.Year, date.Value.Month, DateTime.DaysInMonth(date.Value.Year, date.Value.Month));
            else
                return DateTime.MinValue;
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.NavigateToPage(label.Name);
            }
        }

        private void InvoicePage_Loaded(object sender, RoutedEventArgs e)
        {
            vm.ReloadInvoiceList();
            InvoiceFilter(filterParam);
            if (isInitializing)
            {
                var arrow = FilterExpander.FindDescendantByName("arrow");
                var grid = arrow.Parent as Grid;
                grid.HorizontalAlignment = HorizontalAlignment.Right;
            }
        }

        private void DateBox_DateSelected(object sender, CalendarDateChangedEventArgs e)
        {
            filterParam.IssueDate = DateBox.SelectedDate;
            InvoiceIssueDate.DisplayDate = DateBox.SelectedDate;
            InvoiceFilter(filterParam);
        }
        
        private void FilterValue_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if(comboBox.SelectedItem is CustomerClass customer && customer.CustomerId != 0)
                {
                    filterParam.CustomerId = customer.CustomerId; 
                }
                else if(comboBox.SelectedItem is InvoiceStatusClass status && status.InvoiceStatusId != 0)
                {
                    filterParam.InvoiceStatusId = status.InvoiceStatusId;
                }
                InvoiceFilter(filterParam);
            }
        }
        private void ShowDatailPane()
        {
            Border pane = InvoiceDetailPane;
            if (InvoicePageContentsGrid.ActualHeight < pane.Height)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.Height += pane.Height - InvoicePageContentsGrid.ActualHeight;
            }
            InvoicePageContentsGrid.IsEnabled = false;
            var renderTransform = PaneTransform;
            var slideUpAnimation = new DoubleAnimation
            {
                From = InvoiceDetailPane.Height,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            slideUpAnimation.Completed += SlideUpAnimation_Completed;
            renderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideUpAnimation);



        }

        private void SlideUpAnimation_Completed(object? sender, EventArgs e)
        {
            isInitializing = false;
        }

        private void HideDetailPane()
        {
            var renderTransform = InvoiceDetailPane.RenderTransform as System.Windows.Media.TranslateTransform;
            var slideDownAnimation = new DoubleAnimation
            {
                From = 0,
                To = InvoiceDetailPane.Height,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            renderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideDownAnimation);
            //pane.Visibility = Visibility.Collapsed;
            InvoicePageContentsGrid.IsEnabled = true;
        }

        private void AddInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            isInitializing = true;
            isEditing = false;
            vm.CurrentInvoice = new InvoiceClass();
            vm.InvoiceItemClassList.Clear();
            var defaultItems = _sVM.DefaultItemsList.ToList();
            defaultItems.ForEach(item => vm.InvoiceItemClassList.Add(item.ToInvoiceItem()));
            DateTime issueDate;
            if (ShowAllInvoice.IsOn)
            {
                var y = DateBox.SelectedDate.Year;
                var m = DateBox.SelectedDate.Month;
                issueDate = new DateTime(y, m, DateTime.DaysInMonth(y, m));
            }
            else
            {
                issueDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
            }
            vm.CurrentInvoice.IssueDate = issueDate;
            vm.CurrentInvoice.Subject = $"利用料 {issueDate.ToString("ggy年M月分")}";

            vm.SaveButtonText = "保存";
            vm.PaneTitle = "新規請求書作成";
            ShowDatailPane();

        }

        private void EditInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            isInitializing = true;
            if (InvoiceListDataGrid.SelectedItem is InvoiceClass selectedInvoice)
            {
                isEditing = true;
                vm.CurrentInvoice = selectedInvoice.DeepClone();
                prevTransactionTypeId = vm.CurrentInvoice.TransactionTypeId ?? 1;
                vm.SaveButtonText = "更新";
                vm.PaneTitle = "請求書編集";
                vm.InvoiceItemClassList.Clear(); 
                var items = InvoiceItemClass.GetInvoiceItemsByInvoiceId(selectedInvoice.InvoiceId);
                items.ForEach(item => vm.InvoiceItemClassList.Add(item));
                if (vm.CurrentInvoice.TransactionTypeId == 2)
                {
                    TransactionTypeComboBox.SelectedIndex = 1;
   
                }

                               
                ShowDatailPane();

            }

        }

        private void InvoiceListDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditInvoiceButton_Click(EditInvoiceButton, new RoutedEventArgs());
        }

        private void DeleteInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (InvoiceListDataGrid.SelectedItems.Count == 0) return;
            var selectedItemsList = InvoiceListDataGrid.SelectedItems;
            List<int> idList = new();
            foreach (var item in InvoiceListDataGrid.SelectedItems)
            {
                if (item is InvoiceClass invoice) idList.Add(invoice.InvoiceId);
            }
            foreach (int invoiceId in idList)
            {
                var payment = _pVM.PaymentClassList.FirstOrDefault(payment => payment.InvoiceId == invoiceId);
                if (payment != null)
                {
                    var a = MessageBox.Show(
                                            owner: mainWindow,
                                            messageBoxText:
                                            $"この請求に対する入金記録があります\n" +
                                            $"入金記録も削除しますか？\n" +
                                            $"Yes...入金を削除\n" +
                                            $"No...入金の請求書情報が削除される\n",
                                            caption: "記録未選択",
                                            button: MessageBoxButton.YesNoCancel,
                                            defaultResult: MessageBoxResult.No,
                                            icon: MessageBoxImage.Hand
                                            
                                            );
                    switch (a)
                    {
                        case MessageBoxResult.Yes:
                            PaymentClass.DeletePaymentById(TypeOfID.Payment ,invoiceId);
                            break;

                        case MessageBoxResult.No:
                            PaymentClass.ClearInvoiceIdFromPayment(invoiceId);
                            break;
                        case MessageBoxResult.Cancel:
                            return;
                    }
                }
                InvoiceClass.DeleteInvoiceByInvoiceId(invoiceId);
            }
            _pVM.ReloadPaymentList();
            vm.ReloadInvoiceList();
            vm.ReloadBalanceList();

        }

        private void InvoiceCancelButton_Click(object sender, RoutedEventArgs e)
        {
            var buttonName = ((Button)sender).Name;
            var pane = InvoiceDetailPane;
            
            HideDetailPane();
            vm.CurrentInvoice = null;
            vm.InvoiceItemClassList.Clear();
        }

        private void AddInvoiceItemButton_Click(object sender, RoutedEventArgs e)
        {
            // ViewModelへの参照を取得（DataContextをInvoiceViewModelに設定している場合）
            var viewModel = vm as InvoiceViewModel;
            if (viewModel != null)
            {
                var newItem = new InvoiceItemClass();
                newItem.ItemOrder = vm.InvoiceItemClassList.Count + 1;
                viewModel.InvoiceItemClassList.Add(newItem);
            }
        }

        private void SaveInvoiceButton_Click(object sendwe, RoutedEventArgs e)
        {
            var invoice = vm.CurrentInvoice.DeepClone();
            invoice.SubTotal = vm.InvoiceItemClassList.Sum(x => x.ItemSubTotal);
            invoice.Tax = vm.InvoiceItemClassList.Sum(x => x.Tax);
            //invoice.InvoiceTotal = vm.InvoiceItemClassList.Sum(x => x.ItemTotal);
            invoice.InvoiceItems.AddRange(vm.InvoiceItemClassList.ToList<InvoiceItemClass>());
            var statusInfo = StatusComboBox.SelectedItem as InvoiceStatusClass;
            invoice.InvoiceStatusId = statusInfo?.InvoiceStatusId ?? 1;
            invoice.InvoiceStatus = statusInfo?.InvoiceStatus ?? "作成中";

            invoice.IssueDateString = vm.CurrentInvoice.IssueDateString;

            try
            {
                invoice.Subject = InvoiceSubject.Text;
                if (!isEditing)
                {
                    AddNewInvoice(invoice);
                }
                else
                {
                    UpdateInvoice(invoice);
                }
                _pVM.ReloadPaymentList();
                vm.ReloadInvoiceList();
                vm.ReloadBalanceList();
                HideDetailPane();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        public void AddNewInvoice(InvoiceClass invoice)
        { //新規登録
            var prefix = string.IsNullOrWhiteSpace(SlipNumberInfo.InvoicePrefix) ? "" : SlipNumberInfo.InvoicePrefix;
            var suffix = string.IsNullOrWhiteSpace(SlipNumberInfo.InvoiceSuffix) ? "" : SlipNumberInfo.InvoiceSuffix;
            prefix += invoice.IssueDate?.ToString("yyMM_");
            var numberString = (SlipNumberInfo.InvoiceLatest + 1).ToString("0000");
            var slipNumber = $"{prefix}{numberString}{suffix}";
            invoice.InvoiceItems.Clear();
            invoice.InvoiceItems.AddRange(vm.InvoiceItemClassList.ToList<InvoiceItemClass>());
            invoice.SlipNumber = slipNumber;
            invoice.ItemsTotal = vm.ItemsTotalAmount;
            invoice.PaydByDeposit = vm.ItemsTotalAmount - vm.AfterSettleUpAmount;
            invoice.InvoiceTotal = vm.AfterSettleUpAmount;
            if(invoice.TransactionTypeId == 2 && invoice.InvoiceTotal == 0)
            {
                invoice.InvoiceStatus = "入金済";
                invoice.InvoiceStatusId = 3;
                invoice.PaymentDate = invoice.IssueDate;
                invoice.PaydByDeposit = vm.ItemsTotalAmount - vm.AfterSettleUpAmount;
            }
            else
            {
                invoice.InvoiceStatus = "請求済";
                invoice.InvoiceStatusId = 2;
                invoice.PaymentDate = null;
                invoice.PaydByDeposit = 0;
            }
            invoice.TryAddInvoice();
            SlipNumberInfo.InclimentInvoiceLatest();

        }

        public void UpdateInvoice(InvoiceClass invoice)
        { //更新

            if(invoice.TransactionTypeId == 2)
            {
                invoice.InvoiceStatus = "入金済";
                invoice.InvoiceStatusId = 3;
                invoice.PaymentDate = invoice.IssueDate;
                invoice.PaydByDeposit = vm.ItemsTotalAmount - vm.AfterSettleUpAmount;
            }
            else
            {
                invoice.InvoiceStatus = "請求済";
                invoice.InvoiceStatusId = 2;
                invoice.PaymentDate = null;
                invoice.PaydByDeposit = 0;
            }

            if (prevTransactionTypeId == 2)
            {
                //更新前が前受清算の請求情報
                if (invoice.TransactionTypeId == 2)
                    //前受金→前受金
                    DepositClass.TryUpdateDeposit(invoice);
                else
                    //前受金→売掛金
                    DepositClass.DeleteDepositById(TypeOfID.Invoice,invoice.InvoiceId);
            }
            else
            {
                //更新前が売掛金の請求情報
                if (invoice.TransactionTypeId == 2)
                    //売掛金→前受金
                    DepositClass.TryAddDeposit(invoice);
                else { }//売掛金→売掛金
            }

            // 請求書の更新実行
            if (!invoice.TryUpdateInvoice()) throw new Exception("請求書の更新に失敗しました。");
        }

        private void InvoiceItemName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            //if (isInitializing) return;

            if (sender is ComboBox comboBox)
            {
                var selectedItem =comboBox.SelectedItem;
                var invoiceItem = comboBox.DataContext as InvoiceItemClass;
                if (isInitializing) return;
                if (invoiceItem != null && selectedItem != null)
                {
                    //if (invoiceItem.ItemId == selectedItem.ItemId && invoiceItem.ItemTotal != 0) return;
                    ItemClass comboBoxSelectedItem = new();
                    if (selectedItem is InvoiceItemClass item)
                        comboBoxSelectedItem = vm.ItemClassList.FirstOrDefault(i => i.ItemId == item.ItemId);
                    else
                        comboBoxSelectedItem = selectedItem as ItemClass;

                    invoiceItem.SetItem(comboBoxSelectedItem);
                    invoiceItem.ReTotal(invoiceItem);
                    var contentPresenter = comboBox.TemplatedParent as ContentPresenter;
                    var dataGridCell = contentPresenter.Parent as DataGridCell;
                    var dataGridCellsPanel = VisualTreeHelper.GetParent(dataGridCell) as DataGridCellsPanel;
                    var taxTypeNameComboBox = TaxTypeNameComboBox as DataGridComboBoxColumn;
                    var itemSource = taxTypeNameComboBox.ItemsSource as ListCollectionView;
                    itemSource.MoveCurrentToFirst();
                    //var taxTypeNameComboBox = VisualTreeHelperExtensions.FindVisualChildByName<ComboBox>(dataGridCellsPanel, "TaxTypeNameComboBox") as ComboBox;
                    //taxTypeNameComboBox.Text = invoiceItem.TaxTypeName;
                }
                else
                {
                    if (comboBox.DataContext == null) return;
                    if (comboBox.DataContext.ToString() == "{DataGrid.NewItemPlaceholder}")
                    {
                        //invoiceItem = new InvoiceItemClass();
                        //invoiceItem.SetItem(comboBox.SelectedItem as ItemClass);
                    }
                }
            }

        }

        private void InvoiceIssueDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var datePicker = sender as DatePicker;
            if (datePicker.SelectedDate != null && vm != null)
            {
                var issueDate = (DateTime)datePicker.SelectedDate;
                vm.CurrentInvoice.IssueDateString = issueDate.ToShortDateString();
                var tempDate = issueDate.AddMonths(1);
                vm.CurrentInvoice.DueDate = new DateTime(year: tempDate.Year, month: tempDate.Month, 15);
                InvoiceDueDate.SelectedDate = vm.CurrentInvoice.DueDate;
            }

        }

        private void InvoiceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            var item = e.AddedItems[0] as CustomerClass;
            if (item.CustomerId == 0) return;

            var comboBox = sender as ComboBox;
            var customerItem = comboBox.SelectedItem as CustomerClass;
            if (customerItem.CustomerId > 0)
            {
                vm.CurrentInvoice.CustomerName = _customerVM.CustomerClassList.FirstOrDefault(c => c.CustomerId == customerItem.CustomerId).CustomerName;
            }
        }

        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var dataGrid = InvoiceListDataGrid.IsEnabled ? InvoiceListDataGrid: InvoiceItemsDataGrid;
            foreach(var item in dataGrid.Items)
            {
                var container = dataGrid.ItemContainerGenerator.ContainerFromItem(item);
                if (container is DataGridRow row)
                {
                    
                    row.IsSelected = true;
                    VisualTreeHelperExtensions.FindVisualChildByName<CheckBox>(row, "GridRowCheckBox").IsChecked = true;
                }
            }
        }

        private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var dataGrid = InvoiceListDataGrid.IsEnabled ? InvoiceListDataGrid : InvoiceItemsDataGrid;
            foreach (var item in dataGrid.Items)
            {
                var container = dataGrid.ItemContainerGenerator.ContainerFromItem(item);
                if (container is DataGridRow row)
                {
                    row.IsSelected = false;
                    VisualTreeHelperExtensions.FindVisualChildByName<CheckBox>(row, "GridRowCheckBox").IsChecked = false;

                }
            }

        }

        private void InvoiceCheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var InvoiceItem = checkBox.FindAscendant<DataGridRow>();
            if (InvoiceItem != null)
            {
                InvoiceItem.IsSelected = checkBox.IsChecked == false;
            }
            e.Handled = true;
        }

        private void DeleteInvoiceItemButton_Click(object sender, RoutedEventArgs e)
        {
            var dataGrid = InvoiceItemsDataGrid;
            var selectedItems = dataGrid.SelectedItems;
            List<int> orderList = new ();
            foreach (var item in selectedItems)
            {
                if (item is InvoiceItemClass invoiceItem) orderList.Add(invoiceItem.ItemOrder);
            }
            orderList.Sort((a, b) => b - a);
            foreach (var itemOrder in orderList)
            {
                var item = vm.InvoiceItemClassList[itemOrder - 1];
                var id = item.InvoiceItemId;
                vm.InvoiceItemClassList.Remove(item);
            }
            int order = 1;
            foreach (var item in dataGrid.Items)
            {
                if (item is InvoiceItemClass invoiceItem) invoiceItem.ItemOrder = order++;
            }

        }

        private void ShowAllInvoice_Toggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            if (!toggleSwitch!.IsOn)
                filterParam.IssueDate = null;
            else
                filterParam.IssueDate = DateBox.SelectedDate;

            InvoiceFilter(filterParam);


        }

        private void CopyInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            var dataGrid = InvoiceListDataGrid;
            //var selectedItems = dataGrid.SelectedItems.Cast<InvoiceClass>().Reverse() ;
            var selectedItems = GetSelectedItemsInDisplayOrder(dataGrid);
            foreach (var item in selectedItems)
            {
                if (item is InvoiceClass invoice)
                {
                    var newInvoice = invoice.DeepClone();
                    var tempNewIssueDate = invoice.IssueDate?.AddMonths(2) ?? DateTime.Now;
                    newInvoice.IssueDate = new DateTime(tempNewIssueDate.Year, tempNewIssueDate.Month, 1).AddDays(-1);
                    newInvoice.SlipNumber ="";
                    newInvoice.InvoiceStatusId = 1;
                    newInvoice.InvoiceStatus = "作成中";
                    newInvoice.IssueDateString = newInvoice.IssueDate?.ToShortDateString();
                    newInvoice.DueDate = newInvoice.IssueDate?.AddDays(16);
                    newInvoice.Subject = newInvoice.IssueDate?.ToString("利用料 ggy年M月分");
                    var items = InvoiceItemClass.GetInvoiceItemsByInvoiceId(invoice.InvoiceId);
                    newInvoice.InvoiceItems.AddRange(items);
                    AddNewInvoice(newInvoice);
                    //CopyInvoiceAdd(newInvoice);
                    //SlipNumberInfo.InclimentInvoiceLatest();
                }
            }
            _pVM.ReloadPaymentList();
            vm.ReloadInvoiceList();
            vm.ReloadBalanceList();

        }

        private void CopyInvoiceAdd(InvoiceClass invoice)
        {
            var prefix = string.IsNullOrWhiteSpace(SlipNumberInfo.InvoicePrefix) ? "" : SlipNumberInfo.InvoicePrefix;
            var suffix = string.IsNullOrWhiteSpace(SlipNumberInfo.InvoiceSuffix) ? "" : SlipNumberInfo.InvoiceSuffix;
            prefix += invoice.IssueDate?.ToString("yyMM_");
            var numberString = (SlipNumberInfo.InvoiceLatest + 1).ToString("0000");
            var slipNumber = $"{prefix}{numberString}{suffix}";
            invoice.SlipNumber = slipNumber;
            AddNewInvoice(invoice);
        }
        
        private List<InvoiceClass> GetSelectedItemsInDisplayOrder(DataGrid dataGrid)
        {
            var selectedItems = dataGrid.SelectedItems.Cast<InvoiceClass>().ToList();
            var displayOrderItems = new List<InvoiceClass>();

            foreach (var item in dataGrid.Items)
            {
                if (selectedItems.Contains(item))
                {
                    displayOrderItems.Add(item as InvoiceClass);
                }
            }

            return displayOrderItems;
        }
        
        private void InvoiceItemsDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
        }

        private void CreateInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if(InvoiceListDataGrid.SelectedItems.Count > 0)
            {
                foreach (var item in InvoiceListDataGrid.SelectedItems)
                {
                    if (item is InvoiceClass invoice)
                    {
                        InvoicePdfGenerator generator = new();
                        var fileName = FileNameHelper.GenerateInvoiceFilename(@"c:\users\ma\desktop", invoice);
                        if (invoice.InvoiceItems.Count <= 0)
                        {
                            invoice.InvoiceItems = InvoiceItemClass.GetInvoiceItemsByInvoiceId(invoice.InvoiceId);
                        }
                        generator.CreateInvoicePdf(invoice,fileName);
                    }
                }

            }
        }
        private void InvoiceItemsDataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
        }
        
        private void ContextMenuDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (InvoiceListDataGrid.SelectedItems.Count > 0)
            {
                vm.CurrentInvoice = InvoiceListDataGrid.SelectedItems[0] as InvoiceClass;
                System.Windows.Controls.Page paymentPage = mainWindow.Payment;
                vm.DepositFromInvoicePage = true;
                mainWindow.MainFrame.Navigate(paymentPage);
            }

        }
        
        private void ContextMenuStatusChanged(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                var status = menuItem.Header.ToString();
                var statusList = vm.InvoiceStatusClassList;
                var dataGrid = InvoiceListDataGrid;
                var selectedItems = dataGrid.SelectedItems;
                foreach (var item in selectedItems)
                {
                    if (item is InvoiceClass invoice)
                    {
                        invoice.InvoiceStatus = status;
                        invoice.InvoiceStatusId = statusList.FirstOrDefault(st => st.InvoiceStatus == status).InvoiceStatusId;
                        invoice.TryUpdateInvoice();
                    }
                }
                vm.ReloadInvoiceList();
            }
        }

        private void InvoiceListDataGrid_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

            var originalSource = e.OriginalSource;
            if (originalSource is ScrollViewer)
                e.Handled = true;
        }

        private void InvoiceListDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource;
            if (originalSource is ScrollViewer)
            {
                SelectAllCheckBox_Unchecked(SelectAllCheckBox, new RoutedEventArgs());
            }

        }

        private void InvoiceListDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (InvoiceListDataGrid.SelectedItems.Count > 1)
            {
                ContextMenuStatusDeposited.IsEnabled = false;
                ContextMenuDeposit.IsEnabled = false;
            }
            else
            {
                if(InvoiceListDataGrid.SelectedItems.Count == 1)
                {
                    ContextMenuStatusDeposited.IsEnabled = true;
                    ContextMenuDeposit.IsEnabled = true;
                }
            }
        }

        private void InvoiceListDataGrid_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            ContextMenuStatusDeposited.IsEnabled = true;
            ContextMenuDeposit.IsEnabled = true;
        }

        private void TransactionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DepositLabelController();
        }

        private void DepositLabelController()
        {
            var comboBox = TransactionTypeComboBox;
            var selectedItem = comboBox.SelectedItem as TransactionTypeClass;
            if (selectedItem == null) return;
            if (selectedItem.TransactionName == "前受金")
            {
                InvoiceAmountGrid.Visibility = Visibility.Visible;
                DepositAmountGrid.Visibility = Visibility.Visible;
                var customerList = _customerVM.CustomerClassList;
                var customer = customerList.FirstOrDefault(c => c.CustomerId == vm.CurrentInvoice.CustomerId);
                if (customer != null)
                {

                    if (customer.CustomerBalance <= 0)
                    {
                        var balanceList = vm.BalanceClassList;
                        var customerBalanceList = balanceList.Where(b => b.CustomerId == customer.CustomerId && b.TransactionDate <= vm.CurrentInvoice.IssueDate).ToList();
                        var debitTotal = customerBalanceList.Where(b => b.TransactionTypeId == 1).Sum(b => b.TransactionAmount);
                        var creditTotal = customerBalanceList.Where(b => b.TransactionTypeId == 2).Sum(b => b.TransactionAmount);
                        var totalDepositUntilIssueDate = debitTotal - creditTotal;
                        
                        vm.DepositAmount = vm.CurrentInvoice.ItemsTotal - totalDepositUntilIssueDate <= 0 ? 0 : vm.CurrentInvoice.ItemsTotal - totalDepositUntilIssueDate;
                    }
                    else
                        vm.DepositAmount = 0;
                }
            }
            else
            {
                InvoiceAmountGrid.Visibility = Visibility.Collapsed;
                DepositAmountGrid.Visibility = Visibility.Collapsed;
                vm.DepositAmount = 0;
            }
        }
        private void CurrentInvoice_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine(e.PropertyName);
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // MainWindow の MouseDown イベントを処理
            if (DateBox.PopupIsOpen)
            {
                DateBox.PopupIsOpen = false;
            }
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textbox)
            {
                //InputMethod.SetPreferredImeConversionMode(textbox, ImeConversionModeValues.Native | ImeConversionModeValues.FullShape);
                InputMethod.SetIsInputMethodEnabled(textbox, true);
                InputMethod.SetPreferredImeState(textbox, InputMethodState.On);
            }
        }
        private void TextBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textbox)
            {
                InputMethod.SetIsInputMethodEnabled(mainWindow, false);
                InputMethod.SetPreferredImeState(mainWindow, InputMethodState.Off);
            }
        }

    }
}
