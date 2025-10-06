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
using MigraDoc.DocumentObjectModel.Tables;
using Invoice.Classes;
using Invoice.Accounting;
using Invoice.Pages;

namespace Invoice
{
    /// <summary>
    /// Invoice.xaml の相互作用ロジック
    /// </summary>
    public partial class InvoicePage : Controls.Page
    {
        private readonly CustomerViewModel _customerVM;
        private readonly PaymentViewModel _pVM;
        private readonly SettingsViewModel _sVM;
        private readonly InvoiceViewModel vm;
        private bool isEditing = false;
        private bool isInitializing = true;
        private bool isFirstLoading = true;
        private bool addFromCopy = false;
        public CultureInfo cultureInfo = new("ja-JP");
        private readonly InvoiceFiterParam filterParam = new();
        private readonly MainWindow mainWindow;

        private int prevTransactionTypeId = 0;

        public InvoicePage(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            this.Loaded += InvoicePage_Loaded;
            vm = mainWindowViewModel.InvoiceVM;
            _customerVM = mainWindowViewModel.CustomerVM;
            _pVM = mainWindowViewModel.PaymentVM;
            _sVM = mainWindowViewModel.SettingsVM;
            this.DataContext = mainWindowViewModel;
            
            cultureInfo.DateTimeFormat.Calendar = new JapaneseCalendar();
            cultureInfo.DateTimeFormat.ShortDatePattern = "ggy年M月d日";
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            vm.InvoiceCollectionViewSource = new()
            {
                Source = vm.InvoiceClassList
            };

            EventManager.RegisterClassHandler(typeof(MainWindow), PreviewMouseDownEvent, new MouseButtonEventHandler(MainWindow_MouseDown));

            mainWindow = (MainWindow)Application.Current.MainWindow;
            InvoiceFilter(new InvoiceFiterParam());
            InvoiceSubject.GotKeyboardFocus += TextBox_GotKeyboardFocus;
            InvoiceSubject.PreviewLostKeyboardFocus += TextBox_PreviewLostKeyboardFocus;
            MessageTextBox.GotKeyboardFocus += TextBox_GotKeyboardFocus;
            MessageTextBox.PreviewLostKeyboardFocus += TextBox_PreviewLostKeyboardFocus;
            filterParam.PropertyChanged += FilterParam_PropertyChanged;

        }
        
        private void FilterParam_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        private void InvoiceFilter(InvoiceFiterParam param)
        {
            if (vm.InvoiceCollectionViewSource is CollectionViewSource source)
            {
                //source.Filter += (sender, e) =>
                //{
                //    if (e.Item is InvoiceClass invoice)
                //    {
                //        if ((param.CustomerId == 0 || invoice.CustomerId == param.CustomerId) &&
                //            (param.InvoiceStatusId == 0 || invoice.InvoiceStatusId == param.InvoiceStatusId) &&
                //            (param.TransactionTypeId == 0 || invoice.TransactionTypeId == param.TransactionTypeId) &&
                //            (param.IssueDate == null || (StartOfMonth(param.IssueDate) <= invoice.IssueDate && invoice.IssueDate <= EndOfMonth(param.IssueDate))) &&
                //            (param.DueDate == null || invoice.DueDate == param.DueDate) &&
                //            (param.PaymentDate == null || invoice.PaymentDate == param.PaymentDate) &&
                //            (string.IsNullOrWhiteSpace(param.Subject) || invoice.Subject == param.Subject) &&
                //            (param.InvoiceId == 0 || invoice.InvoiceId == param.InvoiceId))
                //            e.Accepted = true;
                //        else
                //            e.Accepted = false;
                //    }
                //};
                source.Filter += (sender, e) =>
                {
                    if (e.Item is InvoiceClass invoice)
                    {
                        var conditions = new List<bool>
                        {
                            param.CustomerId == 0 || invoice.CustomerId == param.CustomerId,
                            param.InvoiceStatusId == 0 || invoice.InvoiceStatusId == param.InvoiceStatusId,
                            param.TransactionTypeId == 0 || invoice.TransactionTypeId == param.TransactionTypeId,
                            param.IssueDate == null || (StartOfMonth(param.IssueDate) <= invoice.IssueDate && invoice.IssueDate <= EndOfMonth(param.IssueDate)),
                            param.DueDate == null || invoice.DueDate == param.DueDate,
                            param.PaymentDate == null || invoice.PaymentDate == param.PaymentDate,
                            string.IsNullOrWhiteSpace(param.Subject) || invoice.Subject == param.Subject,
                            param.InvoiceId == 0 || invoice.InvoiceId == param.InvoiceId
                        };
                        e.Accepted = conditions.All(c => c);
                    }
                };

                if (isFirstLoading)
                {
                    source.SortDescriptions.Clear();
                    source.SortDescriptions.Add(new SortDescription(nameof(InvoiceClass.IssueDate), ListSortDirection.Descending));
                }
            }
        }

