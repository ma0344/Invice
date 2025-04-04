using Invoice.Converters;
using Invoice.ViewModels;
using Invoice.ViewModels.Invoice.ViewModels;
using ModernWpf;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
        private CustomerViewModel customerVM;
        private InvoiceViewModel invoiceVM;
        private PaymentViewModel paymentVM;
        private SettingsViewModel settingsVM;
        private readonly DateTime BaseDate = new(2000, 1, 1);
        DateTime TargetDate;
        public CultureInfo cultureInfo = new("ja-JP");
        InvoiceClass CurrentInvoice = new();
        SlipNumberClass SlipNumberInfo = new();
        private bool isEditing = false;
        private PaymentFilterParam paymentFilterParam = new();
        private InvoiceFiterParam filterParameter = new();
        private MainWindow mainWindow;

        public PaymentPage(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            this.DataContext = mainWindowViewModel;
            this.Loaded += PaymentPage_Loaded;
            SlipNumberInfo = SlipNumberClass.GetSlipNumberInfo();
            cultureInfo.DateTimeFormat.Calendar = new JapaneseCalendar();
            cultureInfo.DateTimeFormat.ShortDatePattern = "ggy年M月d日";
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            mainWindow = Application.Current.MainWindow as MainWindow;

            customerVM = mainWindowViewModel.CustomerVM;
            invoiceVM = mainWindowViewModel.InvoiceVM;
            paymentVM = mainWindowViewModel.PaymentVM;
            settingsVM = mainWindowViewModel.SettingsVM;
            paymentVM.PaymentListViewSource = new();
            paymentVM.PaymentListViewSource.Source = paymentVM.PaymentClassList;
            paymentVM.InvoiceListForPayment = new();
            paymentVM.InvoiceListForPayment.Source = invoiceVM.InvoiceClassList;
            DateBox.Value = MonthDiff(BaseDate, DateTime.Now);
            FilterDateBox.Value = MonthDiff(BaseDate, DateTime.Now);
            var converter = (InvoiceIdToSlipNumberConverter)this.Resources["InvoiceIdToSlipNumberConverter"];
            converter.InvoiceClassList = mainWindowViewModel.InvoiceVM.InvoiceClassList;
            //            paymentVM.InvoiceListForPayment.Filter += new FilterEventHandler(InvoiceListForPaymentFilter);
        }
        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label)
            {
                mainWindow?.NavigateToPage(label.Name);
            }
        }
        
        private void PaymentPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!invoiceVM.DepositFromInvoicePage)
            {
                paymentVM.ReloadPaymentList();
                PaymentListFilter();
            }
            else
            {

                AddPaymentButton_Click(AddPaymentButton, new RoutedEventArgs());
                filterParameter = new InvoiceFiterParam();
                filterParameter.InvoiceId = invoiceVM.CurrentInvoice.InvoiceId;
                InvoiceListForPaymentFilter();
                InvoiceListDataGrid.SelectedItem = invoiceVM.CurrentInvoice;

                //InvoiceListDataGrid_SelectionChanged(sender, new SelectionChangedEventArgs(, InvoiceListDataGrid.SelectedItems, new List<InvoiceClass>() { invoiceVM.CurrentInvoice }));
            }

        }

        private void PaymentListFilter()
        {
            if (paymentVM == null) return;
            var source = paymentVM.PaymentListViewSource;
            var param = paymentFilterParam;
            var sw = ShowAllPayment.IsOn;
            source.Filter += (sender, e) =>
            {
                if (e.Item is PaymentClass payment)
                {
                    if ((param.PaymentId == null || payment.PaymentId == param.PaymentId) &&
                        (param.SlipNumber == null || payment.SlipNumber == param.SlipNumber) &&
                        (param.CustomerId == null || payment.CustomerId == param.CustomerId) &&
                        (param.InvoiceId == null || payment.InvoiceId == param.InvoiceId) &&
                        (param.PaymentMethodId == null || payment.PaymentMethodId == param.PaymentMethodId) &&
                        (!sw || (MainWindow.StartOfMonth(param.PaymentDate) <= payment.PaymentDate && payment.PaymentDate <= MainWindow.EndOfMonth(param.PaymentDate))) &&
                        (param.PaymentAmount == null || payment.PaymentAmount == param.PaymentAmount) &&
                        (param.Subject == null || payment.Subject == param.Subject))
                        e.Accepted = true;
                    else
                        e.Accepted = false;
                }
            };
        }

        private void DateBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (this.paymentVM == null) return;
            int newValue = (int)(args.NewValue);
            paymentVM.ViewDate = BaseDate.AddMonths(newValue).ToString("ggy年M月", cultureInfo);
            TargetDate = BaseDate.AddMonths(newValue);
            paymentFilterParam.PaymentDate = TargetDate;
            PaymentListFilter();
        }

        public static int MonthDiff(DateTime FromDate, DateTime ToDate)
        {
            // 月差計算（年差考慮(差分1年 → 12(ヶ月)加算)）
            int iRet = (FromDate.Month + (ToDate.Year - FromDate.Year) * 12) - FromDate.Month + 1;
            return iRet;
        }

        private void ShowDatailPane()
        {
            if(!isEditing) ClearPaymentDetailPane();
            Border pane = PaymentDetailPane;
            if (PaymentPageContentsGrid.ActualHeight < pane.Height)
            {
                var pageHeight = PageRootGrid.ActualHeight;
                var titlebarHeight = mainWindow.ActualHeight - pageHeight;

                mainWindow.Height += (pane.Height - PaymentPageContentsGrid.ActualHeight);
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
            var renderTransform = PaymentDetailPane.RenderTransform as System.Windows.Media.TranslateTransform;
            var slideDownAnimation = new DoubleAnimation
            {
                From = 0,
                To = PaymentDetailPane.Height,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            renderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideDownAnimation);
            ClearPaymentDetailPane();
            PaymentPageContentsGrid.IsEnabled = true;
        }

        private void ClearPaymentDetailPane()
        {
            paymentVM.PaymentDetailData = new PaymentClass();
            InvoiceListDataGrid.SelectedItem = null;
            CustomerNameComboBox.SelectedIndex = -1;
            CustomerNameComboBox.SelectedItem = null;
            PaymentDate.SelectedDate = DateTime.Today;
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

            var payment = PaymentListDataGrid.SelectedItem as PaymentClass;
            var detail = paymentVM.PaymentDetailData;
            detail = payment;
            var customer = customerVM.CustomerClassList.FirstOrDefault(cu => cu.CustomerId == payment.CustomerId);
            CustomerNameComboBox.SelectedItem = customer;
            PaymentMethodComboBox.SelectedItem = settingsVM.PaymentMethodClassList.FirstOrDefault(pm => pm.PaymentMethodId == payment.PaymentMethodId);
            PaymentDate.SelectedDate = payment.PaymentDate;

            if(payment.InvoiceId != null)
            {
                PaymentForInvoiceSwitch.IsOn = true;
                filterParameter = new();
                filterParameter.InvoiceId = payment.InvoiceId ?? 0;
            }
            else
            {
                PaymentForInvoiceSwitch.IsOn = true;
                filterParameter = new();
            }
            DateFilterSwitch.IsOn = false;
            InvoiceListForPaymentFilter();
            ShowDatailPane();

        }

        private void DeletePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentListDataGrid.SelectedItems.Count == 0) return;
            var selectedItemList = PaymentListDataGrid.SelectedItems;
            var paymentList = selectedItemList.Cast<PaymentClass>();
            List<int> idList = paymentList.Select(p => p.PaymentId).ToList();
            foreach(int paymentId in idList)
            {
                var payment = paymentVM.PaymentClassList.FirstOrDefault(p => p.PaymentId == paymentId);
                var invoiceId = payment.InvoiceId;
                if (invoiceId != null)
                {
                    var invoice = invoiceVM.InvoiceClassList.FirstOrDefault(i => i.InvoiceId == invoiceId);
                    if (invoice != null) invoice.InvoiceStatusId = 2;
                }
                if (payment != null) payment.DeletePayment();

            }
            paymentVM.ReloadPaymentList();
            invoiceVM.ReloadInvoiceList();
        }

        private void PaymentCancelButton_Click(object sender, RoutedEventArgs e)
        {
            HideDetailPane();

        }

        private void SavePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentForInvoiceSwitch.IsOn && InvoiceListDataGrid.SelectedItem == null)
            {
                MessageBox.Show("支払対象の請求書を選択してください。");
                return;
            }
            if (PaymentMethodComboBox.SelectedIndex < 0 || !(PaymentMethodComboBox.SelectedItem is PaymentMethodClass))
            {
                MessageBox.Show("支払方法を選択してください。");
                return;
            }
            if (CustomerNameComboBox.SelectedIndex <= 0)
            {
                MessageBox.Show("宛先を選択してください");
                return;
            }
            if (!PaymentForInvoiceSwitch.IsOn)
            {
                var customer = CustomerNameComboBox.SelectedItem as CustomerClass;
                paymentVM.PaymentDetailData.CustomerId = customer.CustomerId;
                paymentVM.PaymentDetailData.CustomerName = customer.CustomerName;
            }

            var payment = paymentVM.PaymentDetailData;
            payment.PaymentDate = PaymentDate.DisplayDate;
            payment.PaymentMethodId = ((PaymentMethodClass)PaymentMethodComboBox.SelectedItem).PaymentMethodId;           

            try
            {
                InvoiceClass invoice;
                if (!isEditing)
                {//新規登録
                    if (PaymentForInvoiceSwitch.IsOn && InvoiceListDataGrid.SelectedItem != null)
                    {
                        invoice = InvoiceListDataGrid.SelectedItem as InvoiceClass;
                        payment.InvoiceId = invoice.InvoiceId;
                        invoice.InvoiceStatus = "入金済";
                        invoice.InvoiceStatusId = settingsVM.InvoiceStatusClassList.FirstOrDefault(list => list.InvoiceStatus == "入金済" ).InvoiceStatusId;
                    }
                    else
                        payment.InvoiceId = null;
                    var prefix = string.IsNullOrWhiteSpace(SlipNumberInfo.ReceiptPrefix) ? "" : SlipNumberInfo.ReceiptPrefix;
                    var suffix = string.IsNullOrWhiteSpace(SlipNumberInfo.ReceiptSuffix) ? "" : SlipNumberInfo.ReceiptSuffix;
                    prefix += payment.PaymentDate.ToString("yyMM_");
                    var numberString = (SlipNumberInfo.ReceiptLatest + 1).ToString("0000");
                    var slipNumber = $"{prefix}{numberString}{suffix}";
                    payment.SlipNumber = slipNumber;

                    if (payment.TryAddPayment())
                    {
                        SlipNumberInfo.InclimentReceiptLatest();
                        if (PaymentForInvoiceSwitch.IsOn)
                        {
                            ((InvoiceClass)InvoiceListDataGrid.SelectedItem).UpdateInvoiceStatus(3);
                        }

                        mainWindow.SavedInfomation();
                    }
                    else throw new Exception("入金記録の登録に失敗しました");
                }
                else
                {//更新
                    if (!payment.TryUpdatePayment())
                    {
                        throw new Exception("入金記録の更新に失敗しました");
                    }
                }
                HideDetailPane();
                paymentVM.ReloadPaymentList();
                invoiceVM.ReloadInvoiceList();
                if (invoiceVM.DepositFromInvoicePage)
                {
                    invoiceVM.DepositFromInvoicePage = false;
                    mainWindow.MainFrame.Navigate(mainWindow.InvoicePage);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void PaymentDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var datePicker = sender as DatePicker;
            if (datePicker.SelectedDate != null && paymentVM != null)
            {
                var paymentDate = (DateTime)datePicker.SelectedDate;
                paymentVM.PaymentDetailData.PaymentDateString = paymentDate.ToShortDateString();
            }

        }

        private void CustomerNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            var detail = paymentVM.PaymentDetailData;
            var item = combo.SelectedItem as CustomerClass;
            if (item == null) return;
            detail.CustomerName = item.CustomerName;
            detail.CustomerId = item.CustomerId;
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
                    VisualTreeHelperExtensions.FindVisualChildByName<CheckBox>(row, "GridRowCheckBox").IsChecked = true;
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

                    row.IsSelected = true;
                    VisualTreeHelperExtensions.FindVisualChildByName<CheckBox>(row, "GridRowCheckBox").IsChecked = false;
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
                foreach (var item in PaymentListDataGrid.SelectedItems)
                {
                    if (item is PaymentClass payment)
                    {
                        ReceiptPdfGenerator generator = new();
                        var fileName = FileNameHelper.GenerateReceiptFileName(@"c:\users\ma\desktop", payment);
                        generator.CreateReceiptPdf(payment, fileName);
                    }
                }

            }

        }

        private void GridRowCheckBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var InvoiceItem = checkBox.FindAscendant<DataGridRow>();
            if (InvoiceItem != null)
            {
                InvoiceItem.IsSelected = checkBox.IsChecked == false;
            }
            e.Handled = true;
        }

        private void InvoiceListDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = InvoiceListDataGrid.SelectedItem;
            if (item == null) return;
            if (item is InvoiceClass selectedItem)
            {
                var vm = paymentVM;
                vm.PaymentDetailData = new();
                vm.PaymentDetailData.CustomerId = selectedItem.CustomerId;
                vm.PaymentDetailData.CustomerName = customerVM.CustomerClassList.FirstOrDefault(c => c.CustomerId == selectedItem.CustomerId).CustomerName;
                CustomerNameComboBox.SelectedItem = customerVM.CustomerClassList.FirstOrDefault(c => c.CustomerId == selectedItem.CustomerId);
                vm.PaymentDetailData.PaymentAmount = selectedItem.Total;
                vm.PaymentDetailData.Subject = $"{selectedItem.Subject}として";
            }
        }

        private void InvoiceListForPaymentFilter()
        {
            if (paymentVM == null) return;
            var source = paymentVM.InvoiceListForPayment;
            var param = filterParameter;
            var sw = DateFilterSwitch;
            source.Filter += (sender, e) =>
            {
                if (e.Item is InvoiceClass invoice)
                {
                    if ((param.CustomerId == 0 || invoice.CustomerId == param.CustomerId) &&
                        (param.InvoiceStatusId == 0 || invoice.InvoiceStatusId == param.InvoiceStatusId) &&
                        (param.TransactionTypeId == 0 || invoice.TransactionTypeId == param.TransactionTypeId) &&
                        (sw.IsOn == false ||
                        (MainWindow.StartOfMonth(param.IssueDate) <= invoice.IssueDate && 
                        invoice.IssueDate <= MainWindow.EndOfMonth(param.IssueDate))) &&
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
            
            var combobox = sender as ComboBox;
            if (combobox.SelectedItem is InvoiceStatusClass item)
            {
                filterParameter.InvoiceStatusId = item.InvoiceStatusId;
                InvoiceListForPaymentFilter();
            }
        }

        private void FilterDateBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (this.paymentVM == null) return;
            int newValue = (int)(args.NewValue);
            paymentVM.InvoiceViewDate = BaseDate.AddMonths(newValue).ToString("ggy年M月", cultureInfo);
            TargetDate = BaseDate.AddMonths(newValue);
            filterParameter.IssueDate = TargetDate;
            InvoiceListForPaymentFilter();

        }

        private void DateFilterSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            InvoiceListForPaymentFilter();
        }

        private void CustomerNameFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var customer = comboBox.SelectedItem as CustomerClass;
            filterParameter.CustomerId = customer.CustomerId;
            InvoiceListForPaymentFilter();
        }

        private void FilterClearButton_Click(object sender, RoutedEventArgs e)
        {
            filterParameter = new InvoiceFiterParam();
            InvoiceListForPaymentFilter();
        }
    }

}
