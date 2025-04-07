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

namespace Invoice
{
    /// <summary>
    /// Invoice.xaml の相互作用ロジック
    /// </summary>
    public partial class InvoicePage : Controls.Page
    {
        NumberBox numberBox = new();
        private readonly DateTime BaseDate = new(2000, 1, 1);
        DateTime TargetDate;
        private CustomerViewModel _customerVM;
        private PaymentViewModel _pVM;
        private InvoiceViewModel vm;
        private bool isEditing = false;
        private bool isInitializing = true;
        public CultureInfo cultureInfo = new("ja-JP");
        InvoiceClass CurrentInvoice;
        private InvoiceFiterParam filterParam = new();
        private MainWindow mainWindow;

        SlipNumberClass SlipNumberInfo = new();
        public InvoicePage(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            this.Loaded += InvoicePage_Loaded;
            vm = mainWindowViewModel.InvoiceVM;
            _customerVM = mainWindowViewModel.CustomerVM;
            _pVM = mainWindowViewModel.PaymentVM;
            this.DataContext = mainWindowViewModel;
            SlipNumberInfo = SlipNumberClass.GetSlipNumberInfo();
            cultureInfo.DateTimeFormat.Calendar = new JapaneseCalendar();
            cultureInfo.DateTimeFormat.ShortDatePattern = "ggy年M月d日";
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            vm.InvoiceCollectionViewSource = new CollectionViewSource();
            vm.InvoiceCollectionViewSource.Source = vm.InvoiceClassList;

            mainWindow = Application.Current.MainWindow as MainWindow;
            DateBox.Value = MonthDiff(BaseDate, DateTime.Now);
            CurrentInvoice = vm.CurrentInvoice;
            InvoiceFilter(new InvoiceFiterParam());

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
        }

