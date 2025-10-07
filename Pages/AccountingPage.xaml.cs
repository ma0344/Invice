using Invoice.Accounting;
using Invoice.Classes;
using Invoice.Pages;
using Invoice.ViewModels;
using Invoice.ViewModels.Invoice.ViewModels;
using ModernWpf;
using ModernWpf.Controls;
using PdfSharp.Pdf.Filters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
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

namespace Invoice
{
    /// <summary>
    /// AccountingPage.xaml の相互作用ロジック
    /// </summary>
    public partial class AccountingPage : System.Windows.Controls.Page
    {
        private InvoiceFiterParam filterParam = new();
        private AccountingDataClass accounting = new();
        private InvoiceViewModel InvoiceVM;
        private SettingsViewModel SettingsVM;
        private bool isFirstLoading = true;
        private bool DebugSwitch = false;
        public CollectionViewSource InvoiceViewSource { get; set; }
        private MainWindow mainWindow;
        public AccountingPage()
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;
            MainWindowViewModel mainWindowViewModel = (MainWindowViewModel)mainWindow.DataContext;
            DebugSwitch = mainWindowViewModel.DebugOutIsOn;
            InvoiceVM = mainWindowViewModel.InvoiceVM;
            SettingsVM = mainWindowViewModel.SettingsVM;
            InvoiceViewSource = new();
            InvoiceViewSource.Source = InvoiceVM.InvoiceClassList;
            this.DataContext = this;
            this.Loaded += AccountingPage_Loaded;
        }
        private void InvoiceFilter(InvoiceFiterParam param)
        {
            var source = InvoiceViewSource;
            source.Filter += (sender, e) =>
            {
                if (e.Item is InvoiceClass invoice)
                {
                    if (!ShowAllInvoice.IsOn || (invoice.IssueDate.Value.Year == param.IssueDate.Value.Year && invoice.IssueDate.Value.Month == param.IssueDate.Value.Month))
                        e.Accepted = true;
                    else
                        e.Accepted = false;
                }

            };
            if (isFirstLoading)
            {
                source.SortDescriptions.Clear();
                source.SortDescriptions.Add(new SortDescription(nameof(InvoiceClass.IssueDate), ListSortDirection.Descending));
            }
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

        private void AccountingPage_Loaded(object sender, RoutedEventArgs e)
        {
            InvoiceFilter(filterParam);
            isFirstLoading = false;
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.NavigateToPage(label.Name);
            }
        }

        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var dataGrid = InvoiceListDataGrid;
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
            var dataGrid = InvoiceListDataGrid;
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

        private void ShowAllInvoice_Toggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            if (!toggleSwitch!.IsOn)
                filterParam.IssueDate = null;
            else
                filterParam.IssueDate = DateBox.SelectedDate;
            InvoiceFilter(filterParam);
        }

        private void DateBox_DateSelected(object sender, CalendarDateChangedEventArgs e)
        {
            filterParam.IssueDate = DateBox.SelectedDate;
            InvoiceFilter(filterParam);
        }

        private void InvoiceListDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource;
            if (originalSource is ScrollViewer)
            {
                SelectAllCheckBox_Unchecked(SelectAllCheckBox, new RoutedEventArgs());
            }
        }

        private void CreateCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = SetInvoiceItemsToInvoice();
            string outputFilePath = accounting.CreateCsv(MakeType.file, DateBox.SelectedDate, selectedItems, OutputDirTextBox.Text);
            if (outputFilePath != string.Empty)
            {
                Clipboard.Clear();
                Clipboard.SetText(outputFilePath);
                MessageBox.Show("CSVファイルを作成しました。", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("CSVファイルの作成に失敗しました。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PreviewCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = SetInvoiceItemsToInvoice();

            ShowPreviewPane();
            var csvString = accounting.CreateCsv(MakeType.prev, DateBox.SelectedDate, selectedItems);
            CsvPreviewTextBox.Text = csvString;
        }

        private List<InvoiceClass> SetInvoiceItemsToInvoice()
        {
            var selectedItems = InvoiceListDataGrid.SelectedItems.OfType<InvoiceClass>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("請求書が選択されていません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
            //selectedItems.ForEach
            //    (
            //    item =>
            //    {
            //        item.InvoiceItems.Clear();
            //        item.InvoiceItems = new ObservableCollection<InvoiceItemClass>(GetSelectedInvoiceItems(item.InvoiceId));
            //    }
            //    );
            return selectedItems;
        }

        private List<InvoiceItemClass> GetSelectedInvoiceItems(int invoiceId)
        {
            var itemsList = new List<InvoiceItemClass>();
            var items = InvoiceItemClass.GetInvoiceItemsByInvoiceId(invoiceId);
            items.ForEach(item => item.ItemCode = SettingsVM.ItemClassList.FirstOrDefault(x => x.ItemId == item.ItemId)?.ItemCode ?? "");
            return items;
        }

        private void OutputDirSelectButton_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new Microsoft.Win32.OpenFolderDialog()
            {
                // ダイアログの設定（任意）

                Title = "フォルダを選択してください",

                DefaultDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                RootDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            };
            // ダイアログを表示
            if (folderBrowserDialog.ShowDialog() ?? false)
            {
                // 選択されたディレクトリのパス
                OutputDirTextBox.Text = folderBrowserDialog.FolderName;
            }
        }

        private void ClosePreviewPaneButton_Click(object sender, RoutedEventArgs e)
        {
            HidePreviewPane();
        }

        private void ShowPreviewPane()
        {
            var pane = AccountingCsvPreviewPane;

            if (AccountingContentsGrid.ActualHeight < pane.Height)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.Height += pane.Height - AccountingContentsGrid.ActualHeight;
            }
            else
            {
                pane.Height = AccountingContentsGrid.ActualHeight;
            }

            AccountingContentsGrid.IsEnabled = false;
            var renderTransform = PaneTransform;
            var slideUpAnimation = new DoubleAnimation
            {
                From = pane.Height,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            renderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideUpAnimation);
        }
        private void HidePreviewPane()
        {
            var pane = AccountingCsvPreviewPane;
            var renderTransform = AccountingCsvPreviewPane.RenderTransform as System.Windows.Media.TranslateTransform;
            var slideDownAnimation = new DoubleAnimation
            {
                From = 0,
                To = pane.Height,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            renderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideDownAnimation);
            AccountingContentsGrid.IsEnabled = true;
        }

    }
}
