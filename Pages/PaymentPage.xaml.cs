using Invoice.Accounting;
using Invoice.Classes;
using Invoice.Converters;
using Invoice.Pages;
using Invoice.PdfGenerators;
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
using System.Reflection;
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

namespace Invoice
{
    /// <summary>
    /// Payment.xaml の相互作用ロジック
    /// </summary>
    public partial class PaymentPage : Controls.Page
    {
        // フィールド
        private readonly CustomerViewModel customerVM;
        private readonly InvoiceViewModel invoiceVM;
        private readonly PaymentViewModel paymentVM;
        private readonly SettingsViewModel settingsVM;
        public CultureInfo cultureInfo = new("ja-JP");
        private readonly SlipNumbers slipNumbers = new();
        private bool isEditing = false;
        private bool isFirstLoading = true;
        private readonly PaymentFilterParam paymentFilterParam = new();
        private InvoiceFiterParam filterParameter = new();
        private readonly MainWindow mainWindow;
        private PaymentClass? CurrentPayment;
        private int prevTransactionTypeId = 0;

        // コンストラクタ
        public PaymentPage(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;
            this.DataContext = mainWindowViewModel;
            this.Loaded += PaymentPage_Loaded;
            cultureInfo.DateTimeFormat.Calendar = new JapaneseCalendar();
            cultureInfo.DateTimeFormat.ShortDatePattern = "ggy年M月d日";
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            InputLanguageManager.SetInputLanguage(this, new System.Globalization.CultureInfo("ja-JP"));

            customerVM = mainWindowViewModel.CustomerVM;
            invoiceVM = mainWindowViewModel.InvoiceVM;
            paymentVM = mainWindowViewModel.PaymentVM;
            settingsVM = mainWindowViewModel.SettingsVM;
            slipNumbers = settingsVM.slipNumbers;
            paymentVM.PaymentListViewSource = new() { Source = paymentVM.PaymentClassList };
            paymentVM.InvoiceListForPayment = new() { Source = invoiceVM.InvoiceClassList };
            var converter = (InvoiceIdToSlipNumberConverter)this.Resources["InvoiceIdToSlipNumberConverter"];
            converter.InvoiceClassList = mainWindowViewModel.InvoiceVM.InvoiceClassList;
            EventManager.RegisterClassHandler(typeof(MainWindow), PreviewMouseDownEvent, new MouseButtonEventHandler(MainWindow_MouseDown));
            PaymentAmountTextBox.GotKeyboardFocus += PaymentAmountTextBox_GotKeyboardFocus;

            CurrentPayment = paymentVM.CurrentPayment;
            ReceiptSubject.GotKeyboardFocus += ReceiptSubject_GotKeyboardFocus;
            ReceiptSubject.PreviewLostKeyboardFocus += ReceiptSubject_PreviewLostKeyboardFocus;
            
        }


