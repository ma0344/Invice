using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Animation;
using Invoice.ViewModels.Invoice.ViewModels;
using Invoice.ViewModels;
using ModernWpf;
using Invoice.Converters;
using MigraDoc.DocumentObjectModel;

namespace Invoice
{
    public partial class SettingsPage : Page, INotifyPropertyChanged
    {
        private bool isEditing = false;
        private SettingsViewModel vm;
        public SettingsPage(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            DataContext = mainWindowViewModel;
            vm = mainWindowViewModel.SettingsVM;
            this.Loaded += SettingsPage_Loaded;
        }

        private void TaxRateTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if(textBox.DataContext is TaxTypeClass taxItem)
            {
                var value = textBox.Text;
                double taxRate;
                if (value is string text)
                {
                    double rate;
                    if (text.IsValueNullOrEmpty()) taxRate = 0;
                    else if (text.EndsWith("%")) taxRate = double.Parse(text.Replace("%", ""));
                    else if (double.TryParse(text, out rate))
                    {
                        taxRate = rate / 100;
                    }
                    else taxRate = 0;

                    taxItem.TaxRate = taxRate / 100;
                }
            }
        }

        private void TaxRateTextBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void TaxRateTextBox_TargetUpdated(object? sender, DataTransferEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            var scrollViewer = dataGrid.Template.FindName("DG_ScrollViewer", dataGrid) as ScrollViewer;
            if (scrollViewer != null)
            {
                var verticalScrollBar = scrollViewer.Template.FindName("PART_VerticalScrollBar", scrollViewer) as ScrollBar;
                if (verticalScrollBar != null)
                {
                    // ここで verticalScrollBar を使用できます
                    verticalScrollBar.Visibility = Visibility.Collapsed;
                    dataGrid.Loaded -= DataGrid_Loaded;
                }
            }
            var t = dataGrid.Template.FindName("DG_ScrollViewer", dataGrid);
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.TaxListDataGrid.Loaded += DataGrid_Loaded;
            this.PaymentMethodListDataGrid.Loaded += DataGrid_Loaded;
            this.TransactionMethodListDataGrid.Loaded += DataGrid_Loaded;
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label label)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.NavigateToPage(label.Name);
            }
        }
        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            var dataGrid = ItemListDataGrid;
            if (dataGrid.SelectedItems.Count != 0) dataGrid.SelectedIndex = -1;
            isEditing = false;
            vm.SaveButtonText = "保存";
            vm.PaneTitle = "新規項目登録";
            vm.SelectedItem = new ItemClass();
            ShowDatailPane();
        }

        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            var dataGrid = ItemListDataGrid;
            if (dataGrid.SelectedItems.Count == 0) return;
            if (dataGrid.SelectedItem is ItemClass selectedItem)
            {
                selectedItem.DeleteItem();
                vm.ReloadItems();
            }
            else
            {
                MessageBox.Show("削除する項目を選択してください。");
            }
        }

        private void EditItemButton_Click(object sender, RoutedEventArgs e)
        {
            var dataGrid = ItemListDataGrid;
            if (dataGrid.SelectedItems.Count == 0) return;
            if (dataGrid.SelectedItem is ItemClass selectedItem)
            {
                isEditing = true;
                vm.SaveButtonText = "更新";
                vm.PaneTitle = "項目情報編集";
                vm.SelectedItem = selectedItem.Copy();
                ShowDatailPane();
            }
            else
            {
                MessageBox.Show("編集する項目を選択してください。");
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

        private void ItemListDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditItemButton_Click(EditItemButton, new RoutedEventArgs());
        }


        private void ShowDatailPane()
        {
            ItemPageContentsGrid.IsEnabled = false;
            var renderTransform = ItemDetailPane.RenderTransform as System.Windows.Media.TranslateTransform;
            var slideUpAnimation = new DoubleAnimation
            {
                From = 300,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            renderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideUpAnimation);
        }

        private void HideDetailPane()
        {
            var renderTransform = ItemDetailPane.RenderTransform as System.Windows.Media.TranslateTransform;
            var slideDownAnimation = new DoubleAnimation
            {
                From = 0,
                To = 300,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            renderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, slideDownAnimation);
            ItemPageContentsGrid.IsEnabled = true;
            isEditing = false;
        }

        private void SaveItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isEditing) vm.SelectedItem.AddItem(); else vm.SelectedItem.UpdateItem();
            isEditing = false;
            HideDetailPane();
            vm.ReloadItems();
        }

        private void CancelItemButton_Click(object sender, RoutedEventArgs e)
        {
            HideDetailPane();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;
            T foundChild = null;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (!(child is T t))
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                    {
                        foundChild = t;
                        break;
                    }
                }
                else
                {
                    foundChild = t;
                    break;
                }
            }
            return foundChild;
        }

        private void DeleteTaxButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItemIds = TaxListDataGrid.SelectedItems
                .OfType<TaxTypeClass>()
                .Select(tax => tax.TaxTypeId)
                .ToList();
            foreach (var id in selectedItemIds)
            {
                var taxTypeItem =vm.TaxTypeClassList.FirstOrDefault(x => x.TaxTypeId == id);
                if (taxTypeItem != null)
                {
                    taxTypeItem.DeleteTaxType();
                    vm.TaxTypeClassList.Remove(taxTypeItem);
                }
            }
        }

        private void ApplyTaxButton_Click(object sender, RoutedEventArgs e)
        {
            var orgTaxList = TaxTypeClass.GetTaxes();
            var newTaxList = vm.TaxTypeClassList;
            foreach (var tax in newTaxList)
            {
                if (tax.TaxTypeId != 0)
                {
                    if (orgTaxList.FirstOrDefault(x => x.TaxTypeId == tax.TaxTypeId) is TaxTypeClass orgTax)
                    {
                        if(orgTax.TaxTypeName != tax.TaxTypeName || orgTax.TaxRate != tax.TaxRate)
                            tax.UpdateTaxType();
                    }
                }
                else
                    tax.TaxTypeId = TaxTypeClass.AddTaxType(tax);
            }
        }

        private void ApplySlipNumberButton_Click(object sender, RoutedEventArgs e)
        {
            vm.SlipNumberInfo.UpdateSlipNumberInfo();
        }

        private void ApplyPaymentTitleButton_Click(object sender, RoutedEventArgs e)
        {
            var orgPaymentList = PaymentMethodClass.GetPaymentMethods();
            var newPaymentList = vm.PaymentMethodClassList;
            foreach (var payment in newPaymentList)
            {
                if (payment.PaymentMethodId != 0)
                {
                    if (orgPaymentList.FirstOrDefault(x => x.PaymentMethodId == payment.PaymentMethodId) is PaymentMethodClass orgPayment)
                    {
                        if (orgPayment.MethodName != payment.MethodName)
                            payment.UpdatePaymentMethod();
                    }
                }
                else
                    payment.PaymentMethodId = PaymentMethodClass.AddPaymentMethod(payment);
            }
        }

        private void DeletePaymentTitleButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItemIds = PaymentMethodListDataGrid.SelectedItems
                .OfType<PaymentMethodClass>()
                .Select(payment => payment.PaymentMethodId)
                .ToList();
            foreach (var id in selectedItemIds)
            {
                var paymentMethodItem = vm.PaymentMethodClassList.FirstOrDefault(x => x.PaymentMethodId == id);
                if (paymentMethodItem != null)
                {
                    paymentMethodItem.DeletePaymentMethod();
                    vm.PaymentMethodClassList.Remove(paymentMethodItem);
                }
            }
        }

        private void ApplyTransactionTitleButton_Click(object sender, RoutedEventArgs e)
        {
            var orgTransactionList = TransactionTypeClass.GetTransactionTypes();
            var newTransactionList = vm.TransactionTypeClassList;
            foreach (var transaction in newTransactionList)
            {
                if (transaction.TransactionTypeId != 0)
                {
                    if (orgTransactionList.FirstOrDefault(x => x.TransactionTypeId == transaction.TransactionTypeId) is TransactionTypeClass orgTransaction)
                    {
                        if (orgTransaction.TransactionName != transaction.TransactionName)
                            transaction.UpdateTransactionType();
                    }
                }
                else
                    transaction.TransactionTypeId = TransactionTypeClass.AddTransactionType(transaction);
            }
        }

        private void DeleteTransactionTitleButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItemIds = TransactionMethodListDataGrid.SelectedItems
                .OfType<TransactionTypeClass>()
                .Select(transaction => transaction.TransactionTypeId)
                .ToList();
            foreach (var id in selectedItemIds)
            {
                var transactionMethodItem = vm.TransactionTypeClassList.FirstOrDefault(x => x.TransactionTypeId == id);
                if (transactionMethodItem != null)
                {
                    transactionMethodItem.DeleteTransactionType();
                    vm.TransactionTypeClassList.Remove(transactionMethodItem);
                }
            }
        }

        private void TaxRateTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Text = textBox.Text.Replace("%", "");
            if (string.IsNullOrEmpty(textBox.Text)) textBox.Text = "0";
            textBox.Text = (double.Parse(textBox.Text)/100).ToString();
        }

        private void TaxListDataGrid_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {

        }

        private void TaxListDataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {

        }

        private void TaxListDataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (e.Column.Header.ToString() == "税率")
            {
                if (e.EditingElement is TextBox textBox)
                {
                    textBox.Text = textBox.Text.Replace("%", "");
                    if (string.IsNullOrEmpty(textBox.Text)) textBox.Text = "0";
                    textBox.Text = (double.Parse(textBox.Text) / 100).ToString();
                }
            }
        }
    }
    public static class VisualTreeHelperWrapper
    {
        public static T FindDescendant<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) { return null; }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? FindDescendant<T>(child);
                if (result != null) { return result; }
            }
            return null;
        }
    }
}