        private static DateTime StartOfMonth(DateTime? date)
        {
            if (date.HasValue) return new DateTime(date.Value.Year, date.Value.Month, 1);
            else
                return DateTime.MinValue;
            
        }

        private static DateTime EndOfMonth(DateTime? date)
        {
            if (date.HasValue) return new DateTime(date.Value.Year, date.Value.Month, DateTime.DaysInMonth(date.Value.Year, date.Value.Month));
            else
                return DateTime.MinValue;
        }

        private void InvoicePage_Loaded(object sender, RoutedEventArgs e)
        {
            vm.ReloadInvoiceList();
            if (vm.InvoiceCollectionViewSource is CollectionViewSource viewSource)
            {
                viewSource.Source = vm.InvoiceClassList;
                InvoiceFilter(filterParam);
                if (isInitializing)
                {
                    var arrow = FilterExpander.FindDescendantByName("arrow");
                    if (arrow != null && arrow.Parent is Grid grid)
                        grid.HorizontalAlignment = HorizontalAlignment.Right;
                }
                isFirstLoading = false;
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
                if(comboBox.SelectedItem is CustomerClass customer && customer.CustomerId != -1)
                {
                    filterParam.CustomerId = customer.CustomerId; 
                }
                else if(comboBox.SelectedItem is InvoiceStatusClass status && status.InvoiceStatusId != -1)
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
                if(Application.Current.MainWindow is MainWindow mainWindow)
                    mainWindow.Height += pane.Height - InvoicePageContentsGrid.ActualHeight;
            }
            else
            {
                pane.Height = InvoicePageContentsGrid.ActualHeight;
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
            renderTransform.BeginAnimation(TranslateTransform.YProperty, slideUpAnimation);



        }

        private void SlideUpAnimation_Completed(object? sender, EventArgs e)
        {
            isInitializing = false;
        }

        private void HideDetailPane()
        {
            if(InvoiceDetailPane.RenderTransform is TranslateTransform renderTransform)
            {
                var slideDownAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = InvoiceDetailPane.Height,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                renderTransform.BeginAnimation(TranslateTransform.YProperty, slideDownAnimation);
                //pane.Visibility = Visibility.Collapsed;
                InvoicePageContentsGrid.IsEnabled = true;
                vm.CurrentInvoice = new InvoiceClass();
            }
        }

        private void AddInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            isInitializing = true;
            isEditing = false;
            vm.CurrentInvoice = new InvoiceClass();
            vm.CurrentInvoice.InvoiceItems.Clear();
            var defaultItems = _sVM.DefaultItemsList.ToList();
            defaultItems.ForEach(item => vm.CurrentInvoice.InvoiceItems.Add(item.ToInvoiceItem()));
            
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
            vm.CurrentInvoice.Subject = $"利用料 {issueDate:ggy年M月分}";

            vm.SaveButtonText = "保存";
            vm.PaneTitle = "新規請求書作成";
            //vm.CurrentInvoice.RecalculateTotals();
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
            List<int> idList = [];
            foreach (var item in InvoiceListDataGrid.SelectedItems)
            {
                if (item is InvoiceClass invoice) idList.Add(invoice.InvoiceId);
            }
            foreach (int invoiceId in idList)
            {
                var result = UnitOfWork.ExecuteWithTransaction(uow =>
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
                                PaymentClass.TryDeletePaymentById(TypeOfID.Invoice, invoiceId,uow);
                                break;
                            case MessageBoxResult.No:
                                PaymentClass.ClearInvoiceIdFromPayment(invoiceId,uow);
                                break;
                            case MessageBoxResult.Cancel:
                            default:
                                return false;
                        }
                    }
                    InvoiceClass.DeleteInvoiceByInvoiceId(invoiceId,uow);
                    return true;
                },null);
                if (result == false)
                {
                    MessageBox.Show("請求書の削除に失敗しました。");
                    return;
                }
            }
            _pVM.ReloadPaymentList();
            vm.ReloadInvoiceList();
            _customerVM.ReloadBalances();

        }

        private void InvoiceCancelButton_Click(object sender, RoutedEventArgs e)
        {
            HideDetailPane();
            vm.CurrentInvoice = new();
        }

        private void AddInvoiceItemButton_Click(object sender, RoutedEventArgs e)
        {
            // ViewModelへの参照を取得（DataContextをInvoiceViewModelに設定している場合）
            
            if (vm != null)
            {
                var newItem = new InvoiceItemClass()
                {
                    ItemOrder = vm.CurrentInvoice.InvoiceItems.Count + 1
                };
                vm.CurrentInvoice.InvoiceItems.Add(newItem);
                foreach (var item in vm.CurrentInvoice.InvoiceItems)
                {
                    item.ItemOrder = vm.CurrentInvoice.InvoiceItems.IndexOf(item) + 1;
                }
            }
        }

        private void SaveInvoiceButton_Click(object sendwe, RoutedEventArgs e)
        {
            var invoice = vm.CurrentInvoice;

            
            var statusInfo = StatusComboBox.SelectedItem as InvoiceStatusClass;
            invoice.InvoiceStatusId = statusInfo?.InvoiceStatusId ?? 1;
            invoice.InvoiceStatus = statusInfo?.InvoiceStatus ?? "作成中";

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
                
                _customerVM.ReloadBalances();
                HideDetailPane();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{GetType().Name}.{MethodBase.GetCurrentMethod()!.Name} : {ex.Message}");
            }
        }
        
        public void AddNewInvoice(InvoiceClass invoice)
        { //新規登録
            
            var slipNumberInfo = _sVM.slipNumbers;
            var slipNumber = slipNumberInfo.GetSlipNumber(invoice.IssueDate);
            var numberString = slipNumber.InvoiceNumber;
            invoice.SlipNumber = numberString;
            invoice.PaidByDeposit = vm.CurrentInvoice.PaidByDeposit;
            SetInvoiceStatus(invoice);
            if (invoice.TryAddInvoice() == true) slipNumber.InclimentInvoiceLatest();

        }

        public void UpdateInvoice(InvoiceClass invoice)
        { //更新
            var ret = UnitOfWork.ExecuteWithTransaction(uow =>
            {
                SetInvoiceStatus(invoice);
                if (prevTransactionTypeId == 2)
                { //更新前が前受清算の請求情報
                    //前受金→前受金
                    if (invoice.TransactionTypeId == 2)
                        return DepositClass.TryUpdateDeposit(invoice, uow);
                        //前受金→売掛金
                    else 
                        return DepositClass.DeleteDepositById(TypeOfID.Invoice, invoice.InvoiceId, uow);
                }
                else
                { //更新前が売掛金の請求情報
                    //売掛金→前受金
                    if (invoice.TransactionTypeId == 2)
                        return DepositClass.TryAddDeposit(invoice, uow);
                    //売掛金→売掛金
                    else
                        return true;//何もしない
                }
            }, null);
            if (ret == true)
                // 請求書の更新実行
                if (!invoice.TryUpdateInvoice()) throw new Exception("請求書の更新に失敗しました。");
        }

        private void SetInvoiceStatus(InvoiceClass invoice)
        {
            if (invoice.TransactionTypeId == 2 && invoice.InvoiceTotal == 0)
            {
                invoice.InvoiceStatus = "入金済";
                invoice.InvoiceStatusId = 3;
                invoice.PaymentDate = invoice.IssueDate;
                invoice.PaidByDeposit = vm.CurrentInvoice.PaidByDeposit;
            }
            else
            {
                invoice.InvoiceStatus = "請求済";
                invoice.InvoiceStatusId = 2;
                invoice.PaymentDate = null;
                invoice.PaidByDeposit = 0;
            }

        }

        private void InvoiceItemName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            //if (isInitializing) return;

            if (sender is ComboBox comboBox)
            {
                var selectedItem =comboBox.SelectedItem;
                if (isInitializing) return;
                if (comboBox.DataContext is InvoiceItemClass invoiceItem)
                {
                    if (invoiceItem != null && selectedItem != null)
                    {
                        ItemClass? comboBoxSelectedItem = new();
                        if (selectedItem is InvoiceItemClass item)
                            comboBoxSelectedItem = vm.ItemClassList.FirstOrDefault(i => i.ItemId == item.ItemId);
                        else
                            comboBoxSelectedItem = selectedItem as ItemClass;

                        invoiceItem.SetItem(comboBoxSelectedItem!);
                        var contentPresenter = comboBox.TemplatedParent as ContentPresenter;
                        var dataGridCell = contentPresenter!.Parent as DataGridCell;
                        var dataGridCellsPanel = VisualTreeHelper.GetParent(dataGridCell) as DataGridCellsPanel;
                        var taxTypeNameComboBox = TaxTypeNameComboBox as DataGridComboBoxColumn;
                        var itemSource = taxTypeNameComboBox.ItemsSource as ListCollectionView;
                        itemSource!.MoveCurrentToFirst();
                    }
                    else
                    {
                        if (comboBox.DataContext == null) return;
                    }
                }
            }

        }

        private void InvoiceIssueDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DatePicker datePicker && datePicker.SelectedDate != null && vm != null)
            {
                var issueDate = (DateTime)datePicker.SelectedDate;
                vm.CurrentInvoice.IssueDateString = issueDate.ToShortDateString();
                var tempDate = issueDate.AddMonths(1);
                vm.CurrentInvoice.DueDate = new DateTime(year: tempDate.Year, month: tempDate.Month, 15);
                InvoiceDueDate.SelectedDate = vm.CurrentInvoice.DueDate;
                DepositLabelController(sender, e);
            }

        }

        private void InvoiceName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is CustomerClass item && item.CustomerId > 0 && sender is ComboBox comboBox && comboBox.SelectedItem is CustomerClass customerItem && customerItem.CustomerId > 0)
            {
                    vm.CurrentInvoice.CustomerName = _customerVM.CustomerClassList.FirstOrDefault(c => c.CustomerId == customerItem.CustomerId)!.CustomerName;
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
                    VisualTreeHelperExtensions.FindVisualChildByName<CheckBox>(row, "GridRowCheckBox")!.IsChecked = true;
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
                    VisualTreeHelperExtensions.FindVisualChildByName<CheckBox>(row, "GridRowCheckBox")!.IsChecked = false;

                }
            }

        }

        private void InvoiceCheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is CheckBox checkBox)
            {
                var InvoiceItem = checkBox.FindAscendant<DataGridRow>();
                if (InvoiceItem != null)
                {
                    InvoiceItem.IsSelected = checkBox.IsChecked == false;
                }
                e.Handled = true;
            }
        }

        private void DeleteInvoiceItemButton_Click(object sender, RoutedEventArgs e)
        {
            var dataGrid = InvoiceItemsDataGrid;
            var selectedItems = dataGrid.SelectedItems;
            List<int> orderList = [];
            foreach (var item in selectedItems)
            {
                if (item is InvoiceItemClass invoiceItem) orderList.Add(invoiceItem.ItemOrder);
            }
            orderList.Sort((a, b) => b - a);
            foreach (var itemOrder in orderList)
            {
                var item = vm.CurrentInvoice.InvoiceItems[itemOrder - 1];
                var id = item.InvoiceItemId;
                vm.CurrentInvoice.InvoiceItems.Remove(item);
            }
            foreach (var item in vm.CurrentInvoice.InvoiceItems)
            {
                item.ItemOrder = vm.CurrentInvoice.InvoiceItems.IndexOf(item) + 1;
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
            var selectedInvoices = GetSelectedItemsInDisplayOrder(dataGrid);
            addFromCopy = true;
            foreach (InvoiceClass invoice in selectedInvoices)
            {
                var newInvoice = invoice.DeepClone();
                var tempNewIssueDate = invoice.IssueDate?.AddMonths(2) ?? DateTime.Now;
                newInvoice.InvoiceId = 0;
                newInvoice.IssueDate = new DateTime(tempNewIssueDate.Year, tempNewIssueDate.Month, 1).AddDays(-1);
                newInvoice.SlipNumber ="";
                newInvoice.InvoiceStatusId = 1;
                newInvoice.InvoiceStatus = "作成中";
                newInvoice.IssueDateString = newInvoice.IssueDate?.ToShortDateString();
                newInvoice.DueDate = newInvoice.IssueDate?.AddDays(16);
                newInvoice.Subject = newInvoice.IssueDate?.ToString("利用料 ggy年M月分");
                vm.CurrentInvoice = newInvoice.DeepClone();
                AddNewInvoice(vm.CurrentInvoice);
            }
            _pVM.ReloadPaymentList();
            vm.ReloadInvoiceList();
            _customerVM.ReloadBalances();
            addFromCopy = false;

        }

        private static List<InvoiceClass> GetSelectedItemsInDisplayOrder(DataGrid dataGrid)
        {
            var selectedItems = dataGrid.SelectedItems.Cast<InvoiceClass>().ToList();
            var displayOrderItems = new List<InvoiceClass>();

            foreach (var item in dataGrid.Items)
            {
                if (selectedItems.Contains(item) && item is InvoiceClass invoice)
                {
                    displayOrderItems.Add(invoice);
                }
            }

            return displayOrderItems;
        }
        
        private void CreateInvoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if(InvoiceListDataGrid.SelectedItems.Count > 0)
            {
                var outputDir = SettingsManager.Get("OutputDirectory");
                if (string.IsNullOrWhiteSpace(outputDir))
                    outputDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                foreach (var item in InvoiceListDataGrid.SelectedItems)
                {
                    if (item is InvoiceClass invoice)
                    {
                        InvoicePdfGenerator generator = new();
                        var fileName = FileNameHelper.GenerateInvoiceFilename(outputDir, invoice);
                        if (invoice.InvoiceItems.Count <= 0)
                        {
                            var items = InvoiceItemClass.GetInvoiceItemsByInvoiceId(invoice.InvoiceId);
                            items.ForEach(item => invoice.InvoiceItems.Add(item));
                        }
                        generator.CreateInvoicePdf(invoice,fileName);
                    }
                }

            }
        }

        private void ContextMenuDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (InvoiceListDataGrid.SelectedItems.Count > 0 && InvoiceListDataGrid.SelectedItems[0] is InvoiceClass invoice)
            {
                vm.CurrentInvoice = invoice;
                System.Windows.Controls.Page paymentPage = mainWindow.Payment;
                vm.DepositFromInvoicePage = true;
                mainWindow.MainFrame.Navigate(paymentPage);
            }

        }
        
        private void ContextMenuStatusChanged(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                var status = menuItem.Header.ToString() ?? "";
                var statusList = vm.InvoiceStatusClassList;
                var dataGrid = InvoiceListDataGrid;
                var selectedItems = dataGrid.SelectedItems;
                foreach (var item in selectedItems)
                {
                    if (item is InvoiceClass invoice)
                    {
                        invoice.InvoiceStatus = status;
                        invoice.InvoiceStatusId = statusList.FirstOrDefault(st => st.InvoiceStatus == status)!.InvoiceStatusId;
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
            DepositLabelController(sender, e);
        }

        private void DepositLabelController(object sender, SelectionChangedEventArgs e)
        {

            var comboBox = TransactionTypeComboBox;
            var balanceList = _customerVM.BalanceClassList;
            var customerList = _customerVM.CustomerClassList;
            var invoice = vm.CurrentInvoice;

            if (comboBox.SelectedItem is TransactionTypeClass selectedItem && selectedItem != null )
            {
                if (selectedItem.TransactionName == "前受金")
                {
                    var customer = customerList.FirstOrDefault(c => c.CustomerId == invoice.CustomerId);
                    var customerBalanceList = balanceList
                        .Where(
                        b => b.CustomerId == customer!.CustomerId
                          && b.TransactionDate <= invoice.IssueDate
                          && b.InvoiceId != invoice.InvoiceId
                          )
                        .ToList();
                    var debitTotal = customerBalanceList.Where(b => b.DebOrCreId == 1).Sum(b => b.TransactionAmount);
                    var creditTotal = customerBalanceList.Where(b => b.DebOrCreId == 2).Sum(b => b.TransactionAmount);
                    var depositUntilIssueDate = creditTotal - debitTotal;// 前受残高
                    customerBalanceList.ForEach(bal => Debug.WriteLine($"{bal.InvoiceId} : {bal.TransactionDate} : {bal.DebOrCreId} : {bal.TransactionAmount}"));
                    var afterPaidDeposit = depositUntilIssueDate - invoice.ItemsTotal;// 当該請求額支払後 前受残高
                    var vallist = customerBalanceList.Where(b => b.TransactionTypeId == 1);
                    var paidByDeposit = afterPaidDeposit <= 0 ? depositUntilIssueDate : invoice.ItemsTotal;// 前受精算額（前受が不足の場合は前受残高）
                    // var invoiceTotal = invoice.ItemsTotal - paidByDeposit;// 当該請求書による請求額
                    vm.CurrentInvoice.DepositUntilIssueDate = depositUntilIssueDate;
                    vm.CurrentInvoice.PaidByDeposit = paidByDeposit;

                    // 表示用の残高（当該請求適用後の残高を表示する仕様に合わせる）
                    vm.DepositAmount = depositUntilIssueDate - paidByDeposit;

                    var total = vm.CurrentInvoice.InvoiceTotal;
                    InvoiceAmountGrid.Visibility = Visibility.Visible;
                    DepositAmountGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    vm.CurrentInvoice.PaidByDeposit = 0;
                    vm.CurrentInvoice.DepositUntilIssueDate = 0;
                    var total = vm.CurrentInvoice.InvoiceTotal;
                    InvoiceAmountGrid.Visibility = Visibility.Collapsed;
                    DepositAmountGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // MainWindow の MouseDown イベントを処理
            if (DateBox.PopupIsOpen)
            {
                DateBox.PopupIsOpen = false;
                e.Handled = true;
            }
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textbox)
            {
                InputMethod.SetIsInputMethodEnabled(textbox, true);
                InputMethod.SetPreferredImeState(textbox, InputMethodState.On);
            }
        }

        private void TextBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textbox)
            {
                InputMethod.SetIsInputMethodEnabled(textbox, false);
                InputMethod.SetPreferredImeState(textbox, InputMethodState.Off);
            }
        }


    }
}