        private void DateBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (vm == null) return;
            int newValue = (int)(args.NewValue);
            vm.ViewDate = BaseDate.AddMonths(newValue).ToString("ggy年M月", cultureInfo);
            TargetDate = BaseDate.AddMonths(newValue);
            filterParam.IssueDate = TargetDate;
            InvoiceFilter(filterParam);
        }
            //Init.TargetMonth = TargetDate;        }
        public static int MonthDiff(DateTime FromDate, DateTime ToDate)
        {
            // 月差計算（年差考慮(差分1年 → 12(ヶ月)加算)）
            int iRet = ((ToDate.Year - FromDate.Year) * 12) + ToDate.Month - 2;
            return iRet;
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
            vm.InvoiceItemClassList.Add(new InvoiceItemClass());
            
            
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
                vm.SaveButtonText = "更新";
                vm.PaneTitle = "請求書編集";

                vm.InvoiceItemClassList.Clear(); 
                var items = (InvoiceItemClass.GetInvoiceItemsByInvoiceId(selectedInvoice.InvoiceId));
                
                foreach(var item in items)
                {
                    vm.InvoiceItemClassList.Add(item);
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
                                            icon: MessageBoxImage.Hand
                                            );
                    switch (a)
                    {
                        case MessageBoxResult.Yes:
                            PaymentClass.DeletePaymentByInvoiceId(invoiceId);
                            break;

                        case MessageBoxResult.No:
                            PaymentClass.ClearInvoiceIdFromPayment(invoiceId);
                            break;
                        case MessageBoxResult.Cancel:
                            return;
                    }
                }
                InvoiceItemClass.DeleteInvoiceItemsByInvoiceId(invoiceId);
                new InvoiceClass().DeleteInvoiceById(invoiceId);
            }
            vm.ReloadInvoiceList();
            _pVM.ReloadPaymentList();
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
            var invoice = vm.CurrentInvoice;
            invoice.SubTotal = vm.InvoiceItemClassList.Sum(x => x.ItemSubTotal);
            invoice.Tax = vm.InvoiceItemClassList.Sum(x => x.Tax);
            invoice.Total = vm.InvoiceItemClassList.Sum(x => x.ItemTotal);

            var statusInfo = StatusComboBox.SelectedItem as InvoiceStatusClass;
            invoice.InvoiceStatusId = statusInfo?.InvoiceStatusId ?? 1;
            invoice.InvoiceStatus = statusInfo?.InvoiceStatus ?? "作成中";

            invoice.IssueDateString = vm.CurrentInvoice.IssueDateString;

            try
            {
                invoice.Subject = InvoiceSubject.Text;
                if (!isEditing)
                {
                    //新規登録
                    var prefix = string.IsNullOrWhiteSpace(SlipNumberInfo.InvoicePrefix) ? "" : SlipNumberInfo.InvoicePrefix;
                    var suffix = string.IsNullOrWhiteSpace(SlipNumberInfo.InvoiceSuffix) ? "" : SlipNumberInfo.InvoiceSuffix;
                    prefix += invoice.IssueDate.ToString("yyMM_");
                    var numberString = (SlipNumberInfo.InvoiceLatest + 1).ToString("0000");
                    var slipNumber = $"{prefix}{numberString}{suffix}";
                    invoice.SlipNumber = slipNumber;

                    if (invoice.TryAddInvoice())
                    {
                        SlipNumberInfo.InclimentInvoiceLatest();
                        InvoiceItemClass.AddInvoiceItem(vm.InvoiceItemClassList.ToList(),invoice.InvoiceId);
                        
                        vm.InvoiceClassList.Add(invoice);
                    }
                    else
                    {
                        throw new Exception("請求書の登録に失敗しました。");
                    }
                
                }
                else
                {
                    //更新

                    if (invoice.TryUpdateInvoice())
                    {
                        InvoiceItemClass.DeleteInvoiceItemsByInvoiceId(invoice.InvoiceId);
                        InvoiceItemClass.AddInvoiceItem(vm.InvoiceItemClassList.ToList(), invoice.InvoiceId);
                    }
                    else
                    {
                        throw new Exception("請求書の更新に失敗しました。");
                    }
                }
                vm.ReloadInvoiceList();
                HideDetailPane();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
            var comboBox = sender as ComboBox;
            var customerItem = comboBox.SelectedItem as CustomerClass;
            if (customerItem != null)
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
            if (!toggleSwitch.IsOn)
                filterParam.IssueDate = null;
            else
                filterParam.IssueDate = TargetDate;

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
                    var invoiceItems = InvoiceItemClass.GetInvoiceItemsByInvoiceId(invoice.InvoiceId);

                    var tempNewIssueDate = invoice.IssueDate.AddMonths(2);
                    var newIssueDate = new DateTime(tempNewIssueDate.Year, tempNewIssueDate.Month, 1).AddDays(-1);
                    var newDueDate = newIssueDate.AddDays(16);
                    var newInvoice = invoice.DeepClone();
                    newInvoice.InvoiceId = 0;
                    newInvoice.SlipNumber ="";
                    newInvoice.InvoiceStatusId = 1;
                    newInvoice.InvoiceStatus = "作成中";
                    newInvoice.IssueDate = newIssueDate;
                    newInvoice.IssueDateString = newIssueDate.ToShortDateString();
                    newInvoice.DueDate = newDueDate;
                    newInvoice.SubTotal = invoiceItems.Sum(x => x.ItemSubTotal);
                    newInvoice.Tax = invoiceItems.Sum(x => x.Tax);
                    newInvoice.Total = invoiceItems.Sum(x => x.ItemTotal);
                    newInvoice.CustomerId = invoice.CustomerId;
                    newInvoice.CustomerName = invoice.CustomerName;
                    newInvoice.Subject = newInvoice.IssueDate.ToString("利用料 ggy年M月分");
                    CopyInvoiceAdd(newInvoice, invoiceItems);
                    newInvoice.InvoiceItems = invoiceItems;
                    SlipNumberInfo.InclimentInvoiceLatest();
                }
            }
            vm.ReloadInvoiceList();

        }

        private void CopyInvoiceAdd(InvoiceClass invoice, List<InvoiceItemClass> invoiceItems)
        {
            var prefix = string.IsNullOrWhiteSpace(SlipNumberInfo.InvoicePrefix) ? "" : SlipNumberInfo.InvoicePrefix;
            var suffix = string.IsNullOrWhiteSpace(SlipNumberInfo.InvoiceSuffix) ? "" : SlipNumberInfo.InvoiceSuffix;
            prefix += invoice.IssueDate.ToString("yyMM_");
            var numberString = (SlipNumberInfo.InvoiceLatest + 1).ToString("0000");
            var slipNumber = $"{prefix}{numberString}{suffix}";
            invoice.SlipNumber = slipNumber;

            if (invoice.TryAddInvoice())
            {
                InvoiceItemClass.AddInvoiceItem(invoiceItems, invoice.InvoiceId);

                //var balance = new BalanceClass()
                //{
                //    CustomerId = invoice.CustomerId,
                //    InvoiceId = invoice.InvoiceId,
                //    DebOrCreId = 1,
                //    SlipNumber = invoice.SlipNumber,
                //    TransactionAmount = invoice.Total,
                //    TransactionDate = invoice.IssueDate,
                //    TransactionTypeId = invoice.TransactionTypeId
                //};
                //balance.AddBalance();
                //SlipNumberInfo.InclimentInvoiceLatest();

            }

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
        private void InvoiceItem_Added(object sender, EventArgs e)
        {
            var itemList = vm.ItemClassList;
            var invoiceItemList = vm.InvoiceItemClassList;
            invoiceItemList.Add(sender as InvoiceItemClass);

        }
        private void InvoiceItem_Changed(object sender, EventArgs e)
        {
            var itemList = vm.ItemClassList;
            if(sender is InvoiceItemClass invoiceItem)
            {
                
                var item = itemList.FirstOrDefault(item => item.ItemId == invoiceItem.ItemId);
                
                //invoiceItem.SetItem(item);
                
            }


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
    }
}
