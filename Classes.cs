using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data.SqlClient;
using MySqlConnector;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Invoice.ViewModels;
using System.Windows.Data;
using System.Windows.Documents;
using System.Data;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel.Design;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace Invoice
{

    // T_BALANCE テーブルに対応するクラス
    public class BalanceClass
    {
        public int BalanceId { get; set; }
        public int CustomerId { get; set; }
        public int InvoiceId { get; set; }
        public int PaymentId { get; set; }
        public string SlipNumber { get; set; }
        public int DebOrCreId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int TransactionTypeId { get; set; }
        public int TransactionAmount { get; set; }

        // データベースから全てのレコードを取得
        public static List<BalanceClass> GetAllBalances()
        {
            var balances = new List<BalanceClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_BALANCE", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var balance = new BalanceClass
                {
                    BalanceId = reader.GetInt32("BALANCE_ID"),
                    CustomerId = reader.GetInt32("CUSTOMER_ID"),
                    InvoiceId = reader.IsDBNull("INVOICE_ID") ? 0 : reader.GetInt32("INVOICE_ID"),
                    PaymentId = reader.IsDBNull("PAYMENT_ID") ? 0 : reader.GetInt32("PAYMENT_ID"),
                    SlipNumber = reader.IsDBNull("SLIP_NUMBER") ? "" : reader.GetString("SLIP_NUMBER"),
                    DebOrCreId = reader.GetInt32("DEBIT_OR_CREDIT_ID"),
                    TransactionDate = reader.GetDateTime("TRANSACTION_DATE"),
                    TransactionTypeId = reader.GetInt32("TRANSACTION_TYPE_ID"),
                    TransactionAmount = reader.GetInt32("TRANSACTION_AMOUNT")
                };
                balances.Add(balance);
            }
            return balances;
        }

        public static List<BalanceClass> GetBalancesByCustomerId(int customerId)
        {
            var balances = new List<BalanceClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "SELECT * FROM T_BALANCE WHERE CUSTOMER_ID = @CustomerId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", customerId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var balance = new BalanceClass
                {
                    BalanceId = reader.GetInt32("BALANCE_ID"),
                    CustomerId = reader.GetInt32("CUSTOMER_ID"),
                    InvoiceId = reader.GetInt32("INVOICE_ID"),
                    DebOrCreId = reader.GetInt32("DEBIT_OR_CREDIT_ID"),
                    TransactionDate = reader.GetDateTime("TRANSACTION_DATE"),
                    TransactionTypeId = reader.GetInt32("TRANSACTION_TYPE_ID"),
                    TransactionAmount = reader.GetInt32("TRANSACTION_AMOUNT")
                };
                balances.Add(balance);
            }
            return balances;
        }

        // 新しいレコードを追加
        public void AddBalance()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"INSERT INTO T_BALANCE (CUSTOMER_ID, INVOICE_ID, SLIP_NUMBER, DEBIT_OR_CREDIT_ID, TRANSACTION_DATE, TRANSACTION_TYPE_ID, TRANSACTION_AMOUNT)
                             VALUES (@CustomerId, @InvoiceId, @SlipNumber, @DebOrCreId, @TransactionDate, @TransactionTypeId, @TransactionAmount)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@DebOrCreId", DebOrCreId);
            command.Parameters.AddWithValue("@SlipNumber", SlipNumber);
            command.Parameters.AddWithValue("@TransactionDate", TransactionDate);
            command.Parameters.AddWithValue("@TransactionTypeId", TransactionTypeId);
            command.Parameters.AddWithValue("@TransactionAmount", TransactionAmount);
            command.ExecuteNonQuery();
            BalanceId = (int)command.LastInsertedId;
        }

        // レコードを更新
        public void UpdateBalance()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"UPDATE T_BALANCE SET CUSTOMER_ID = @CustomerId, INVOICE_ID = @InvoiceId, DEBIT_OR_CREDIT_ID = DebOrCreId, TRANSACTION_DATE = @TransactionDate, TRANSACTION_TYPE_ID = @TransactionTypeId, TRANSACTION_AMOUNT = @TransactionAmount WHERE BALANCE_ID = @BalanceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@DebOrCreId", DebOrCreId);
            command.Parameters.AddWithValue("@TransactionDate", TransactionDate);
            command.Parameters.AddWithValue("@TransactionTypeId", TransactionTypeId);
            command.Parameters.AddWithValue("@TransactionAmount", TransactionAmount);
            command.Parameters.AddWithValue("@BalanceId", BalanceId);
            command.ExecuteNonQuery();
        }

        public void DeleteBalanceById(int id)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_BALANCE WHERE BALANCE_ID = @BalanceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@BalanceId", id);
            command.ExecuteNonQuery();
        }

        public void DeleteBalanceByInvoiceId(int invoiceId)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_BALANCE WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", invoiceId);
            command.ExecuteNonQuery();
        }

        // レコードを削除
        public void DeleteBalance()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_BALANCE WHERE BALANCE_ID = @BalanceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@BalanceId", BalanceId);
            command.ExecuteNonQuery();
        }
    }

    // T_CUSTOMER テーブルに対応するクラス
    public class CustomerClass : INotifyPropertyChanged
    {
        public int CustomerId { get; set; } = 0;

        private string _customerName = "";
        public string CustomerName
        {
            get => _customerName;
            set
            {
                _customerName = value;
                OnPropertyChanged(nameof(CustomerName));
            }
        }

        private string _customerKana = "";
        public string CustomerKana
        {
            get => _customerKana;
            set
            {
                _customerKana = value;
                OnPropertyChanged(nameof(CustomerKana));
            }
        }
        private int _customerBalance = 0;
        public int CustomerBalance
        {
            get => _customerBalance;
            set
            {
                _customerBalance = value;
                OnPropertyChanged(nameof(CustomerBalance));
            }
        }


        private bool _customerVisible = true;
        public bool CustomerVisible
        {
            get => _customerVisible;
            set
            {
                if (_customerVisible != value)
                {
                    _customerVisible = value;
                    OnPropertyChanged(nameof(CustomerVisible));
                }
            }
        }

        public static List<CustomerClass> GetCustomers()
        {
            var commandString = "SELECT * FROM T_CUSTOMER";
            var customers = new List<CustomerClass>();
            string connenctionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connenctionString);
            connection.Open();
            using var command = new MySqlCommand(commandString, connection);
            using var reader = command.ExecuteReader();
            customers.Add(new CustomerClass());
            while (reader.Read())
            {
                var customer = new CustomerClass();
                customer.CustomerId = reader.GetInt32("CUSTOMER_ID");
                customer.CustomerName = reader.GetString("CUSTOMER_NAME");
                customer.CustomerKana = reader.GetString("CUSTOMER_KANA");
                customer.CustomerBalance = reader.GetInt32("BALANCE");
                customer.CustomerVisible = reader.GetBoolean("VISIBLE");
                customers.Add(customer);
            }
            return customers;

        }
        public void UpdateCustomerInDatabase()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("UPDATE T_CUSTOMER SET CUSTOMER_NAME=@name, CUSTOMER_KANA=@kana, BALANCE=@balance, VISIBLE=@visible WHERE CUSTOMER_ID=@id", connection);
            command.Parameters.AddWithValue("@name", CustomerName);
            command.Parameters.AddWithValue("@kana", CustomerKana);
            command.Parameters.AddWithValue("@balance", CustomerBalance);
            command.Parameters.Add("@visible", MySqlDbType.Bit).Value = CustomerVisible;
            command.Parameters.AddWithValue("@id", CustomerId);
            command.ExecuteNonQuery();
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddCustomerInDatabase()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("INSERT INTO T_CUSTOMER (CUSTOMER_NAME, CUSTOMER_KANA, BALANCE, VISIBLE) VALUES (@name, @kana, @balance, @visible)", connection);
            command.Parameters.AddWithValue("@name", CustomerName);
            command.Parameters.AddWithValue("@kana", CustomerKana);
            command.Parameters.AddWithValue("@balance", CustomerBalance);
            command.Parameters.Add("@visible", MySqlDbType.Bit).Value = true;
            command.ExecuteNonQuery();
        }
    }

    public class CustomerFilterParam
    {
        public int? CustomerId { get; set; } = null;
        public string? CustomerName { get; set; } = null;
        public string? CustomerKana { get; set; } = null;
        public int? CustomerBalance { get; set; } = null;
        public bool? CustomerVisible { get; set; } = null;
    }
    // T_DEBIT_OR_CREDIT テーブルに対応するクラス
    public class DebitOrCreditClass
    {
        public int DebitOrCreditId { get; set; }
        public string DebitOrCreditName { get; set; }

        public static List<DebitOrCreditClass> GetAllDebitOrCredits()
        {
            var list = new List<DebitOrCreditClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_DEBIT_OR_CREDIT", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new DebitOrCreditClass
                {
                    DebitOrCreditId = reader.GetInt32("DEBIT_OR_CREDIT_ID"),
                    DebitOrCreditName = reader.GetString("DEBIT_OR_CREDIT")
                };
                list.Add(item);
            }
            return list;
        }
    }

    // T_INVOICE テーブルに対応するクラス
    public class InvoiceClass : INotifyPropertyChanged
    {

        public int InvoiceId { get; set; } = 0;
        public int CustomerId { get; set; } = 0;
        public DateTime IssueDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; } = DateTime.Now;
        public string Subject { get; set; } = "";
        public string SlipNumber { get; set; } = "";
        public int SubTotal { get; set; } = 0;
        public int Tax { get; set; } = 0;
        public int Total { get; set; } = 0;
        public string Message { get; set; } = "";
        public int TransactionTypeId { get; set; } = 1;
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public int InvoiceStatusId { get; set; } = 1;
        public string CustomerName { get; set; } = "";
        public string InvoiceStatus { get; set; } = "";
        public string IssueDateString { get; set; } = "";
        public List<InvoiceItemClass> InvoiceItems { get; set; } = new List<InvoiceItemClass>();

        public static List<InvoiceClass> GetAllInvoice()
        {
            var invoices = new List<InvoiceClass>();
            string connenctionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connenctionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_INVOICE", connection);
            using var reader = command.ExecuteReader();

            CultureInfo cultureInfo = new("ja-JP");
            cultureInfo.DateTimeFormat.Calendar = new JapaneseCalendar();
            cultureInfo.DateTimeFormat.ShortDatePattern = "ggy年M月d日";
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            while (reader.Read())
            {
                var invoice = new InvoiceClass();
                invoice.InvoiceId = reader.GetInt32("INVOICE_ID");
                invoice.CustomerId = reader.GetInt32("CUSTOMER_ID");
                invoice.IssueDate = reader.GetDateTime("ISSUE_DATE");
                invoice.DueDate = reader.GetDateTime("DUE_DATE");
                invoice.Subject = reader.GetString("SUBJECT");
                invoice.SlipNumber = reader.GetString("SLIP_NUMBER");
                invoice.SubTotal = reader.GetInt32("SUBTOTAL");
                invoice.Tax = reader.GetInt32("TAX");
                invoice.Total = reader.GetInt32("TOTAL");
                invoice.Message = reader.GetString("MESSAGE");
                invoice.TransactionTypeId = reader.GetInt32("TRANSACTION_TYPE_ID");
                invoice.PaymentDate = reader.GetDateTime("PAYMENT_DATE");
                invoice.InvoiceStatusId = reader.GetInt32("INVOICE_STATUS_ID");
                invoice.IssueDateString = invoice.IssueDate.ToShortDateString();

                invoices.Add(invoice);
            }
            return invoices;
        }
        public static List<InvoiceClass> GetInvoiceByMonth(DateTime date)
        {
            var invoices = new List<InvoiceClass>();
            string connenctionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connenctionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_INVOICE WHERE ISSUE_DATE BETWEEN @start AND @end", connection);
            command.Parameters.AddWithValue("@start", date);
            command.Parameters.AddWithValue("@end", date.AddMonths(1).AddDays(-1));
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var invoice = new InvoiceClass();
                invoice.InvoiceId = reader.GetInt32("INVOICE_ID");
                invoice.CustomerId = reader.GetInt32("CUSTOMER_ID");
                invoice.IssueDate = reader.GetDateTime("ISSUE_DATE");
                invoice.DueDate = reader.GetDateTime("DUE_DATE");
                invoice.Subject = reader.GetString("SUBJECT");
                invoice.SlipNumber = reader.GetString("SLIP_NUMBER");
                invoice.SubTotal = reader.GetInt32("SUBTOTAL");
                invoice.Tax = reader.GetInt32("TAX");
                invoice.Total = reader.GetInt32("TOTAL");
                invoice.Message = reader.GetString("MESSAGE");
                invoice.TransactionTypeId = reader.GetInt32("TRANSACTION_TYPE_ID");
                invoice.PaymentDate = reader.GetDateTime("PAYMENT_DATE");
                invoice.InvoiceStatusId = reader.GetInt32("INVOICE_STATUS_ID");
                invoice.IssueDateString = invoice.IssueDate.ToShortDateString();
                invoices.Add(invoice);
            }
            return invoices;
        }

        public static List<InvoiceClass> GetInvoiceByStatus(int statusId)
        {
            var invoices = new List<InvoiceClass>();
            string connenctionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connenctionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_INVOICE WHERE INVOICE_STATUS_ID = @statusId", connection);
            command.Parameters.AddWithValue("@statusId", statusId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var invoice = new InvoiceClass();
                invoice.InvoiceId = reader.GetInt32("INVOICE_ID");
                invoice.CustomerId = reader.GetInt32("CUSTOMER_ID");
                invoice.IssueDate = reader.GetDateTime("ISSUE_DATE");
                invoice.DueDate = reader.GetDateTime("DUE_DATE");
                invoice.Subject = reader.GetString("SUBJECT");
                invoice.SlipNumber = reader.GetString("SLIP_NUMBER");
                invoice.SubTotal = reader.GetInt32("SUBTOTAL");
                invoice.Tax = reader.GetInt32("TAX");
                invoice.Total = reader.GetInt32("TOTAL");
                invoice.Message = reader.GetString("MESSAGE");
                invoice.TransactionTypeId = reader.GetInt32("TRANSACTION_TYPE_ID");
                invoice.PaymentDate = reader.GetDateTime("PAYMENT_DATE");
                invoice.InvoiceStatusId = reader.GetInt32("INVOICE_STATUS_ID");
                invoice.IssueDateString = invoice.IssueDate.ToShortDateString();
                invoices.Add(invoice);
            }
            return invoices;
        }


        public bool TryAddInvoice()
        {
            try
            {
                AddInvoice();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public void AddInvoice()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"INSERT INTO T_INVOICE (CUSTOMER_ID, ISSUE_DATE, DUE_DATE, SUBJECT, SLIP_NUMBER, SUBTOTAL, TAX, TOTAL, MESSAGE, TRANSACTION_TYPE_ID, PAYMENT_DATE, INVOICE_STATUS_ID)
                             VALUES (@CustomerId, @IssueDate, @DueDate, @Subject, @SlipNumber, @Subtotal, @Tax, @Total, @Message, @TransactionTypeId, @PaymentDate, @InvoiceStatusId)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@IssueDate", IssueDate);
            command.Parameters.AddWithValue("@DueDate", DueDate);
            command.Parameters.AddWithValue("@Subject", Subject);
            command.Parameters.AddWithValue("@SlipNumber", SlipNumber);
            command.Parameters.AddWithValue("@Subtotal", SubTotal);
            command.Parameters.AddWithValue("@Tax", Tax);
            command.Parameters.AddWithValue("@Total", Total);
            command.Parameters.AddWithValue("@Message", Message);
            command.Parameters.AddWithValue("@TransactionTypeId", TransactionTypeId);
            command.Parameters.AddWithValue("@PaymentDate", PaymentDate);
            command.Parameters.AddWithValue("@InvoiceStatusId", InvoiceStatusId);
            command.ExecuteNonQuery();
            InvoiceId = (int)command.LastInsertedId;

        }

        public bool TryUpdateInvoice()
        {
            try
            {
                UpdateInvoice();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }


        public void UpdateInvoice()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = 
                @"UPDATE T_INVOICE SET CUSTOMER_ID = @CustomerId, ISSUE_DATE = @IssueDate, DUE_DATE = @DueDate, SUBJECT = @Subject, SLIP_NUMBER = @SlipNumber, SUBTOTAL = @Subtotal, TAX = @Tax, TOTAL = @Total, MESSAGE = @Message, TRANSACTION_TYPE_ID = @TransactionTypeId, PAYMENT_DATE = @PaymentDate, INVOICE_STATUS_ID = @InvoiceStatusId WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@IssueDate", IssueDate);
            command.Parameters.AddWithValue("@DueDate", DueDate);
            command.Parameters.AddWithValue("@Subject", Subject);
            command.Parameters.AddWithValue("@SlipNumber", SlipNumber);
            command.Parameters.AddWithValue("@Subtotal", SubTotal);
            command.Parameters.AddWithValue("@Tax", Tax);
            command.Parameters.AddWithValue("@Total", Total);
            command.Parameters.AddWithValue("@Message", Message);
            command.Parameters.AddWithValue("@TransactionTypeId", TransactionTypeId);
            command.Parameters.AddWithValue("@PaymentDate", PaymentDate);
            command.Parameters.AddWithValue("@InvoiceStatusId", InvoiceStatusId);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.ExecuteNonQuery();
        }

        public void UpdateInvoiceStatus(int statusId)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "UPDATE T_INVOICE SET INVOICE_STATUS_ID = @StatusId WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@StatusId", statusId);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.ExecuteNonQuery();
        }

        public bool TryDeleteInvoice()
        {
            try
            {
                DeleteInvoice();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public void DeleteInvoice()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_INVOICE WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.ExecuteNonQuery();
        }

        public bool TryDeleteInvoiceById(int id)
        {
            try
            {
                DeleteInvoiceById(id);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public void DeleteInvoiceById(int id)
        {
            using var connection = new MySqlConnection(ConnectionInfo.Builder.ConnectionString);
            connection.Open();
            string query = "DELETE FROM T_INVOICE WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", id);
            command.ExecuteNonQuery();
        }

        public InvoiceClass DeepClone()
        {
            return new InvoiceClass
            {
                InvoiceId = this.InvoiceId,
                CustomerId = this.CustomerId,
                IssueDate = this.IssueDate,
                DueDate = this.DueDate,
                Subject = this.Subject,
                SlipNumber = this.SlipNumber,
                SubTotal = this.SubTotal,
                Tax = this.Tax,
                Total = this.Total,
                Message = this.Message,
                TransactionTypeId = this.TransactionTypeId,
                PaymentDate = this.PaymentDate,
                InvoiceStatusId = this.InvoiceStatusId,
                CustomerName = this.CustomerName,
                InvoiceStatus = this.InvoiceStatus,
                IssueDateString = this.IssueDateString
                // 必要に応じて他のプロパティもコピー
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // T_INVOICE_ITEMS テーブルに対応するクラス
    public class InvoiceItemClass : INotifyPropertyChanged
    {
        //public EventHandler ItemChanged;
        //public EventHandler ItemAdded;
        //public EventHandler ItemDeleted;

        //public DataGridComboBoxColumn ComboboxCulumn { get; set; }

        // InvoiceItemI
        private int _InvoiceItemId = 0;
        public int InvoiceItemId
        {
            get => _InvoiceItemId;
            set
            {
                _InvoiceItemId = value;
                OnPropertyChanged(nameof(InvoiceItemId));
            }
        }

        //InvoiceId
        private int _InvoiceId = 0;
        public int InvoiceId
        {
            get => _InvoiceId;
            set
            {
                _InvoiceId = value;
                OnPropertyChanged(nameof(InvoiceId));
            }
        }

        // ItemOrder
        private int _ItemOrder = 0;
        public int ItemOrder
        {
            get => _ItemOrder;
            set
            {
                _ItemOrder = value;
                OnPropertyChanged(nameof(ItemOrder));
            }
        }

        // ItemId
        private int _ItemId = 0;
        public int ItemId
        {
            get => _ItemId;
            set
            {
                _ItemId = value;
                OnPropertyChanged(nameof(ItemId));
            }
        }

        // ItemName
        private string _ItemName = "";
        public string ItemName
        {
            get => _ItemName;
            set
            {
                _ItemName = value;
                OnPropertyChanged(nameof(ItemName));
            }
        }

        // UnitPrice
        private int _UnitPrice = 0;
        public int UnitPrice
        {
            get => _UnitPrice;
            set
            {
                _UnitPrice = value;
                OnPropertyChanged(nameof(UnitPrice));
                ReTotal();
            }
        }

        // Quantity
        private int _Quantity = 1;
        public int Quantity
        {
            get => _Quantity;
            set
            {
                _Quantity = value;
                OnPropertyChanged(nameof(Quantity));
                ReTotal();
            }
        }

        // Unit
        private string _Unit = "";
        public string Unit
        {
            get => _Unit;
            set
            {
                _Unit = value;
                OnPropertyChanged(nameof(Unit));
                ReTotal();
            }
        }

        // ItemSubTotal
        private int _ItemSubTotal = 0;
        public int ItemSubTotal
        {
            get => _ItemSubTotal;
            set
            {
                _ItemSubTotal = value;
                OnPropertyChanged(nameof(ItemSubTotal));
            }
        }

        // TaxTypeId
        private string _TaxTypeName = "";
        public string TaxTypeName
        {
            get => _TaxTypeName;
            set
            {
                _TaxTypeName = value;
                OnPropertyChanged(nameof(TaxTypeName));
            }
        }

        // SelectedTax
        private TaxTypeClass _selectedTax;
        public TaxTypeClass SelectedTax
        {
            get => _selectedTax;
            set
            {
                if (_selectedTax != value)
                {
                    _selectedTax = value;
                    OnPropertyChanged(nameof(SelectedTax));
                    if (_selectedTax != null && _taxTypeId != _selectedTax.TaxTypeId)
                    {
                        _taxTypeId = _selectedTax.TaxTypeId;
                        OnPropertyChanged(nameof(TaxTypeId));
                        TaxTypeName = _selectedTax.TaxTypeName;
                    }
                    ReTotal();
                }
            }
        }

        // TaxTypeId
        private int _taxTypeId = 1;
        public int TaxTypeId
        {
            get => _taxTypeId;
            set
            {
                if (_taxTypeId != value)
                {
                    _taxTypeId = value;
                    OnPropertyChanged(nameof(TaxTypeId));

                    var tax = InvoiceViewModel.TaxTypeClassList.FirstOrDefault(t => t.TaxTypeId == _taxTypeId);
                    if (tax != null && _selectedTax != tax)
                    {
                        _selectedTax = tax;
                        OnPropertyChanged(nameof(SelectedTax));
                        TaxTypeName = _selectedTax.TaxTypeName;
                    }
                    ReTotal();
                }
            }
        }

        // Tax
        private int _Tax = 0;
        public int Tax
        {
            get => _Tax;
            set
            {
                _Tax = value;
                OnPropertyChanged(nameof(Tax));
            }
        }

        // ItemTotal
        private int _ItemTotal = 0;
        public int ItemTotal
        {
            get => _ItemTotal;
            set
            {
                _ItemTotal = value;
                OnPropertyChanged(nameof(ItemTotal));
            }
        }

        public void ReTotal()
        {
            ItemSubTotal = Quantity * UnitPrice;
            var taxRate = SelectedTax?.TaxRate ?? 0;
            Tax = (int)(ItemSubTotal * taxRate);
            ItemTotal = ItemSubTotal + Tax;
        }
        public void ReTotal(InvoiceItemClass item)
        {
            item.ItemSubTotal = item.Quantity * item.UnitPrice;
            item.Tax = (int)(item.Quantity * TaxTypeClass.GetTaxes().FirstOrDefault(t => t.TaxTypeId == item.TaxTypeId)?.TaxRate ?? 0);
            item.ItemTotal = item.ItemSubTotal + item.Tax;
        }

        private ItemClass _selectedItem;
        public ItemClass SelectedItem
        {
            get => _selectedItem;
            set
            {
                if(_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    if (_selectedItem != null)
                    {
                        // 選択されたアイテムの情報をInvoiceItemのプロパティに反映
                        ItemId = _selectedItem.ItemId;
                        ItemName = _selectedItem.ItemName;
                        UnitPrice = _selectedItem.UnitPrice;
                        Quantity = 1;
                        Unit = _selectedItem.Unit;
                        TaxTypeId = _selectedItem.TaxTypeId;
                        TaxTypeName = new TaxTypeClass().getTaxTypeName(TaxTypeId);
                    }
                }
            }
        }

        public void SetItem(ItemClass item)
        {
            if (item == null) return;
            ItemId = item.ItemId;
            ItemName = item.ItemName;
            UnitPrice = item.UnitPrice;
            Quantity = 1;
            Unit = item.Unit;
            TaxTypeId = item.TaxTypeId;
            TaxTypeName = new TaxTypeClass().getTaxTypeName(TaxTypeId);

        }

        public static List<InvoiceItemClass> GetInvoiceItemsByInvoiceId(int invoiceId)
        {
            var items = new List<InvoiceItemClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "SELECT * FROM T_INVOICE_ITEMS WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", invoiceId);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new InvoiceItemClass
                {
                    InvoiceItemId = reader.GetInt32("INVOICE_ITEM_ID"),
                    InvoiceId = reader.GetInt32("INVOICE_ID"),
                    ItemOrder = reader.GetInt32("ITEM_ORDER"),
                    ItemId = reader.GetInt32("ITEM_ID"),
                    ItemName = reader.GetString("ITEM_NAME"),
                    UnitPrice = reader.GetInt32("UNIT_PRICE"),
                    Quantity = reader.GetInt32("QUANTITY"),
                    Unit = reader.IsDBNull("UNIT") ? "" : reader.GetString("UNIT"),
                    ItemSubTotal = reader.GetInt32("ITEM_SUBTOTAL"),
                    TaxTypeId = reader.GetInt32("TAX_TYPE_ID"),
                    Tax = reader.GetInt32("TAX"),
                    ItemTotal = reader.GetInt32("ITEM_TOTAL")
                };
                items.Add(item);
            }
            return items;
        }


        public static void DeleteInvoiceItemsByInvoiceId(int invoiceId)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_INVOICE_ITEMS WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", invoiceId);
            command.ExecuteNonQuery();
        }

        public static void DeleteInvoiceItemByInvoiceItemId(int invoiceItemId)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_INVOICE_ITEMS WHERE INVOICE_ITEM_ID = @InvoiceItemId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceItemId", invoiceItemId);
            command.ExecuteNonQuery();
        }

        public static void AddInvoiceItem(List<InvoiceItemClass> ItemList, int invoiceId)
        {
            string commandString = "INSERT INTO T_INVOICE_ITEMS (INVOICE_ID, ITEM_ORDER, ITEM_ID, ITEM_NAME, UNIT_PRICE, QUANTITY, UNIT, ITEM_SUBTOTAL, TAX_TYPE_ID, TAX, ITEM_TOTAL) VALUES ";
            int order = 1;
            foreach (var item in ItemList)
            {
                item.ItemOrder = order++;
                commandString += $"({invoiceId}, {item.ItemOrder}, {item.ItemId}, '{item.ItemName}', {item.UnitPrice}, {item.Quantity}, '{item.Unit}', {item.ItemSubTotal}, {item.TaxTypeId}, {item.Tax}, {item.ItemTotal}),";
            }
            commandString = commandString.Remove(commandString.Length - 1);
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand(commandString, connection);
            command.ExecuteNonQuery();

        }

        public void AddInvoiceItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"INSERT INTO T_INVOICE_ITEMS (INVOICE_ID, ITEM_ORDER, ITEM_ID, ITEM_NAME, UNIT_PRICE, QUANTITY, UNIT,ITEM_SUBTOTAL, TAX_TYPE_ID, TAX, ITEM_TOTAL)
                             VALUES (@InvoiceId, @ItemOrder, @ItemId, @ItemName, @UnitPrice, @Quantity, @Unit, @ItemSubtotal, @TaxTypeId, @Tax, @ItemTotal)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@ItemOrder", ItemOrder);
            command.Parameters.AddWithValue("@ItemId", ItemId);
            command.Parameters.AddWithValue("@ItemName", ItemName);
            command.Parameters.AddWithValue("@UnitPrice", UnitPrice);
            command.Parameters.AddWithValue("@Quantity", Quantity);
            command.Parameters.AddWithValue("@Unit", Unit);
            command.Parameters.AddWithValue("@ItemSubtotal", ItemSubTotal);
            command.Parameters.AddWithValue("@TaxTypeId", TaxTypeId);
            command.Parameters.AddWithValue("@Tax", Tax);
            command.Parameters.AddWithValue("@ItemTotal", ItemTotal);
            command.ExecuteNonQuery();
        }

        public void UpdateInvoiceItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"UPDATE T_INVOICE_ITEMS SET INVOICE_ID = @InvoiceId, ITEM_ORDER = @ItemOrder, ITEM_ID = @ItemId, ITEM_NAME = @ItemName,
                             UNIT_PRICE = @UnitPrice, QUANTITY = @Quantity, UNIT = @Unit ,ITEM_SUBTOTAL = @ItemSubtotal, TAX_TYPE_ID = @TaxTypeId,
                             TAX = @Tax, ITEM_TOTAL = @ItemTotal WHERE INVOICE_ITEMS_ID = @InvoiceItemsId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@ItemOrder", ItemOrder);
            command.Parameters.AddWithValue("@ItemId", ItemId);
            command.Parameters.AddWithValue("@ItemName", ItemName);
            command.Parameters.AddWithValue("@UnitPrice", UnitPrice);
            command.Parameters.AddWithValue("@Quantity", Quantity);
            command.Parameters.AddWithValue("@Unit", Unit);
            command.Parameters.AddWithValue("@ItemSubtotal", ItemSubTotal);
            command.Parameters.AddWithValue("@TaxTypeId", TaxTypeId);
            command.Parameters.AddWithValue("@Tax", Tax);
            command.Parameters.AddWithValue("@ItemTotal", ItemTotal);
            command.Parameters.AddWithValue("@InvoiceItemsId", InvoiceItemId);
            command.ExecuteNonQuery();
        }

        public void DeleteInvoiceItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_INVOICE_ITEMS WHERE INVOICE_ITEMS_ID = @InvoiceItemsId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceItemsId", InvoiceItemId);
            command.ExecuteNonQuery();
        }

        public InvoiceItemClass DeepClone()
        {
            return new InvoiceItemClass
            {
                InvoiceItemId = this.InvoiceItemId,
                InvoiceId = this.InvoiceId,
                ItemOrder = this.ItemOrder,
                ItemId = this.ItemId,
                ItemName = this.ItemName,
                UnitPrice = this.UnitPrice,
                Quantity = this.Quantity,
                Unit = this.Unit,
                ItemSubTotal = this.ItemSubTotal,
                TaxTypeId = this.TaxTypeId,
                Tax = this.Tax,
                ItemTotal = this.ItemTotal
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // T_INVOICE_STATUS テーブルに対応するクラス
    public class InvoiceStatusClass : INotifyPropertyChanged
    {
        public PropertyChangedEventHandler StatusChanged;
        private int _InvoiceStatusId = 0;
        public int InvoiceStatusId 
        {
            get
            {
                return _InvoiceStatusId;
            }
            set
            {
                if(_InvoiceStatusId != value)
                {
                    _InvoiceStatusId = value;
                    OnPropertyChanged(nameof(InvoiceStatusId));
                }

            }
        }
        private string _InvoiceStatus  = "";
        public string InvoiceStatus
        {
            get
            {
                return _InvoiceStatus;
            }
            set
            {
                if (_InvoiceStatus != value)
                {
                    _InvoiceStatus = value;
                    OnPropertyChanged(nameof(InvoiceStatus));
                }

            }
        }

        public static List<InvoiceStatusClass> GetInvoiceStatuses()
        {
            var invoiceStatuses = new List<InvoiceStatusClass>();
            string connenctionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connenctionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_INVOICE_STATUS", connection);
            using var reader = command.ExecuteReader();
            invoiceStatuses.Add(new InvoiceStatusClass());
            while (reader.Read())
            {
                var invoiceStatus = new InvoiceStatusClass();
                invoiceStatus.InvoiceStatusId = reader.GetInt32("INVOICE_STATUS_ID");
                invoiceStatus.InvoiceStatus = reader.GetString("INVOICE_STATUS");
                invoiceStatuses.Add(invoiceStatus);
            }
            return invoiceStatuses;
        }



        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            StatusChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
    }

    // T_PAYMENT テーブルに対応するクラス
    public class PaymentClass : INotifyPropertyChanged
    {
        private int _PaymentId;
        public int PaymentId
        {
            get { return _PaymentId; }
            set
            {
                if (_PaymentId != value)
                {
                    _PaymentId = value;
                    OnPropertyChanged(nameof(PaymentId));
                }
            }
        }

        private int? _InvoiceId;
        public int? InvoiceId
        {
            get { return _InvoiceId; }
            set
            {
                if (_InvoiceId != value)
                {
                    _InvoiceId = value;
                    OnPropertyChanged(nameof(InvoiceId));
                }
            }
        }

        private int _PaymentMethodId;
        public int PaymentMethodId
        {
            get { return _PaymentMethodId; }
            set
            {
                if (_PaymentMethodId != value)
                {
                    _PaymentMethodId = value;
                    OnPropertyChanged(nameof(PaymentMethodId));
                }
            }
        }

        private string _SlipNumber = "";
        public string SlipNumber
        {
            get { return _SlipNumber; }
            set
            {
                if (_SlipNumber != value)
                {
                    _SlipNumber = value;
                    OnPropertyChanged(nameof(SlipNumber));
                }
            }
        }

        private int _CustomerId;
        public int CustomerId
        {
            get { return _CustomerId; }
            set
            {
                if (_CustomerId != value)
                {
                    _CustomerId = value;
                    OnPropertyChanged(nameof(CustomerId));
                }
            }
        }

        private string _CustomerName = "";
        public string CustomerName
        {
            get { return _CustomerName; }
            set
            {
                if (_CustomerName != value)
                {
                    _CustomerName = value;
                    OnPropertyChanged(nameof(CustomerName));
                }
            }
        }

        private DateTime _PaymentDate = DateTime.Now;
        public DateTime PaymentDate
        {
            get { return _PaymentDate; }
            set
            {
                if (_PaymentDate != value)
                {
                    _PaymentDate = value;
                    OnPropertyChanged(nameof(PaymentDate));
                }
            }
        }

        private int _PaymentAmount;
        public int PaymentAmount
        {
            get { return _PaymentAmount; }
            set
            {
                if (_PaymentAmount != value)
                {
                    _PaymentAmount = value;
                    OnPropertyChanged(nameof(PaymentAmount));
                }
            }
        }

        private string _Subject;
        public string Subject
        {
            get { return _Subject; }
            set
            {
                if (_Subject != value)
                {
                    _Subject = value;
                    OnPropertyChanged(nameof(Subject));
                }
            }
        }

        private string _PaymentDateString;
        public string PaymentDateString
        {
            get { return _PaymentDateString; }
            set
            {

                if (_PaymentDateString != value)
                {
                    _PaymentDateString = value;
                    OnPropertyChanged(nameof(PaymentDateString));

                }
            }
        }

        public static List<PaymentClass> GetAllPayments()
        {
            var payments = new List<PaymentClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_PAYMENT", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var payment = new PaymentClass
                {
                    PaymentId = reader.GetInt32("PAYMENT_ID"),
                    InvoiceId = reader.IsDBNull("INVOICE_ID") ? null : reader.GetInt32("INVOICE_ID"),
                    PaymentMethodId = reader.GetInt32("PAYMENT_METHOD_ID"),
                    SlipNumber = reader.GetString("SLIP_NUMBER"),
                    CustomerId = reader.GetInt32("CUSTOMER_ID"),
                    PaymentDate = reader.GetDateTime("PAYMENT_DATE"),
                    PaymentAmount = reader.GetInt32("PAYMENT_AMOUNT"),
                    Subject = reader.IsDBNull("SUBJECT") ? "" : reader.GetString("SUBJECT"),
                };
                payment.PaymentDateString = payment.PaymentDate.ToShortDateString();
                payments.Add(payment);
            }
            return payments;

        }

        public static List<PaymentClass> GetPaymentsByMonth(DateTime date)
        {
            var payments = new List<PaymentClass>();
            string connenctionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connenctionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_PAYMENT WHERE PAYMENT_DATE BETWEEN @start AND @end", connection);
            command.Parameters.AddWithValue("@start", date);
            command.Parameters.AddWithValue("@end", date.AddMonths(1).AddDays(-1));
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var payment = new PaymentClass();
                payment.PaymentId = reader.GetInt32("PAYMENT_ID");
                payment.InvoiceId = reader.IsDBNull("INVOICE_ID") ? null : reader.GetInt32("INVOICE_ID");
                payment.PaymentMethodId = reader.GetInt32("PAYMENT_METHOD_ID");
                payment.SlipNumber = reader.GetString("SLIP_NUMBER");
                payment.CustomerId = reader.GetInt32("CUSTOMER_ID");
                payment.PaymentDate = reader.GetDateTime("PAYMENT_DATE");
                payment.PaymentAmount = reader.GetInt32("PAYMENT_AMOUNT");
                payment.Subject = reader.IsDBNull("SUBJECT") ? "" : reader.GetString("SUBJECT");
                payment.PaymentDateString = payment.PaymentDate.ToShortDateString();
                payments.Add(payment);
            }
            return payments;
        }


        public bool TryAddPayment()
        {
            try
            {
                AddPayment();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public void AddPayment()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"INSERT INTO T_PAYMENT (INVOICE_ID, PAYMENT_METHOD_ID, CUSTOMER_ID, SLIP_NUMBER, PAYMENT_DATE, PAYMENT_AMOUNT, SUBJECT)
                             VALUES (@InvoiceId, @PaymentMethodId, @CustomerId, @SlipNumber, @PaymentDate, @PaymentAmount, @Subject)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@PaymentMethodId", PaymentMethodId);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@SlipNumber", SlipNumber);
            command.Parameters.AddWithValue("@PaymentDate", PaymentDate);
            command.Parameters.AddWithValue("@PaymentAmount", PaymentAmount);
            command.Parameters.AddWithValue("@Subject", Subject);
            command.ExecuteNonQuery();
            PaymentId = (int)command.LastInsertedId;
        }

        public bool TryUpdatePayment()
        {
            try
            {
                UpdatePayment();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public void UpdatePayment()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"UPDATE T_PAYMENT SET INVOICE_ID = @InvoiceId, PAYMENT_METHOD_ID = @PaymentMethodId,
                             CUSTOMER_ID = @CustomerId, PAYMENT_DATE = @PaymentDate, PAYMENT_AMOUNT = @PaymentAmount,
                             SUBJECT = @Subject WHERE PAYMENT_ID = @PaymentId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@PaymentMethodId", PaymentMethodId);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@PaymentDate", PaymentDate);
            command.Parameters.AddWithValue("@PaymentAmount", PaymentAmount);
            command.Parameters.AddWithValue("@Subject", Subject);
            command.Parameters.AddWithValue("@PaymentId", PaymentId);
            command.ExecuteNonQuery();
        }

        public bool TryDeletePayment()
        {
            try
            {
                DeletePayment();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public void DeletePayment()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_PAYMENT WHERE PAYMENT_ID = @PaymentId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@PaymentId", PaymentId);
            command.ExecuteNonQuery();
        }

        public static bool TryDeletePaymentById(int paymentId)
        {
            try
            {
                DeletePyamentById(paymentId);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public static void DeletePyamentById(int paymentId)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_PAYMENT WHERE PAYMENT_ID = @PaymentId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@PaymentId", paymentId);
            command.ExecuteNonQuery();
        }

        public static bool TryDeletePaymentByInvoiceId(int invoiceId)
        {
            try
            {
                DeletePaymentByInvoiceId(invoiceId);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public static void DeletePaymentByInvoiceId(int invoiceId)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_PAYMENT WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", invoiceId);
            command.ExecuteNonQuery();
        }

        public static void ClearInvoiceIdFromPayment(int invoiceId)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "UPDATE T_PAYMENT SET INVOICE_ID = NULL WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", invoiceId);
            command.ExecuteNonQuery();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    // T_ITEM テーブルに対応するクラス
    public class ItemClass : INotifyPropertyChanged
    {
        public int ItemId { get; set; } = 0;

        private string _itemName = "";
        public string ItemName
        {
            get => _itemName;
            set
            {
                _itemName = value;
                OnPropertyChanged(nameof(ItemName));
            }
        }

        private string _itemCode = "";
        public string ItemCode
        {
            get => _itemCode;
            set
            {
                _itemCode = value;
                OnPropertyChanged(nameof(ItemCode));
            }
        }

        private string _unit = "";
        public string Unit
        {
            get => _unit;
            set
            {
                _unit = value;
                OnPropertyChanged(nameof(Unit));
            }
        }

        private int _unitPrice = 0;
        public int UnitPrice
        {
            get => _unitPrice;
            set
            {
                _unitPrice = value;
                OnPropertyChanged(nameof(UnitPrice));
            }
        }

        private int _taxTypeId = 1;
        public int TaxTypeId
        {
            get => _taxTypeId;
            set
            {
                _taxTypeId = value;
                OnPropertyChanged(nameof(TaxTypeId));
            }
        }

        private string _taxTypeName = "";
        public string TaxTypeName
        {
            get => _taxTypeName;
            set
            {
                _taxTypeName = value;
                OnPropertyChanged(nameof(TaxTypeName));
            }
        }

        public static List<ItemClass> GetItems()
        {
            var items = new List<ItemClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_ITEM", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new ItemClass
                {
                    ItemId = reader.GetInt32("ITEM_ID"),
                    ItemName = reader.GetString("ITEM_NAME"),
                    ItemCode = reader.GetString("ITEM_CODE"),
                    Unit = reader.GetString("UNIT"),
                    UnitPrice = reader.GetInt32("UNIT_PRICE"),
                    TaxTypeId = reader.GetInt32("TAX_TYPE_ID")
                };
                items.Add(item);
            }
            return items;
        }

        public ItemClass Copy()
        {
            return (ItemClass)this.MemberwiseClone();
        }

        public void AddItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand(
                "INSERT INTO T_ITEM (ITEM_NAME, ITEM_CODE, UNIT, UNIT_PRICE, TAX_TYPE_ID) " +
                "VALUES (@name, @code, @unit, @price, @taxTypeId)", connection);
            command.Parameters.AddWithValue("@name", ItemName);
            command.Parameters.AddWithValue("@code", ItemCode);
            command.Parameters.AddWithValue("@unit", Unit);
            command.Parameters.AddWithValue("@price", UnitPrice);
            command.Parameters.AddWithValue("@taxTypeId", TaxTypeId);
            command.ExecuteNonQuery();
        }

        public void UpdateItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand(
                "UPDATE T_ITEM SET ITEM_NAME=@name, ITEM_CODE=@code, UNIT=@unit, " +
                "UNIT_PRICE=@price, TAX_TYPE_ID=@taxTypeId WHERE ITEM_ID=@id", connection);
            command.Parameters.AddWithValue("@name", ItemName);
            command.Parameters.AddWithValue("@code", ItemCode);
            command.Parameters.AddWithValue("@unit", Unit);
            command.Parameters.AddWithValue("@price", UnitPrice);
            command.Parameters.AddWithValue("@taxTypeId", TaxTypeId);
            command.Parameters.AddWithValue("@id", ItemId);
            command.ExecuteNonQuery();
        }

        public void DeleteItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("DELETE FROM T_ITEM WHERE ITEM_ID=@id", connection);
            command.Parameters.AddWithValue("@id", ItemId);
            command.ExecuteNonQuery();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // T_PAYMENT_METHOD テーブルに対応するクラス
    public class PaymentMethodClass
    {
        public int PaymentMethodId { get; set; }
        public string MethodName { get; set; }
        public int DebitOrCreditId { get; set; }

        public static List<PaymentMethodClass> GetPaymentMethods()
        {
            var paymentMethods = new List<PaymentMethodClass>();
            string connenctionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connenctionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_PAYMENT_METHOD", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var method = new PaymentMethodClass
                {
                    PaymentMethodId = reader.GetInt32("PAYMENT_METHOD_ID"),
                    MethodName = reader.GetString("METHOD_NAME"),
                    DebitOrCreditId = reader.GetInt32("DEBIT_OR_CREDIT_ID")
                };
                paymentMethods.Add(method);
            }
            return paymentMethods;
        }
        public static int AddPaymentMethod(PaymentMethodClass methodClass)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("INSERT INTO T_PAYMENT_METHOD (METHOD_NAME, DEBIT_OR_CREDIT_ID) VALUES (@methodName, @debitOrCreditId)", connection);
            command.Parameters.AddWithValue("@methodName", methodClass.MethodName);
            command.Parameters.AddWithValue("@debitOrCreditId", 2);
            command.ExecuteNonQuery();
            return (int)command.LastInsertedId;
        }
        public void UpdatePaymentMethod()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("UPDATE T_PAYMENT_METHOD SET METHOD_NAME=@methodName WHERE PAYMENT_METHOD_ID=@methodId", connection);
            command.Parameters.AddWithValue("@methodName", MethodName);
            command.Parameters.AddWithValue("@methodId", PaymentMethodId);
            command.Parameters.AddWithValue("@debitOrCreditId", 2);
            command.ExecuteNonQuery();
        }

        public void DeletePaymentMethod()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("DELETE FROM T_PAYMENT_METHOD WHERE PAYMENT_METHOD_ID=@methodId", connection);
            command.Parameters.AddWithValue("@methodId", PaymentMethodId);
            command.ExecuteNonQuery();
        }
    }

    public class CompanyInfo
    {
        private int _companyId = 0;
        public int CompanyId { get; set; }

        private string _companyName = "";
        public string CompanyName { get; set; }

        private string _companyPostalcode = "";
        public string CompanyPostalcode { get; set; }

        private string _companyAddress = "";
        public string CompanyAddress { get; set; }

        private string _companyPhone = "";
        public string CompanyPhone { get; set; }

        private string _presidentName = "";
        public string PresidentName { get; set; }

        public static CompanyInfo GetCompanyInfo()
        {
            var companyInfo = new CompanyInfo();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_COMPANY", connection);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                companyInfo.CompanyId = reader.GetInt32("COMPANY_ID");
                companyInfo.CompanyName = reader.GetString("COMPANY_NAME");
                companyInfo.CompanyPostalcode = reader.GetString("COMPANY_POSTALCODE");
                companyInfo.CompanyAddress = reader.GetString("COMPANY_ADDRESS");
                companyInfo.CompanyPhone = reader.GetString("COMPANY_PHONE");
                companyInfo.PresidentName = reader.GetString("PRESIDENT_NAME");
            }
            return companyInfo;
        }

        public void UpdateCompanyInfo()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand(
                "UPDATE T_COMPANY SET COMPANY_NAME=@companyName, COMPANY_POSTALCODE=@companyPostalcode, " +
                "COMPANY_ADDRESS=@companyAddress, COMPANY_PHONE=@companyPhone, PRESIDENT_NAME=@presidentName", connection);
            command.Parameters.AddWithValue("@companyName", CompanyName);
            command.Parameters.AddWithValue("@companyPostalcode", CompanyPostalcode);
            command.Parameters.AddWithValue("@companyAddress", CompanyAddress);
            command.Parameters.AddWithValue("@companyPhone", CompanyPhone);
            command.Parameters.AddWithValue("@presidentName", PresidentName);
            command.ExecuteNonQuery();
        }

    }
    // T_SLIP_NUMBER_INFO テーブルに対応するクラス
    public class SlipNumberClass : INotifyPropertyChanged
    {
        private string _InvoicePrefix = "";
        public string InvoicePrefix
        {
            get => _InvoicePrefix;
            set
            {
                _InvoicePrefix = value;
                OnPropertyChanged(nameof(InvoicePrefix));
            }
        }

        private string _InvoiceSuffix = "";
        public string InvoiceSuffix
        {
            get => _InvoiceSuffix;
            set
            {
                _InvoiceSuffix = value;
                OnPropertyChanged(nameof(InvoiceSuffix));
            }
        }

        private string _ReceiptPrefix = "";
        public string ReceiptPrefix
        {
            get => _ReceiptPrefix;
            set
            {
                _ReceiptPrefix = value;
                OnPropertyChanged(nameof(ReceiptPrefix));
            }
        }

        private string _ReceiptSuffix = "";
        public string ReceiptSuffix
        {
            get => _ReceiptSuffix;
            set
            {
                _ReceiptSuffix = value;
                OnPropertyChanged(nameof(ReceiptSuffix));
            }
        }

        private int _InvoiceLatest = 0;
        public int InvoiceLatest
        {
            get => _InvoiceLatest;
            set
            {
                _InvoiceLatest = value;
                OnPropertyChanged(nameof(InvoiceLatest));
            }
        }

        private int _ReceiptLatest = 0;
        public int ReceiptLatest
        {
            get => _ReceiptLatest;
            set
            {
                _ReceiptLatest = value;
                OnPropertyChanged(nameof(ReceiptLatest));
            }
        }

        public static SlipNumberClass GetSlipNumberInfo()
        {
            var slipNumberInfo = new SlipNumberClass();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_SLIP_NUMBER_INFO", connection);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                slipNumberInfo.InvoicePrefix = reader.GetString("INVOICE_PREFIX");
                slipNumberInfo.InvoiceSuffix = reader.GetString("INVOICE_SUFFIX");
                slipNumberInfo.ReceiptPrefix = reader.GetString("RECEIPT_PREFIX");
                slipNumberInfo.ReceiptSuffix = reader.GetString("RECEIPT_SUFFIX");
                slipNumberInfo.InvoiceLatest = reader.GetInt32("INVOICE_LATEST");
                slipNumberInfo.ReceiptLatest = reader.GetInt32("RECEIPT_LATEST");
            }
            return slipNumberInfo;
        }

        public void UpdateSlipNumberInfo()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand(
                "UPDATE T_SLIP_NUMBER_INFO SET INVOICE_PREFIX=@invoicePrefix, INVOICE_SUFFIX=@invoiceSuffix, " +
                "RECEIPT_PREFIX=@receiptPrefix, RECEIPT_SUFFIX=@receiptSuffix, INVOICE_LATEST=@invoiceLatest, " +
                "RECEIPT_LATEST=@receiptLatest", connection);
            command.Parameters.AddWithValue("@invoicePrefix", InvoicePrefix);
            command.Parameters.AddWithValue("@invoiceSuffix", InvoiceSuffix);
            command.Parameters.AddWithValue("@receiptPrefix", ReceiptPrefix);
            command.Parameters.AddWithValue("@receiptSuffix", ReceiptSuffix);
            command.Parameters.AddWithValue("@invoiceLatest", InvoiceLatest);
            command.Parameters.AddWithValue("@receiptLatest", ReceiptLatest);
            command.ExecuteNonQuery();
        }
        public void InclimentInvoiceLatest()
        {
            InvoiceLatest++;
            UpdateSlipNumberInfo();
        }
        public void InclimentReceiptLatest()
        {
            ReceiptLatest++;
            UpdateSlipNumberInfo();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            UpdateSlipNumberInfo();
        }
    }

    // T_TAX_TYPE テーブルに対応するクラス
    public class TaxTypeClass : INotifyPropertyChanged
    {

        private int _taxTypeId = 0;
        public int TaxTypeId
        {
            get => _taxTypeId;
            set
            {
                _taxTypeId = value;
                OnPropertyChanged(nameof(TaxTypeId));
            }
        }

        private string _taxTypeName = "";
        public string TaxTypeName
        {
            get => _taxTypeName;
            set
            {
                _taxTypeName = value;
                OnPropertyChanged(nameof(TaxTypeName));
            }
        }

        private double _taxRate = 0;
        public double TaxRate
        {
            get => _taxRate;
            set
            {
                _taxRate = value;
                OnPropertyChanged(nameof(TaxRate));
            }
        }

        public static List<TaxTypeClass> GetTaxes()
        {
            var taxes = new List<TaxTypeClass>();
            string connenctionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connenctionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_TAX_TYPE", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var tax = new TaxTypeClass();
                tax.TaxTypeId = reader.GetInt32("TAX_TYPE_ID");
                tax.TaxTypeName = reader.GetString("TAX_TYPE_NAME");
                tax.TaxRate = reader.GetDouble("TAX_RATE");
                taxes.Add(tax);
            }
            return taxes;
        }

        public static int AddTaxType(TaxTypeClass taxType)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("INSERT INTO T_TAX_TYPE (TAX_TYPE_NAME, TAX_RATE) VALUES (@name, @rate)", connection);
            command.Parameters.AddWithValue("@name", taxType.TaxTypeName);
            command.Parameters.AddWithValue("@rate", taxType.TaxRate);
            command.ExecuteNonQuery();
            return (int)command.LastInsertedId;
        }

        public void UpdateTaxType()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("UPDATE T_TAX_TYPE SET TAX_TYPE_NAME=@name, TAX_RATE=@rate WHERE TAX_TYPE_ID=@id", connection);
            command.Parameters.AddWithValue("@name", TaxTypeName);
            command.Parameters.AddWithValue("@rate", TaxRate);
            command.Parameters.AddWithValue("@id", TaxTypeId);
            command.ExecuteNonQuery();
        }

        public void DeleteTaxType()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("DELETE FROM T_TAX_TYPE WHERE TAX_TYPE_ID=@id", connection);
            command.Parameters.AddWithValue("@id", TaxTypeId);
            command.ExecuteNonQuery();
        }

        public string getTaxTypeName(int taxTypeId)
        {
            return TaxTypeClass.GetTaxes().FirstOrDefault(t => t.TaxTypeId == taxTypeId)?.TaxTypeName ?? "";
        }

        public TaxTypeClass GetTaxTypeClassByID(int taxTypeId)
        {
            return TaxTypeClass.GetTaxes().FirstOrDefault(t => t.TaxTypeId == taxTypeId);
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // T_TRANSACTION_TYPE テーブルに対応するクラス
    public class TransactionTypeClass
    {
        public int TransactionTypeId { get; set; }
        public string TransactionName { get; set; }
        public int DebitOrCreditId { get; set; }

        public static List<TransactionTypeClass> GetTransactionTypes()
        {
            var types = new List<TransactionTypeClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_TRANSACTION_TYPE", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var type = new TransactionTypeClass
                {
                    TransactionTypeId = reader.GetInt32("TRANSACTION_TYPE_ID"),
                    TransactionName = reader.GetString("TRANSACTION_NAME"),
                    DebitOrCreditId = reader.GetInt32("DEBIT_OR_CREDIT_ID")
                };
                types.Add(type);
            }
            return types;
        }
        public static int AddTransactionType(TransactionTypeClass transactionType)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("INSERT INTO T_TRANSACTION_TYPE (TRANSACTION_NAME, DEBIT_OR_CREDIT_ID) VALUES (@name, @debitOrCreditId)", connection);
            command.Parameters.AddWithValue("@name", transactionType.TransactionName);
            command.Parameters.AddWithValue("@debitOrCreditId", 2);
            command.ExecuteNonQuery();
            return (int)command.LastInsertedId;
        }
        public void UpdateTransactionType()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("UPDATE T_TRANSACTION_TYPE SET TRANSACTION_NAME=@name WHERE TRANSACTION_TYPE_ID=@id", connection);
            command.Parameters.AddWithValue("@name", TransactionName);
            command.Parameters.AddWithValue("@id", TransactionTypeId);
            command.Parameters.AddWithValue("@debitOrCreditId", 2);
            command.ExecuteNonQuery();
        }
        public void DeleteTransactionType()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("DELETE FROM T_TRANSACTION_TYPE WHERE TRANSACTION_TYPE_ID=@id", connection);
            command.Parameters.AddWithValue("@id", TransactionTypeId);
            command.ExecuteNonQuery();
        }
    }

    public class InvoiceFiterParam
    {
        public int CustomerId { get; set; } = 0;
        public int InvoiceStatusId { get; set; } = 0;
        public int TransactionTypeId { get; set; } = 0;
        public DateTime? IssueDate { get; set; } = null;
        public DateTime? DueDate { get; set; } = null;
        public DateTime? PaymentDate { get; set; } = null;
        public string? Subject { get; set; } = null;
        public int InvoiceId { get; set; } = 0;
        public int PaymentId { get; set; } = 0;

    }

    public class PaymentFilterParam
    {
        public int? PaymentId { get; set; } = null;
        public string? SlipNumber { get; set; } = null;
        public int? CustomerId { get; set; } = null;
        public int? InvoiceId { get; set; } = null;
        public int? PaymentMethodId { get; set; } = null;
        public DateTime? PaymentDate { get; set; } = null;
        public int? PaymentAmount { get; set; } = null;
        public string? Subject { get; set; } = null;

    }

    public static class VisualTreeHelperExtensions
    {
        public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild)
                {
                    return tChild;
                }

                var result = FindVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public static T? FindVisualChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild && tChild.Name == name)
                {
                    return tChild;
                }

                var result = FindVisualChildByName<T>(child, name);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj)
    where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static childItem findVisualChild<childItem>(DependencyObject obj)
            where childItem : DependencyObject
        {
            foreach (childItem child in FindVisualChildren<childItem>(obj))
            {
                return child;
            }

            return null;
        }
    }


    public static class FileNameHelper
    {

        public static string GenerateInvoiceFilename(string directory, InvoiceClass invoice)
        {
            string baseName = $"請求書_{invoice.DueDate.ToString("yyyyMM")}_{invoice.Subject}_{invoice.CustomerName}.pdf";
            string uniqueName = GenerateUniqueFileName(directory, baseName);
            return uniqueName;
        }
        public static string GenerateReceiptFileName(string directory, PaymentClass payment)
        {
            string basename = $"領収書_{payment.PaymentDate.ToString("yyyyMM")}_{payment.Subject}_{payment.CustomerName}.pdf";
            string uniqueName = GenerateUniqueFileName(directory, basename);
            return uniqueName;
        }

        public static string GenerateUniqueFileName(string directory, string baseName)
        {
            string fileName = baseName;
            string noExtName = Path.GetFileNameWithoutExtension(baseName);
            var files = Directory.GetFiles(directory, $"{noExtName}*", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                return $"{directory}\\{fileName}";
            }
            
            var countNumbers = new List<int>();
            var pattern = @$"(?:{noExtName}\s\()([\d]*)(?:\))";
            foreach (var file in files)
            {
                var match = Regex.Match(file, pattern);
                if (match.Success)
                {
                    countNumbers.Add(int.Parse(match.Groups[1].Value));
                }
            }
            var maxNumber = countNumbers.Count == 0 ? 0 : countNumbers.Max();
            fileName = $"{directory}\\{noExtName} ({maxNumber + 1}).pdf";

            return fileName;
        }
    }
}