        // イベントハンドラ
        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label)
            {
                mainWindow?.NavigateToPage(label.Name);
            }
        }
        
        private void PaymentPage_Loaded(object sender, RoutedEventArgs e)
        {
            paymentVM.ReloadPaymentList();
            invoiceVM.ReloadInvoiceList();
            customerVM.ReloadCustomers(true);
            customerVM.ReloadBalances();
            // 請求書からの入金処理かで分岐
            if (!invoiceVM.DepositFromInvoicePage)
            {// 請求書からの入金処理ではない
                paymentVM.ReloadPaymentList();
                PaymentListFilter();
                isFirstLoading = false;
            }
            else
            {// 請求書からの入金処理
                // 請求番号で請求書リストをフィルタリング
                PaymentForInvoiceSwitch.IsOn = true;
                DateFilterSwitch.IsOn = false;
                filterParameter = new InvoiceFiterParam(){ InvoiceId = invoiceVM.CurrentInvoice.InvoiceId };
                InvoiceListForPaymentFilter();
                // フィルタリングされた請求書の数で分岐
                if (InvoiceListDataGrid.Items.Count != 1)
                {// 請求書が無いもしくは複数存在する => 例外処理（メッセージの表示後に元の状態へ）
                    HideDetailPane();
                    filterParameter = new InvoiceFiterParam();
                    InvoiceListForPaymentFilter();
                    invoiceVM.DepositFromInvoicePage = false;
                    mainWindow.MainFrame.Navigate(mainWindow.InvoicePage);
                    throw new Exception("支払対象の請求書が不正です");
                }
                // フィルタリングされた請求書を選択状態に
                // 請求書のInvoiceIdが登録されている入金情報を取得
                CurrentPayment = paymentVM.PaymentClassList.FirstOrDefault(p => p.InvoiceId == invoiceVM.CurrentInvoice.InvoiceId);
                // 請求書に紐づいた入金情報の有無で分岐
                if (CurrentPayment == null)
                {// 請求書に紐づいた入金情報が無い。=> 新規の入金情報を請求情報に紐づけて作成
                    AddPaymentButton_Click(AddPaymentButton, new RoutedEventArgs());
                    InvoiceListDataGrid.SelectedIndex = 0;
                    TransactionTypeBox.SelectedIndex = 0;
                }
                else
                {// 請求書に紐づいた入金情報を編集状態に
                    PaymentListDataGrid.SelectedItem = CurrentPayment;
                    EditPaymentButton_Click(EditPaymentButton, new RoutedEventArgs());
                }

            }

        }

        private void PaymentListDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditPaymentButton_Click(EditPaymentButton, new RoutedEventArgs());
        }

        private void AddPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            isEditing = false;
            paymentVM.SaveButtonText = "保存";
            paymentVM.PaneTitle = "入金記録作成";
            ShowDatailPane();
        }

        private void EditPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            isEditing = true;
            if(PaymentListDataGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show(
                    owner: mainWindow,
                    messageBoxText: "編集する入金記録を選択してください",
                    caption: "記録未選択",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Hand
                    );
                return;
            }
            paymentVM.SaveButtonText = "更新";
            paymentVM.PaneTitle = "入金記録編集";
            if(PaymentListDataGrid.SelectedItem is PaymentClass payment)
            {
                prevTransactionTypeId = payment.TransactionTypeId;
                CurrentPayment = payment;
                var detail = paymentVM.CurrentPayment;
                detail = payment;
                var customer = customerVM.CustomerClassList.FirstOrDefault(cu => cu.CustomerId == payment.CustomerId);
                paymentVM.CurrentPayment = detail;
                CustomerNameComboBox.SelectedItem = customer;
            
                TransactionTypeBox.SelectedItem = settingsVM.TransactionTypeClassList.FirstOrDefault(tType => tType.TransactionTypeId == payment.TransactionTypeId);
                PaymentDate.SelectedDate = payment.PaymentDate;
                PaymentDate.DisplayDate = payment.PaymentDate;
                if(payment.InvoiceId != null)
                {
                    PaymentForInvoiceSwitch.IsOn = true;
                    filterParameter = new()
                    {
                        InvoiceId = payment.InvoiceId ?? 0
                    };
                }
                else
                {
                    PaymentForInvoiceSwitch.IsOn = false;
                    filterParameter = new();
                }
                DateFilterSwitch.IsOn = false;
                InvoiceListForPaymentFilter();
                ShowDatailPane();
            }

        }

        private void DeletePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentListDataGrid.SelectedItems.Count == 0) return;
            var selectedItemList = PaymentListDataGrid.SelectedItems;
            var paymentList = selectedItemList.Cast<PaymentClass>();
            List<int> idList = [.. paymentList.Select(p => p.PaymentId)];

            foreach(int paymentId in idList)
            {
                var payment = paymentVM.PaymentClassList.FirstOrDefault(p => p.PaymentId == paymentId);
                if (payment == null) continue;
                var invoiceId = payment.InvoiceId;
                if (invoiceId != null)
                {
                    var invoice = invoiceVM.InvoiceClassList.FirstOrDefault(i => i.InvoiceId == invoiceId);
                    if (invoice != null) invoice.InvoiceStatusId = 2;
                }
                var depositId = payment.DepositId;
                var unitOfWork = new UnitOfWork();
                var result = UnitOfWork.ExecuteWithTransaction(uow =>
                {
                    if (payment.TransactionTypeId == 2) DepositClass.DeleteDepositById(new IDs(paymentId:payment.PaymentId), uow);
                    payment?.TryDeletePayment(uow);
                    return true;
                }, null);


            }
            paymentVM.ReloadPaymentList();
            invoiceVM.ReloadInvoiceList();
            customerVM.ReloadBalances();
        }

        private void PaymentCancelButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentPayment = new PaymentClass();
            HideDetailPane();
            if (invoiceVM.DepositFromInvoicePage)
            {
                filterParameter = new InvoiceFiterParam();
                InvoiceListForPaymentFilter();

                invoiceVM.DepositFromInvoicePage = false;
                mainWindow.MainFrame.Navigate(mainWindow.InvoicePage);
            }

        }

        private void SavePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            var payment = paymentVM.CurrentPayment;
            if (PaymentForInvoiceSwitch.IsOn && InvoiceListDataGrid.SelectedItem == null)
            {
                MessageBox.Show("支払対象の請求書を選択してください。");
                return;
            }
            if (TransactionTypeBox.SelectedIndex < 0 || (TransactionTypeBox.SelectedItem is not TransactionTypeClass))
            {
                MessageBox.Show("支払方法を選択してください。");
                return;
            }
            if (CustomerNameComboBox.SelectedIndex <= 0)
            {
                MessageBox.Show("宛先を選択してください");
                return;
            }
            if (CustomerNameComboBox.SelectedItem is CustomerClass customer)
            {
                payment.CustomerId = customer.CustomerId;
                payment.CustomerName = customer.CustomerName;
                payment.PaymentDate = payment.PaymentDate;
                payment.TransactionTypeId = ((TransactionTypeClass)TransactionTypeBox.SelectedItem).TransactionTypeId;
                payment.InvoiceId = null;
                if (PaymentForInvoiceSwitch.IsOn)
                {
                    var invoice = InvoiceListDataGrid.SelectedItem as InvoiceClass;
                    payment.InvoiceId = invoice?.InvoiceId ?? null;
                }
            }


            try
            {
                if (!isEditing)
                {
                    AddNewPayment(payment);
                }
                else
                {
                    UpdatePayment(payment);
                }

                // 保存後の請求書の状態を更新
                HideDetailPane();
                paymentVM.ReloadPaymentList();
                invoiceVM.ReloadInvoiceList();
                customerVM.ReloadBalances();
                customerVM.ReloadCustomers(true);
                if (invoiceVM.DepositFromInvoicePage)
                {
                    filterParameter = new InvoiceFiterParam();
                    InvoiceListForPaymentFilter();

                    invoiceVM.DepositFromInvoicePage = false;
                    mainWindow.MainFrame.Navigate(mainWindow.InvoicePage);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"{GetType().Name}.{MethodBase.GetCurrentMethod()!.Name} : {ex.Message}");
            }

        }
        
        private void PaymentDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is DatePicker datePicker && datePicker.SelectedDate != null && paymentVM != null)
            {
                var paymentDate = (DateTime)datePicker.SelectedDate;
                paymentVM.CurrentPayment.PaymentDateString = paymentDate.ToShortDateString();
            }

        }

        private void CustomerNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo && combo.SelectedItem is CustomerClass item)
            {
                var detail = paymentVM.CurrentPayment ?? new();
                detail.CustomerName = item.CustomerName;
                detail.CustomerId = item.CustomerId;
            }
        }

        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var dataGrid = PaymentListDataGrid;
            foreach (var item in dataGrid.Items)
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
            var dataGrid = PaymentListDataGrid;
            foreach (var item in dataGrid.Items)
            {
                var container = dataGrid.ItemContainerGenerator.ContainerFromItem(item);
                if (container is DataGridRow row)
                {
                    row.IsSelected = false; // 修正: 解除
                    var cb = VisualTreeHelperExtensions.FindVisualChildByName<CheckBox>(row, "GridRowCheckBox");
                    if (cb != null) cb.IsChecked = false;
                }
            }
        }
        
        private void ShowAllPayment_Toggled(object sender, RoutedEventArgs e)
        {
            PaymentListFilter();
        }

        private void CreateReceiptButton_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentListDataGrid.SelectedItems.Count > 0)
            {
                // 設定ファイルから出力先ディレクトリを取得
                var outputDir = SettingsManager.Get("OutputDirectory");
                if (string.IsNullOrWhiteSpace(outputDir))
                    outputDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                foreach (var item in PaymentListDataGrid.SelectedItems)
                {
                    if (item is PaymentClass payment)
                    {
                        ReceiptPdfGenerator generator = new();
                        var fileName = FileNameHelper.GenerateReceiptFileName(outputDir, payment);
                        generator.CreateReceiptPdf(payment, fileName);
                    }
                }

            }

        }

        private void PaymentAmountTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Text = textBox.Text.Replace("\\", "").Replace(",", "").Replace("-", "");
                if (string.IsNullOrEmpty(textBox.Text)) textBox.Text = "0";
            }
        }
        
        private void GridRowCheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                var InvoiceItem = checkBox.FindAscendant<DataGridRow>();
                if (InvoiceItem != null)
                {
                    InvoiceItem.IsSelected = checkBox.IsChecked == false;
                }
                e.Handled = true;
            }

        }

        private void InvoiceListDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = InvoiceListDataGrid.SelectedItem;
            if (item == null) return;
            if (item is InvoiceClass selectedItem)
            {
                if (!isEditing)
                {
                    var vm = paymentVM;
                    vm.CurrentPayment = new()
                    {
                        CustomerId = selectedItem.CustomerId,
                        CustomerName = customerVM.CustomerClassList.FirstOrDefault(c => c.CustomerId == selectedItem.CustomerId)!.CustomerName,
                        PaymentAmount = selectedItem.InvoiceTotal ?? 0,
                        Subject = $"{selectedItem.Subject}として",
                    };
                    CustomerNameComboBox.SelectedItem = customerVM.CustomerClassList.FirstOrDefault(c => c.CustomerId == selectedItem.CustomerId);
                }
            }
        }

        private void PaymentForInvoiceSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (InvoiceForPaymentBorder == null) return;
            if (PaymentForInvoiceSwitch.IsOn)
                InvoiceForPaymentBorder.Visibility = Visibility.Visible;
            else
                InvoiceForPaymentBorder.Visibility = Visibility.Collapsed;
            InvoiceForPaymentBorder.IsEnabled = true;
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (sender is ComboBox combobox && combobox.SelectedItem is InvoiceStatusClass item)
            {
                filterParameter.InvoiceStatusId = item.InvoiceStatusId;
                InvoiceListForPaymentFilter();
            }
        }

        private void DateBox_DateSelected(object sender, CalendarDateChangedEventArgs e)
        {
            if (e.AddedDate is DateTime addedDate && e.AddedDate > DateTime.MinValue) 
            {
                paymentVM.FilterDate = addedDate;
                paymentFilterParam.PaymentDate = addedDate;
                PaymentListFilter();
            }
        }

        private void FilterDateBox_DateSelected(object sender, CalendarDateChangedEventArgs e)
        {
            if (e.AddedDate is DateTime addedDate && e.AddedDate > DateTime.MinValue)
            {
                paymentVM.FilterDate = addedDate;
                filterParameter.IssueDate = addedDate;
                InvoiceListForPaymentFilter();
            }
        }

        private void DateFilterSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            InvoiceListForPaymentFilter();
        }

        private void CustomerNameFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is CustomerClass customer)
            {
                filterParameter.CustomerId = customer.CustomerId;
                InvoiceListForPaymentFilter();
            }
        }

        private void FilterClearButton_Click(object sender, RoutedEventArgs e)
        {
            filterParameter = new InvoiceFiterParam();
            InvoiceListForPaymentFilter();
        }

        private void TransactionTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is TransactionTypeClass transactionType && InvoiceForPaymentBorder != null)
            {
                if (transactionType.TransactionName == "売掛金")
                {
                    PaymentForInvoiceSwitch.IsOn = true;
                    InvoiceForPaymentBorder.Visibility = Visibility.Visible;
                }
                else
                {
                    PaymentForInvoiceSwitch.IsOn = false;
                    InvoiceForPaymentBorder.Visibility = Visibility.Collapsed;
                    InvoiceForPaymentBorder.IsEnabled = true;
                }
            }

        }

        private void ReceiptSubject_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textbox)
            {
                //InputMethod.SetPreferredImeConversionMode(textbox, ImeConversionModeValues.Native | ImeConversionModeValues.FullShape);
                InputMethod.SetIsInputMethodEnabled(textbox, true);
                InputMethod.SetPreferredImeState(textbox, InputMethodState.On);
            }
        }

        private void ReceiptSubject_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                InputMethod.SetIsInputMethodEnabled(mainWindow, false);
                InputMethod.SetPreferredImeState(mainWindow, InputMethodState.Off);
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

        // カスタムロジック
        private void PaymentListFilter()
        {
            if (paymentVM == null) return;
            var source = paymentVM.PaymentListViewSource;
            var param = paymentFilterParam;
            var sw = ShowAllPayment.IsOn;
            // フィルタ多重登録防止
            source.Filter -= PaymentListViewSource_Filter;
            source.Filter += PaymentListViewSource_Filter;
            if (isFirstLoading)
            {
                source.SortDescriptions.Clear();
                source.SortDescriptions.Add(new SortDescription(nameof(PaymentClass.PaymentDate), ListSortDirection.Descending));
            }
        }

        private void PaymentListViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is PaymentClass payment)
            {
                var p = paymentFilterParam;
                bool sw = ShowAllPayment.IsOn;
                var conditions = new List<bool>
                {
                    p.PaymentId == null || payment.PaymentId == p.PaymentId,
                    p.SlipNumber == null || payment.SlipNumber == p.SlipNumber,
                    p.CustomerId == null || payment.CustomerId == p.CustomerId,
                    p.InvoiceId == null || payment.InvoiceId == p.InvoiceId,
                    p.TransactionTypeId == null || payment.TransactionTypeId == p.TransactionTypeId,
                    !sw || (MainWindow.StartOfMonth(p.PaymentDate) <= payment.PaymentDate && payment.PaymentDate <= MainWindow.EndOfMonth(p.PaymentDate)),
                    p.PaymentAmount == null || payment.PaymentAmount == p.PaymentAmount,
                    p.Subject == null || payment.Subject == p.Subject
                };
                e.Accepted = conditions.All(c => c);
            }
        }

        private void InvoiceListForPaymentFilter()
        {
            if (paymentVM == null) return;
            var source = paymentVM.InvoiceListForPayment;
            // フィルタ多重登録防止
            source.Filter -= InvoiceListForPayment_Filter;
            source.Filter += InvoiceListForPayment_Filter;
        }

        private void InvoiceListForPayment_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is InvoiceClass invoice)
            {
                var param = filterParameter;
                var sw = DateFilterSwitch;
                var conditions = new List<bool>
                {
                    param.CustomerId == 0 || invoice.CustomerId == param.CustomerId,
                    param.InvoiceStatusId == 0 || invoice.InvoiceStatusId == param.InvoiceStatusId,
                    param.TransactionTypeId == 0 || invoice.TransactionTypeId == param.TransactionTypeId,
                    !sw.IsOn || (MainWindow.StartOfMonth(param.IssueDate) <= invoice.IssueDate && invoice.IssueDate <= MainWindow.EndOfMonth(param.IssueDate)),
                    (param.DueDate == null || invoice.DueDate == param.DueDate),
                    (param.PaymentDate == null || invoice.PaymentDate == param.PaymentDate),
                    (string.IsNullOrWhiteSpace(param.Subject) || invoice.Subject == param.Subject),
                    (param.InvoiceId == 0 || invoice.InvoiceId == param.InvoiceId)
                };
                e.Accepted = conditions.All(c => c);
            }
        }

        private void ShowDatailPane()
        {
            if(!isEditing) ClearPaymentDetailPane();
            Border pane = PaymentDetailPane;
            if (PaymentPageContentsGrid.ActualHeight < pane.Height)
            {
                mainWindow.Height += (pane.Height - PaymentPageContentsGrid.ActualHeight);
            }
            else
            {
                pane.Height = PaymentPageContentsGrid.ActualHeight;
            }

            PaymentPageContentsGrid.IsEnabled = false;
            var renderTransform = PaneTransform;
            var slideUpAnimation = new DoubleAnimation
            {
                From = PaymentDetailPane.Height,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            renderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideUpAnimation);
        }

        private void HideDetailPane()
        {
            if(PaymentDetailPane.RenderTransform is TranslateTransform renderTransform)
            {
                var slideDownAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = PaymentDetailPane.Height,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                renderTransform.BeginAnimation(TranslateTransform.YProperty, slideDownAnimation);
                ClearPaymentDetailPane();
                PaymentPageContentsGrid.IsEnabled = true;
            }
        }

        private void ClearPaymentDetailPane()
        {
            paymentVM.CurrentPayment = new PaymentClass();
            InvoiceListDataGrid.SelectedItem = null;
            CustomerNameComboBox.SelectedIndex = -1;
            CustomerNameComboBox.SelectedItem = null;
            PaymentDate.SelectedDate = DateTime.Today;
        }

        public void AddNewPayment(PaymentClass payment)
        {//新規登録
            var invoice = PaymentForInvoiceSwitch.IsOn ? InvoiceListDataGrid.SelectedItem as InvoiceClass : null;
            payment.InvoiceId = invoice?.InvoiceId ?? null;
            SlipNumberClass slipNumber = slipNumbers.GetSlipNumber(payment.PaymentDate);
            payment.SlipNumber = slipNumber.ReceiptNumber;
            var unitOfWork = new UnitOfWork();

            var result = UnitOfWork.ExecuteWithTransaction(uow =>
            {
                // 入金記録の登録
                if (payment.TryAddPayment(uow))
                { // 入金記録の登録に成功した場合
                    // 伝票番号の最終値を更新
                    slipNumber.InclimentReceiptLatest();
                    // 入金目的が売掛金の場合
                    if (payment.TransactionTypeId == 1 && invoice != null)
                    {
                        // 請求書の状態を「入金済」に更新
                        invoice.InvoiceStatus = "入金済";
                        invoice.InvoiceStatusId = settingsVM.InvoiceStatusClassList.FirstOrDefault(list => list.InvoiceStatus == "入金済")!.InvoiceStatusId;
                        ((InvoiceClass)InvoiceListDataGrid.SelectedItem).UpdateInvoiceStatus(3, uow);
                    }
                    return true;
                }
                return false;
            }, null);

            if(result==true) 
                if(payment.TryUpdatePayment()!=true)
                    throw new Exception("入金記録の登録に失敗しました");
        }

        public void UpdatePayment(PaymentClass payment)
        {
            if (payment == null) return;
            var result = UnitOfWork.ExecuteWithTransaction(uow =>
            {
                var invoice = invoiceVM.InvoiceClassList.FirstOrDefault(i => i.InvoiceId == payment.InvoiceId);
                if(invoice != null && payment.TransactionTypeId == 1)
                {
                    invoice.InvoiceStatus = "入金済";
                    invoice.InvoiceStatusId = 3;
                    invoice.PaymentDate = payment.PaymentDate;
                    ((InvoiceClass)InvoiceListDataGrid.SelectedItem).UpdateInvoiceStatus(3, uow);
                }
                else if (invoice != null && payment.TransactionTypeId == 2)
                {
                    invoice.InvoiceStatus = "請求済";
                    invoice.InvoiceStatusId = 2;
                    invoice.PaymentDate = DateTime.Now;
                    ((InvoiceClass)InvoiceListDataGrid.SelectedItem).UpdateInvoiceStatus(2, uow);
                }

                var previd = prevTransactionTypeId;
                var oldPayment = CurrentPayment;
                if (prevTransactionTypeId == 2)
                {//更新前が前受金の入金情報
                    if (payment.TransactionTypeId == 2)
                    {//更新後が前受金の入金情報
                        // 前受金→前受金
                        DepositClass.TryUpdateDeposit(payment, uow);
                    }
                    else
                    {//更新後が売掛金の入金情報
                        // 前受金→売掛金
                        if (payment.InvoiceId == null)
                        {
                            MessageBox.Show("請求書を選択してください");
                            return false;
                        }
                        // 前受金の削除（T_BALANCEテーブルのレコードも削除される）
                        DepositClass.DeleteDepositById(TypeOfID.Deposit, payment.DepositId, uow);
                        payment.DepositId = null;

                        if (invoice != null)
                        {
                            payment.InvoiceId = invoice.InvoiceId;
                            BalanceClass.TryAddBalance(payment, uow);
                        }
                    }
                }
                else
                {//更新前が売掛金の入金情報
                    if (payment.TransactionTypeId == 2)
                    {
                        // 売掛金→前受金
                        BalanceClass.DeleteBalanceById(new IDs(paymentId : payment.PaymentId), uow);
                        DepositClass.TryAddDeposit(payment, uow);
                        payment.InvoiceId = null;
                    }
                    else
                    {
                        // 売掛金→売掛金
                        // 何もしない
                    }
                }
                CurrentPayment = new PaymentClass();
                return true;
            },null);
            payment.TryUpdatePayment();

        }

    }

}
