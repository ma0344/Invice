using Invoice.ViewModels;
using Invoice.ViewModels.Invoice.ViewModels;
using ModernWpf;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Invoice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Page DashBoardPage { get; set; }
        public Page InvoicePage { get; set; }
        public Page CustomerPage { get; set; }
        public Page Payment { get; set; }
        public Page SettingsPage { get; set; }

        public MainWindowViewModel MainWindowViewModel { get; set; }

        public ObservableCollection<CultureInfo> CustomersList;
        public ObservableCollection<ItemClass> ItemsList;
        public List<TaxTypeClass> TaxList;

        public MainWindow()
        {
            InitializeComponent();

            MainWindowViewModel = new MainWindowViewModel();
            this.DataContext = MainWindowViewModel;
            DashBoardPage = new DashBoard();
            InvoicePage = new InvoicePage(MainWindowViewModel);
            CustomerPage = new CustomerPage(MainWindowViewModel);
            Payment = new PaymentPage(MainWindowViewModel);
            SettingsPage = new SettingsPage(MainWindowViewModel);
            MainFrame.Navigate(DashBoardPage);
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            TaxList = TaxTypeClass.GetTaxes();
        }

        public void NavigateToPage(string pageName)
        {
            switch (pageName)
            {
                case "DashBoardLabel":
                    MainFrame.Navigate(DashBoardPage);
                    break;
                case "CustomerLabel":
                    MainFrame.Navigate(CustomerPage);
                    break;
                case "InvoiceLabel":
                    MainFrame.Navigate(InvoicePage);
                    break;
                case "PaymentLabel":
                    MainFrame.Navigate(Payment);
                    break;
                case "SettingsLabel":
                    MainFrame.Navigate(SettingsPage);
                    break;
                default:
                    break;
            }
        }

        public void SavedInfomation()
        {
            
            MessageBox.Show(
                owner: this,
                messageBoxText: "保存しました",
                caption: "保存完了",
                button: MessageBoxButton.OK,
                icon: MessageBoxImage.Information
                );

        }

        public static DateTime StartOfMonth(DateTime? date)
        {
            if (date.HasValue) return new DateTime(date.Value.Year, date.Value.Month, 1);
            else
                return DateTime.MinValue;

        }

        public static DateTime EndOfMonth(DateTime? date)
        {
            if (date.HasValue) return new DateTime(date.Value.Year, date.Value.Month, DateTime.DaysInMonth(date.Value.Year, date.Value.Month));
            else
                return DateTime.MinValue;
        }

    }


}