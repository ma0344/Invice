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
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection;
using System.Diagnostics;
using PdfSharp.Quality;

namespace Invoice
{
    public enum TypeOfID
    {
        Instans = 0,
        Customer = 1,
        Invoice = 2,
        Payment = 3,
        Deposit = 4
    }

    // T_BALANCE テーブルに対応するクラス
    public class BalanceClass
    {
        public int BalanceId { get; set; }
        public int CustomerId { get; set; }
        public int? InvoiceId { get; set; }
        public int? PaymentId { get; set; }
        public int? DepositId { get; set; }
        public string SlipNumber { get; set; }
        public int DebOrCreId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int TransactionTypeId { get; set; }
        public int TransactionAmount { get; set; }
        public string CustomerName { get; set;}
        public string TransactionTypeName { get; set;}
        public string DateString => TransactionDate.ToShortDateString();
        public decimal? DebitAmount => DebOrCreId == 1 ? TransactionAmount : (decimal?)null;
        public decimal? CreditAmount => DebOrCreId == 2 ? TransactionAmount : (decimal?)null;

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
                    InvoiceId = reader.IsDBNull("INVOICE_ID") ? null : reader.GetInt32("INVOICE_ID"),
                    PaymentId = reader.IsDBNull("PAYMENT_ID") ? null : reader.GetInt32("PAYMENT_ID"),
                    DepositId = reader.IsDBNull("DEPOSIT_ID") ? null : reader.GetInt32("DEPOSIT_ID"),
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

        public static int GetBalanceById(TypeOfID type, int id)
        {
            using var command = QueryBuilder.CommandBuilder("SELECT *", "T_BALANCE", type, id);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return reader.GetInt32("BALANCE_ID");
            }
            return 0;
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
                    InvoiceId = reader.IsDBNull("INVOICE_ID") ? null : reader.GetInt32("INVOICE_ID"),
                    PaymentId = reader.IsDBNull("PAYMENT_ID") ? null : reader.GetInt32("PAYMENT_ID"),
                    DepositId = reader.IsDBNull("DEPOSIT_ID") ? null : reader.GetInt32("DEPOSIT_ID"),
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
        
        public static int GetBalanceByCustomerIdUntilDate(int customerId, DateTime date)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "SELECT SUM(TRANSACTION_AMOUNT) FROM T_BALANCE WHERE CUSTOMER_ID = @CustomerId AND TRANSACTION_DATE <= @Date";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", customerId);
            command.Parameters.AddWithValue("@Date", date);
            return Convert.ToInt32(command.ExecuteScalar());
        }
        // 新しいレコードを追加
        public static bool TryAddBalance(object obj)
        {
            try
            {
                var balance = new BalanceClass();
                if (obj is PaymentClass payment)
                {
                    balance.CustomerId = payment.CustomerId;
                    balance.InvoiceId = payment.InvoiceId;
                    balance.PaymentId = payment.PaymentId;
                    balance.DepositId = payment.DepositId;
                    balance.DebOrCreId = 2;
                    balance.SlipNumber = payment.SlipNumber;
                    balance.TransactionDate = payment.PaymentDate;
                    balance.TransactionTypeId = payment.TransactionTypeId;
                    balance.TransactionAmount = payment.PaymentAmount;
                }
                else if (obj is InvoiceClass invoice)
                {
                    balance.CustomerId = invoice.CustomerId;
                    balance.InvoiceId = invoice.InvoiceId;
                    balance.PaymentId = null;
                    balance.DepositId = null;
                    balance.DebOrCreId = 1;
                    balance.SlipNumber = invoice.SlipNumber;
                    balance.TransactionDate = invoice.IssueDate ?? DateTime.Now;
                    balance.TransactionTypeId = invoice.TransactionTypeId ?? 0;
                    balance.TransactionAmount = invoice.InvoiceTotal ?? 0;
                }
                else if (obj is DepositClass deposit)
                {
                    balance.CustomerId = deposit.CustomerId;
                    balance.InvoiceId = deposit.InvoiceId;
                    balance.PaymentId = deposit.PaymentId;
                    balance.DepositId = deposit.DepositId;
                    balance.DebOrCreId = deposit.DebOrCreId;
                    balance.SlipNumber = deposit.SlipNumber;
                    balance.TransactionDate = deposit.DepositDate;
                    balance.TransactionTypeId = 2;
                    balance.TransactionAmount = deposit.DepositAmount;
                }
                else
                {
                    throw new ArgumentException("Invalid object type");
                }
                    AddBalance(balance);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }
        public static void AddBalance(BalanceClass balance)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"INSERT INTO T_BALANCE (CUSTOMER_ID, INVOICE_ID, PAYMENT_ID, DEPOSIT_ID, SLIP_NUMBER, DEBIT_OR_CREDIT_ID, TRANSACTION_DATE, TRANSACTION_TYPE_ID, TRANSACTION_AMOUNT) VALUES (@CustomerId, @InvoiceId, @PaymentId, @DepositId, @SlipNumber, @DebOrCreId, @TransactionDate, @TransactionTypeId, @TransactionAmount)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", balance.CustomerId);
            command.Parameters.AddWithValue("@InvoiceId", balance.InvoiceId);
            command.Parameters.AddWithValue("@PaymentId", balance.PaymentId);
            command.Parameters.AddWithValue("@DepositId", balance.DepositId);
            command.Parameters.AddWithValue("@DebOrCreId", balance.DebOrCreId);
            command.Parameters.AddWithValue("@SlipNumber", balance.SlipNumber);
            command.Parameters.AddWithValue("@TransactionDate", balance.TransactionDate);
            command.Parameters.AddWithValue("@TransactionTypeId", balance.TransactionTypeId);
            command.Parameters.AddWithValue("@TransactionAmount", balance.TransactionAmount);
            command.ExecuteNonQuery();
            balance.BalanceId = (int)command.LastInsertedId;
        }

        // レコードを更新
        public void UpdateBalance()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"UPDATE T_BALANCE SET CUSTOMER_ID = @CustomerId, INVOICE_ID = @InvoiceId, PAYMENT_ID = @PaymentId, DEPOSIT_ID = @DepositId, DEBIT_OR_CREDIT_ID = @DebOrCreId, SLIP_NUMBER = @SlipNumber, TRANSACTION_DATE = @TransactionDate, TRANSACTION_TYPE_ID = @TransactionTypeId, TRANSACTION_AMOUNT = @TransactionAmount WHERE BALANCE_ID = @BalanceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@PaymentId", PaymentId);
            command.Parameters.AddWithValue("@DepositId", DepositId);
            command.Parameters.AddWithValue("@DebOrCreId", DebOrCreId);
            command.Parameters.AddWithValue("@TransactionDate", TransactionDate);
            command.Parameters.AddWithValue("@TransactionTypeId", TransactionTypeId);
            command.Parameters.AddWithValue("@TransactionAmount", TransactionAmount);
            command.Parameters.AddWithValue("@BalanceId", BalanceId);
            command.ExecuteNonQuery();
        }

        public static void TryUpdateBalance(object? obj)
        {
            try
            {
                var balance = new BalanceClass();
                if (obj is PaymentClass payment)
                {
                    balance.BalanceId = BalanceClass.GetBalanceById(TypeOfID.Payment, payment.PaymentId);
                    balance.CustomerId = payment.CustomerId;
                    balance.InvoiceId = payment.InvoiceId;
                    balance.PaymentId = payment.PaymentId;
                    balance.DepositId = payment.DepositId;
                    balance.DebOrCreId = 2;
                    balance.SlipNumber = payment.SlipNumber;
                    balance.TransactionDate = payment.PaymentDate;
                    balance.TransactionTypeId = 2;
                    balance.TransactionAmount = payment.PaymentAmount;
                }
                else if (obj is InvoiceClass invoice)
                {
                    balance.BalanceId = BalanceClass.GetBalanceById(TypeOfID.Invoice, invoice.InvoiceId);
                    balance.CustomerId = invoice.CustomerId;
                    balance.InvoiceId = invoice.InvoiceId;
                    balance.PaymentId = null;
                    balance.DepositId = null;
                    balance.DebOrCreId = 1;
                    balance.SlipNumber = invoice.SlipNumber;
                    balance.TransactionDate = invoice.IssueDate ?? DateTime.Now;
                    balance.TransactionTypeId = 1;
                    balance.TransactionAmount = invoice.InvoiceTotal ?? 0;
                }
                else if (obj is DepositClass deposit)
                {
                    balance.BalanceId = BalanceClass.GetBalanceById(TypeOfID.Deposit, deposit.DepositId);
                    balance.CustomerId = deposit.CustomerId;
                    balance.InvoiceId = deposit.InvoiceId;
                    balance.PaymentId = deposit.PaymentId;
                    balance.DepositId = deposit.DepositId;
                    balance.DebOrCreId = deposit.DebOrCreId;
                    balance.SlipNumber = deposit.SlipNumber;
                    balance.TransactionDate = deposit.DepositDate;
                    balance.TransactionTypeId = 2;
                    balance.TransactionAmount = deposit.DepositAmount;
                }
                else
                {
                    throw new ArgumentException("Invalid object type");
                }
                UpdateBalance(balance);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static void UpdateBalance(BalanceClass balance)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = @"UPDATE T_BALANCE SET CUSTOMER_ID = @CustomerId, INVOICE_ID = @InvoiceId, PAYMENT_ID = @PaymentId, DEPOSIT_ID = @DepositId, DEBIT_OR_CREDIT_ID = @DebOrCreId, SLIP_NUMBER = @SlipNumber, TRANSACTION_DATE = @TransactionDate, TRANSACTION_TYPE_ID = @TransactionTypeId, TRANSACTION_AMOUNT = @TransactionAmount WHERE BALANCE_ID = @BalanceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", balance.CustomerId);
            command.Parameters.AddWithValue("@InvoiceId", balance.InvoiceId);
            command.Parameters.AddWithValue("@PaymentId", balance.PaymentId);
            command.Parameters.AddWithValue("@DepositId", balance.DepositId);
            command.Parameters.AddWithValue("@DebOrCreId", balance.DebOrCreId);
            command.Parameters.AddWithValue("@SlipNumber", balance.SlipNumber);
            command.Parameters.AddWithValue("@TransactionDate", balance.TransactionDate);
            command.Parameters.AddWithValue("@TransactionTypeId", balance.TransactionTypeId);
            command.Parameters.AddWithValue("@TransactionAmount", balance.TransactionAmount);
            command.Parameters.AddWithValue("@BalanceId", balance.BalanceId);
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

        public static void DeleteBalanceById(TypeOfID type, int id)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = QueryBuilder.StringBuilder(command:"DELETE", tableName:"T_BALANCE", type);
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
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

    public class BalanceFilterParam
    {
        public int? BalanceId { get; set; } = null;
        public int? CustomerId { get; set; } = null;
        public int? InvoiceId { get; set; } = null;
        public int? PaymentId { get; set; } = null;
        public int? DepositId { get; set; } = null;
        public string? SlipNumber { get; set; } = null;
        public int? DebOrCreId { get; set; } = null;
        public DateTime? TransactionDate { get; set; } = null;
        public int? TransactionTypeId { get; set; } = null;
        public int? TransactionAmount { get; set; } = null;
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

    public class DefaultItemsClass : ItemClass, INotifyPropertyChanged
    {
        public int DefaultItemsId { get; set; }

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
        // ItemName
        // UnitPrice
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
        // ItemSubTotal
        private int _ItemSubTotal = 0;
        public int ItemSubTotal
        {
            get => _ItemSubTotal;
            set
            {
                _ItemSubTotal = UnitPrice * Quantity;
                OnPropertyChanged(nameof(ItemSubTotal));
            }
        }

        // TaxTypeId
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
                    if (_selectedTax != null && TaxTypeId != _selectedTax.TaxTypeId)
                    {
                        TaxTypeId = _selectedTax.TaxTypeId;
                        OnPropertyChanged(nameof(TaxTypeId));
                        TaxTypeName = _selectedTax.TaxTypeName;
                    }
                    ReTotal();
                }
            }
        }

        // TaxTypeId
        // Tax
        private int _Tax = 0;
        public int Tax
        {
            get => _Tax;
            set
            {
                if (_selectedTax == null) return;
                _Tax = (int)Math.Round(_selectedTax.TaxRate * _ItemSubTotal, 0, MidpointRounding.ToEven);
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
                _ItemTotal = _ItemSubTotal + Tax;
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
        public void ReTotal(DefaultItemsClass item)
        {
            item.ItemSubTotal = item.Quantity * item.UnitPrice;
            item.Tax = (int)(item.Quantity * TaxTypeClass.GetTaxes().FirstOrDefault(t => t.TaxTypeId == item.TaxTypeId)?.TaxRate ?? 0);
            item.ItemTotal = item.ItemSubTotal + item.Tax;
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

        public static List<DefaultItemsClass> GetDefaultItems()
        {
            var items = new List<DefaultItemsClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_DEFAULT_ITEMS", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new DefaultItemsClass
                {
                    DefaultItemsId = reader.GetInt32("DEFAULT_ITEMS_ID"),
                    ItemOrder = reader.GetInt32("ITEM_ORDER"),
                    ItemId = reader.GetInt32("ITEM_ID"),
                    UnitPrice = reader.GetInt32("UNIT_PRICE"),
                    Quantity = reader.GetInt32("QUANTITY"),
                    Unit = reader.GetString("UNIT"),
                    TaxTypeId = reader.GetInt32("TAX_TYPE_ID")
                };
                items.Add(item);
            }
            return items;
        }

        public InvoiceItemClass ToInvoiceItem()
        {
            var invoiceItem = new InvoiceItemClass();
            invoiceItem.ItemOrder = ItemOrder;
            invoiceItem.ItemId = ItemId;
            invoiceItem.ItemName = ItemName;
            invoiceItem.UnitPrice = UnitPrice;
            invoiceItem.Quantity = Quantity;
            invoiceItem.Unit = Unit;
            invoiceItem.TaxTypeId = TaxTypeId;
            invoiceItem.TaxTypeName = TaxTypeName;
            invoiceItem.ItemSubTotal = ItemSubTotal;
            invoiceItem.Tax = Tax;
            invoiceItem.ItemTotal = ItemTotal;
            return invoiceItem;
        }

        public DefaultItemsClass Copy()
        {
            return (DefaultItemsClass)this.MemberwiseClone();
        }
        public static void AddDefaultItems(List<DefaultItemsClass> items)
        {
            CrearDefaultitemsTable();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            foreach (var item in items)
            {
                using var command = new MySqlCommand(
                    "INSERT INTO T_DEFAULT_ITEMS (ITEM_ORDER, ITEM_ID, UNIT_PRICE, QUANTITY, UNIT, TAX_TYPE_ID) " +
                    "VALUES (@itemOrder, @itemId, @unitPrice, @quantity, @unit, @taxTypeId)", connection);
                command.Parameters.AddWithValue("@itemOrder", item.ItemOrder);
                command.Parameters.AddWithValue("@itemId", item.ItemId);
                command.Parameters.AddWithValue("@unitPrice", item.UnitPrice);
                command.Parameters.AddWithValue("@quantity", item.Quantity);
                command.Parameters.AddWithValue("@unit", item.Unit);
                command.Parameters.AddWithValue("@taxTypeId", item.TaxTypeId);
                command.ExecuteNonQuery();
            }
        }

        public void AddDefaultItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand(
                "INSERT INTO T_DEFAULT_ITEMS (ITEM_ORDER, ITEM_ID, UNIT_PRICE, QUANTITY, UNIT, TAX_TYPE_ID) " +
                "VALUES (@itemOrder, @itemId, @unitPrice, @quantity, @unit, @taxTypeId)", connection);
            command.Parameters.AddWithValue("@itemOrder", ItemOrder);
            command.Parameters.AddWithValue("@itemId", ItemId);
            command.Parameters.AddWithValue("@unitPrice", UnitPrice);
            command.Parameters.AddWithValue("@quantity", Quantity);
            command.Parameters.AddWithValue("@unit", Unit);
            command.Parameters.AddWithValue("@taxTypeId", TaxTypeId);
            command.ExecuteNonQuery();
            DefaultItemsId = (int)command.LastInsertedId;
        }

        public void UpdateDefaultItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand(
                "UPDATE T_DEFAULT_ITEMS SET ITEM_ORDER=@itemOrder, ITEM_ID=@itemId, UNIT_PRICE=@unitPrice, QUANTITY=@quantity, UNIT=@unit, TAX_TYPE_ID=@taxTypeId WHERE DEFAULT_ITEMS_ID=@defaultItemsId", connection);
            command.Parameters.AddWithValue("@itemOrder", ItemOrder);
            command.Parameters.AddWithValue("@itemId", ItemId);
            command.Parameters.AddWithValue("@unitPrice", UnitPrice);
            command.Parameters.AddWithValue("@quantity", Quantity);
            command.Parameters.AddWithValue("@unit", Unit);
            command.Parameters.AddWithValue("@taxTypeId", TaxTypeId);
            command.Parameters.AddWithValue("@defaultItemsId", DefaultItemsId);
            command.ExecuteNonQuery();
        }

        public static void CrearDefaultitemsTable()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("TRUNCATE TABLE T_DEFAULT_ITEMS", connection);
            command.ExecuteNonQuery();
        }

        public void DeleteDefaultItem()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand(
                "DELETE FROM T_DEFAULT_ITEMS WHERE DEFAULT_ITEMS_ID=@defaultItemsId", connection);
            command.Parameters.AddWithValue("@defaultItemsId", DefaultItemsId);
            command.ExecuteNonQuery();
        }
    }

    public static class QueryBuilder
    {
        public static string StringBuilder(string command = "SELECT *", string tableName ="", TypeOfID type = 0)
        {
            string query = $"{command} FROM {tableName}";
            switch (type)
            {
                case TypeOfID.Customer:
                    query += " WHERE CUSTOMER_ID = @id";
                    break;
                case TypeOfID.Invoice:
                    query += " WHERE INVOICE_ID = @id";
                    break;
                case TypeOfID.Payment:
                    query += " WHERE PAYMENT_ID = @id";
                    break;
                case TypeOfID.Deposit:
                    query += " WHERE DEPOSIT_ID = @id";
                    break;
                default:
                    break;
            }
            return query;
        }
        public static MySqlCommand CommandBuilder(string command = "SELECT *", string tableName = "", TypeOfID type = 0, int id = 0)
        {
            string query = StringBuilder(command, tableName, type);
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            var connection = new MySqlConnection(connectionString);
            connection.Open();
            var cmd = new MySqlCommand(query, connection);
            if (type != 0)
            {
                cmd.Parameters.AddWithValue("@id", id);
            }
            return cmd;
        }
    }

    // T_DEPOSIT テーブルに対応するクラス
    public class DepositClass
    {
        public int DepositId {get; set;}
        public int? InvoiceId {get; set;}
        public int? PaymentId {get; set;}
        public int CustomerId {get; set;}
        public DateTime DepositDate {get; set;}
        public int DepositAmount {get; set;}
        public string SlipNumber {get; set;}
        public int DebOrCreId {get; set;}

        public List<DepositClass> GetDepsits(TypeOfID type = 0, int id = 0)
        {
            var deposits = new List<DepositClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = QueryBuilder.StringBuilder(command
                :"SELECT *",tableName:"T_DEPOSIT", type);
            using var command = new MySqlCommand(query, connection);
            if (type != 0)
            {
                command.Parameters.AddWithValue("@id", id);
            }
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var deposit = new DepositClass
                {
                    DepositId = reader.GetInt32("DEPOSIT_ID"),
                    InvoiceId = reader.IsDBNull("INVOICE_ID") ? null : reader.GetInt32("INVOICE_ID"),
                    PaymentId = reader.IsDBNull("PAYMENT_ID") ? null : reader.GetInt32("PAYMENT_ID"),
                    CustomerId = reader.GetInt32("CUSTOMER_ID"),
                    DepositDate = reader.GetDateTime("DEPOSIT_DATE"),
                    DepositAmount = reader.GetInt32("DEPOSIT_AMOUNT"),
                    SlipNumber = reader.GetString("SLIP_NUMBER"),
                    DebOrCreId = reader.GetInt32("DEBIT_OR_CREDIT_ID")
                };
                deposits.Add(deposit);
            }
            return deposits;
        }

        public static DepositClass GetDeposit(TypeOfID type = 0, int? id = 0)
        {
            if (id == null) return null;
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            var query = QueryBuilder.StringBuilder("SELECT *", "T_DEPOSIT", type);
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new DepositClass
                {
                    DepositId = reader.GetInt32("DEPOSIT_ID"),
                    InvoiceId = reader.IsDBNull("INVOICE_ID") ? null : reader.GetInt32("INVOICE_ID"),
                    PaymentId = reader.IsDBNull("PAYMENT_ID") ? null : reader.GetInt32("PAYMENT_ID"),
                    CustomerId = reader.GetInt32("CUSTOMER_ID"),
                    DepositDate = reader.GetDateTime("DEPOSIT_DATE"),
                    DepositAmount = reader.GetInt32("DEPOSIT_AMOUNT"),
                    SlipNumber = reader.GetString("SLIP_NUMBER"),
                    DebOrCreId = reader.GetInt32("DEBIT_OR_CREDIT_ID")
                };
            }
            return null;
        }

        public static int? TryAddDeposit(object? obj = null)
        {
            try
            {
                DepositClass deposit = new();
                if (obj is PaymentClass payment)
                {
                    deposit.InvoiceId = null;
                    deposit.PaymentId = payment.PaymentId;
                    deposit.CustomerId = payment.CustomerId;
                    deposit.DepositDate = payment.PaymentDate;
                    deposit.DepositAmount = payment.PaymentAmount;
                    deposit.SlipNumber = payment.SlipNumber;
                    deposit.DebOrCreId = 2;
                }
                else if (obj is InvoiceClass invoice)
                {
                    deposit.InvoiceId = invoice.InvoiceId;
                    deposit.PaymentId = null;
                    deposit.CustomerId = invoice.CustomerId;
                    deposit.DepositDate = invoice.IssueDate ?? DateTime.Now;
                    deposit.DepositAmount = invoice.PaydByDeposit;
                    deposit.SlipNumber = invoice.SlipNumber;
                    deposit.DebOrCreId = 1;
                }
                if(deposit.DepositAmount > 0) AddDeposit(deposit);

                BalanceClass.TryAddBalance(deposit);
                return deposit.DepositId;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
        
        public static int AddDeposit(DepositClass deposit = null)
        {
            if (deposit == null)
            {
                deposit = new DepositClass();
            }
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("INSERT INTO T_DEPOSIT (INVOICE_ID, PAYMENT_ID, CUSTOMER_ID, DEPOSIT_DATE, DEPOSIT_AMOUNT, SLIP_NUMBER, DEBIT_OR_CREDIT_ID) VALUES (@InvoiceId, @PaymentId, @CustomerId, @DepositDate, @DepositAmount, @SlipNumber, @DebitOrCreditId)", connection);
            command.Parameters.AddWithValue("@InvoiceId", deposit.InvoiceId);
            command.Parameters.AddWithValue("@PaymentId", deposit.PaymentId);
            command.Parameters.AddWithValue("@CustomerId", deposit.CustomerId);
            command.Parameters.AddWithValue("@DepositDate", deposit.DepositDate);
            command.Parameters.AddWithValue("@DepositAmount", deposit.DepositAmount);
            command.Parameters.AddWithValue("@SlipNumber", deposit.SlipNumber);
            command.Parameters.AddWithValue("@DebitOrCreditId", deposit.DebOrCreId);
            command.ExecuteNonQuery();
            deposit.DepositId = (int)command.LastInsertedId;
            return deposit.DepositId;
        }

        public static bool TryUpdateDeposit(object? obj)
        {
            try
            {
                var deposit = UpdateDeposit(obj);
                BalanceClass.TryUpdateBalance(deposit);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public static DepositClass UpdateDeposit(object? obj)
        {
            var deposit = new DepositClass();
            if (obj is PaymentClass payment)
            {
                deposit = DepositClass.GetDeposit(TypeOfID.Payment, payment.PaymentId);
                //deposit.InvoiceId = null;
                //deposit.PaymentId = payment.PaymentId;
                deposit.CustomerId = payment.CustomerId;
                deposit.DepositDate = payment.PaymentDate;
                deposit.DepositAmount = payment.PaymentAmount;
                deposit.SlipNumber = payment.SlipNumber;
                deposit.DebOrCreId = 2;
                //deposit.DepositId = payment.DepositId ?? 0;
            }
            else if (obj is InvoiceClass invoice)
            {
                deposit = GetDeposit(TypeOfID.Invoice, invoice.InvoiceId);
                //deposit.InvoiceId = invoice.InvoiceId;
                //deposit.PaymentId = null;
                deposit.CustomerId = invoice.CustomerId;
                deposit.DepositDate = invoice.PaymentDate ?? DateTime.Now;
                deposit.DepositAmount = invoice.PaydByDeposit;
                deposit.SlipNumber = invoice.SlipNumber;
                deposit.DebOrCreId = 1;
            }

            var whereClause = obj is PaymentClass ? "PAYMENT_ID" : "INVOICE_ID";
            var id = obj is PaymentClass ? deposit.PaymentId : deposit.InvoiceId;
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand($"UPDATE T_DEPOSIT SET INVOICE_ID=@InvoiceId, PAYMENT_ID=@PaymentId, CUSTOMER_ID=@CustomerId, DEPOSIT_DATE=@DepositDate, DEPOSIT_AMOUNT=@DepositAmount, SLIP_NUMBER=@SlipNumber, DEBIT_OR_CREDIT_ID=@DebitOrCreditId WHERE {whereClause}=@Id", connection);
            command.Parameters.AddWithValue("@InvoiceId", deposit.InvoiceId);
            command.Parameters.AddWithValue("@PaymentId", deposit.PaymentId);
            command.Parameters.AddWithValue("@CustomerId", deposit.CustomerId);
            command.Parameters.AddWithValue("@DepositDate", deposit.DepositDate);
            command.Parameters.AddWithValue("@DepositAmount", deposit.DepositAmount);
            command.Parameters.AddWithValue("@SlipNumber", deposit.SlipNumber);
            command.Parameters.AddWithValue("@DebitOrCreditId", deposit.DebOrCreId);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
            return deposit;
        }

        public static void DeleteDepositById(TypeOfID type, int? id)
        {
            if (id == null) return;
            using var command = QueryBuilder.CommandBuilder("DELETE", "T_DEPOSIT", type, (int)id);
            command.ExecuteNonQuery();
            BalanceClass.DeleteBalanceById(type, (int)id);

        }

    }

    // T_INVOICE テーブルに対応するクラス
    public class InvoiceClass : INotifyPropertyChanged
    {
        private int _InvoiceId = 0;
        public int InvoiceId 
        {
            get
            {
                return _InvoiceId;            
            }
            set
            {
                _InvoiceId = value;
                OnPropertyChanged(nameof(InvoiceId));
            }
        }
        
        private int _CustomerId = 0;
        public int CustomerId 
        {
            get
            {
                return _CustomerId;            
            }
            set
            {
                _CustomerId = value;
                OnPropertyChanged(nameof(CustomerId));
            }
        }
        
        private DateTime? _IssueDate = DateTime.Now;
        public DateTime? IssueDate 
        {
            get
            {
                return _IssueDate;            
            }
            set
            {
                _IssueDate = value;
                OnPropertyChanged(nameof(IssueDate));
            }
        }
        
        private DateTime? _DueDate = DateTime.Now;
        public DateTime? DueDate 
        {
            get
            {
                return _DueDate;            
            }
            set
            {
                _DueDate = value;
                OnPropertyChanged(nameof(DueDate));
            }
        }
        
        private string? _Subject = "";
        public string? Subject 
        {
            get
            {
                return _Subject;            
            }
            set
            {
                _Subject = value;
                OnPropertyChanged(nameof(Subject));
            }
        }
        
        private string? _SlipNumber = "";
        public string? SlipNumber 
        {
            get
            {
                return _SlipNumber;            
            }
            set
            {
                _SlipNumber = value;
                OnPropertyChanged(nameof(SlipNumber));
            }
        }
        
        private int? _SubTotal = 0;
        public int? SubTotal 
        {
            get
            {
                return _SubTotal;            
            }
            set
            {
                _SubTotal = value;
                OnPropertyChanged(nameof(SubTotal));
            }
        }
        
        private int? _Tax = 0;
        public int? Tax 
        {
            get
            {
                return _Tax;            
            }
            set
            {
                _Tax = value;
                OnPropertyChanged(nameof(Tax));
            }
        }
        
        private int? _InvoiceTotal = 0;
        public int? InvoiceTotal 
        {
            get
            {
                return _InvoiceTotal;            
            }
            set
            {
                _InvoiceTotal = value;
                OnPropertyChanged(nameof(InvoiceTotal));
            }
        }
        

        private int _ItemsTotal = 0;
        public int ItemsTotal 
        {
            get
            {
                return _ItemsTotal;            
            }
            set
            {
                _ItemsTotal = value;
                OnPropertyChanged(nameof(ItemsTotal));
            }
        }
        
        private int _PaydByDeposit = 0;
        public int PaydByDeposit 
        {
            get
            {
                return _PaydByDeposit;            
            }
            set
            {
                _PaydByDeposit = value;
                OnPropertyChanged(nameof(PaydByDeposit));
            }
        }
        
        private string? _Message = "";
        public string? Message 
        {
            get
            {
                return _Message;            
            }
            set
            {
                _Message = value;
                OnPropertyChanged(nameof(Message));
            }
        }
        
        private int? _TransactionTypeId = 1;
        public int? TransactionTypeId 
        {
            get
            {
                return _TransactionTypeId;            
            }
            set
            {
                _TransactionTypeId = value;
                OnPropertyChanged(nameof(TransactionTypeId));
            }
        }
        
        private DateTime? _PaymentDate = DateTime.Now;
        public DateTime? PaymentDate 
        {
            get
            {
                return _PaymentDate;            
            }
            set
            {
                _PaymentDate = value;
                OnPropertyChanged(nameof(PaymentDate));
            }
        }
        
        private int _InvoiceStatusId = 0;
        public int InvoiceStatusId 
        {
            get
            {
                return _InvoiceStatusId;            
            }
            set
            {
                _InvoiceStatusId = value;
                OnPropertyChanged(nameof(InvoiceStatusId));
            }
        }
        
        private string _CustomerName = "";
        public string CustomerName 
        {
            get
            {
                return _CustomerName;            
            }
            set
            {
                _CustomerName = value;
                OnPropertyChanged(nameof(CustomerName));
            }
        }
        
        private string _InvoiceStatus = "";
        public string InvoiceStatus 
        {
            get
            {
                return _InvoiceStatus;            
            }
            set
            {
                _InvoiceStatus = value;
                OnPropertyChanged(nameof(InvoiceStatus));
            }
        }
        
        private string? _IssueDateString = "";
        public string? IssueDateString 
        {
            get
            {
                return _IssueDateString;
            }
            set
            {
                _IssueDateString = value;
                OnPropertyChanged(nameof(IssueDateString));
            }
        }
        
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
                invoice.IssueDate = reader.IsDBNull("ISSUE_DATE") ? null : reader.GetDateTime("ISSUE_DATE");
                invoice.DueDate = reader.IsDBNull("DUE_DATE") ? null : reader.GetDateTime("DUE_DATE");
                invoice.Subject = reader.IsDBNull("SUBJECT") ? null : reader.GetString("SUBJECT");
                invoice.SlipNumber = reader.IsDBNull("SLIP_NUMBER") ? null : reader.GetString("SLIP_NUMBER");
                invoice.ItemsTotal = reader.GetInt32("ITEMS_TOTAL");
                invoice.SubTotal = reader.IsDBNull("SUBTOTAL") ? null : reader.GetInt32("SUBTOTAL");
                invoice.Tax = reader.IsDBNull("TAX") ? null : reader.GetInt32("TAX");
                invoice.PaydByDeposit = reader.GetInt32("PAYD_BY_DEPOSIT");
                invoice.InvoiceTotal = reader.IsDBNull("TOTAL") ? null : reader.GetInt32("TOTAL");
                invoice.Message = reader.IsDBNull("MESSAGE") ? null : reader.GetString("MESSAGE");
                invoice.TransactionTypeId = reader.IsDBNull("TRANSACTION_TYPE_ID") ? null : reader.GetInt32("TRANSACTION_TYPE_ID");
                invoice.PaymentDate = reader.IsDBNull("PAYMENT_DATE") ? null : reader.GetDateTime("PAYMENT_DATE");
                invoice.InvoiceStatusId = reader.GetInt32("INVOICE_STATUS_ID");
                invoice.IssueDateString = invoice.IssueDate?.ToShortDateString() ?? null;

                invoices.Add(invoice);
            }
            return invoices;
        }

        public bool TryAddInvoice()
        {
            try
            {
                AddInvoice();
                InvoiceItemClass.AddInvoiceItems(InvoiceItems, InvoiceId);
                if (TransactionTypeId == 1)
                    BalanceClass.TryAddBalance(this);
                else if (TransactionTypeId == 2)
                {
                    PaymentDate = IssueDate;
                    DepositClass.TryAddDeposit(this);
                }
                else return false;
                
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
            string query = @"INSERT INTO T_INVOICE (CUSTOMER_ID, ISSUE_DATE, DUE_DATE, SUBJECT, SLIP_NUMBER, ITEMS_TOTAL, SUBTOTAL, TAX, PAYD_BY_DEPOSIT, TOTAL, MESSAGE, TRANSACTION_TYPE_ID, PAYMENT_DATE, INVOICE_STATUS_ID)
                             VALUES (@CustomerId, @IssueDate, @DueDate, @Subject, @SlipNumber, @ItemsTotal, @Subtotal, @Tax, @PaydByDeposit, @InvoiceTotal, @Message, @TransactionTypeId, @PaymentDate, @InvoiceStatusId)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@IssueDate", IssueDate);
            command.Parameters.AddWithValue("@DueDate", DueDate);
            command.Parameters.AddWithValue("@Subject", Subject);
            command.Parameters.AddWithValue("@SlipNumber", SlipNumber);
            command.Parameters.AddWithValue("@ItemsTotal", ItemsTotal);
            command.Parameters.AddWithValue("@Subtotal", SubTotal);
            command.Parameters.AddWithValue("@Tax", Tax);
            command.Parameters.AddWithValue("@PaydByDeposit", PaydByDeposit);
            command.Parameters.AddWithValue("@InvoiceTotal", InvoiceTotal);
            command.Parameters.AddWithValue("@Message", Message);
            command.Parameters.AddWithValue("@TransactionTypeId", TransactionTypeId);
            command.Parameters.AddWithValue("@PaymentDate", PaymentDate);
            command.Parameters.AddWithValue("@InvoiceStatusId", InvoiceStatusId);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.ExecuteNonQuery();
            InvoiceId = (int)command.LastInsertedId;

        }

        public bool TryUpdateInvoice()
        {
            try
            {
                UpdateInvoice();
                InvoiceItemClass.UpdateInvoiceItems(InvoiceItems, InvoiceId);
                //BalanceClass.TryAddBalance(this);
                if (TransactionTypeId == 1)
                    BalanceClass.TryUpdateBalance(this);
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
            string query = @"UPDATE T_INVOICE SET CUSTOMER_ID = @CustomerId, ISSUE_DATE = @IssueDate, DUE_DATE = @DueDate, SUBJECT = @Subject, SLIP_NUMBER = @SlipNumber, ITEMS_TOTAL = @ItemsTotal, SUBTOTAL = @Subtotal, TAX = @Tax, PAYD_BY_DEPOSIT = @PaydByDeposit, TOTAL = @InvoiceTotal, MESSAGE = @Message, TRANSACTION_TYPE_ID = @TransactionTypeId, PAYMENT_DATE = @PaymentDate, INVOICE_STATUS_ID = @InvoiceStatusId WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@IssueDate", IssueDate);
            command.Parameters.AddWithValue("@DueDate", DueDate);
            command.Parameters.AddWithValue("@Subject", Subject);
            command.Parameters.AddWithValue("@SlipNumber", SlipNumber);
            command.Parameters.AddWithValue("@ItemsTotal", ItemsTotal);
            command.Parameters.AddWithValue("@Subtotal", SubTotal);
            command.Parameters.AddWithValue("@Tax", Tax);
            command.Parameters.AddWithValue("@PaydByDeposit", PaydByDeposit);
            command.Parameters.AddWithValue("@InvoiceTotal", InvoiceTotal);
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


        public static void DeleteInvoiceByInvoiceId(int id)
        {
            InvoiceItemClass.DeleteInvoiceItemsByInvoiceId(id);
            using var connection = new MySqlConnection(ConnectionInfo.Builder.ConnectionString);
            connection.Open();
            string query = "DELETE FROM T_INVOICE WHERE INVOICE_ID = @InvoiceId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", id);
            command.ExecuteNonQuery();
            DepositClass.DeleteDepositById(TypeOfID.Invoice, id);
            BalanceClass.DeleteBalanceById(TypeOfID.Invoice, id);
        }

        public InvoiceClass DeepClone()
        {
            var newInvoice = new InvoiceClass
            {
                InvoiceId = this.InvoiceId,
                CustomerId = this.CustomerId,
                IssueDate = this.IssueDate,
                DueDate = this.DueDate,
                Subject = this.Subject,
                SlipNumber = this.SlipNumber,
                ItemsTotal = this.ItemsTotal,
                SubTotal = this.SubTotal,
                Tax = this.Tax,
                PaydByDeposit = this.PaydByDeposit,
                InvoiceTotal = this.InvoiceTotal,
                Message = this.Message,
                TransactionTypeId = this.TransactionTypeId,
                PaymentDate = this.PaymentDate,
                InvoiceStatusId = this.InvoiceStatusId,
                CustomerName = this.CustomerName,
                InvoiceStatus = this.InvoiceStatus,
                IssueDateString = this.IssueDateString,
                // 必要に応じて他のプロパティもコピー
            };
            newInvoice.InvoiceItems.AddRange(this.InvoiceItems);
            return newInvoice;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case "CustomerId":
                case "IssueDate":
                case "DueDate":
                case "Subject":
                case "SlipNumber":
                case "InvoiceTotal":
                case "ItemsTotal":
                case "PaydByDeposit":
                case "Message":
                case "TransactionTypeId":
                case "PaymentDate":
                case "InvoiceStatusId":
                case "InvoiceStatus":
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    break;
            }

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

        public static void AddInvoiceItems(List<InvoiceItemClass> ItemList, int invoiceId)
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

        public static void UpdateInvoiceItems(List<InvoiceItemClass> ItemList, int invoiceId)
        {
            DeleteInvoiceItemsByInvoiceId(invoiceId);
            AddInvoiceItems(ItemList, invoiceId);

        }

        public void UpdateInvoiceItem()
        {
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
        public void CheckRegisteredHandlers()
        {
            var eventField = typeof(InvoiceItemClass).GetField("PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            if (eventField != null)
            {
                var eventDelegate = eventField.GetValue(this) as MulticastDelegate;
                if (eventDelegate != null)
                {
                    foreach (var handler in eventDelegate.GetInvocationList())
                    {
                        Debug.WriteLine($"登録されているメソッド: {handler.Method.Name}");
                    }
                }
            }
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

        private int _TransactionTypeId;
        public int TransactionTypeId
        {
            get { return _TransactionTypeId; }
            set
            {
                if (_TransactionTypeId != value)
                {
                    _TransactionTypeId = value;
                    OnPropertyChanged(nameof(TransactionTypeId));
                }
            }
        }

        private int? _DepositId;
        public int? DepositId
        {
            get { return _DepositId; }
            set 
            {
                if (_DepositId != value)
                {
                    _DepositId = value;
                    OnPropertyChanged(nameof(DepositId));
                }
            }
        }

        private int _PaymentMethodId = 1;
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
                    DepositId = reader.IsDBNull("DEPOSIT_ID") ? null : reader.GetInt32("DEPOSIT_ID"),
                    TransactionTypeId = reader.GetInt32("TRANSACTION_TYPE_ID"),
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
                payment.DepositId = reader.IsDBNull("DEPOSIT_ID") ? null : reader.GetInt32("DEPOSIT_ID");
                payment.TransactionTypeId = reader.GetInt32("TRANSACTION_TYPE_ID");
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
                if(TransactionTypeId == 1)
                    BalanceClass.TryAddBalance(this);
                else if (TransactionTypeId == 2)
                    DepositClass.TryAddDeposit(this);
                else return false;
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
            string query = @"INSERT INTO T_PAYMENT (INVOICE_ID, DEPOSIT_ID, TRANSACTION_TYPE_ID, PAYMENT_METHOD_ID, CUSTOMER_ID, SLIP_NUMBER, PAYMENT_DATE, PAYMENT_AMOUNT, SUBJECT)
                             VALUES (@InvoiceId, @DepositId, @TransactionTypeId, @PaymentMethodId, @CustomerId, @SlipNumber, @PaymentDate, @PaymentAmount, @Subject)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@DepositId", DepositId);
            command.Parameters.AddWithValue("@TransactionTypeId", TransactionTypeId);
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
            string query = @"UPDATE T_PAYMENT SET INVOICE_ID = @InvoiceId, DEPOSIT_ID = @DepositId, TRANSACTION_TYPE_ID = @TransactionTypeId, PAYMENT_METHOD_ID = @PaymentMethodId, CUSTOMER_ID = @CustomerId, SLIP_NUMBER = @SlipNumber, PAYMENT_DATE = @PaymentDate, PAYMENT_AMOUNT = @PaymentAmount, SUBJECT = @Subject WHERE PAYMENT_ID = @PaymentId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@DepositId", DepositId);
            command.Parameters.AddWithValue("@TransactionTypeId", TransactionTypeId);
            command.Parameters.AddWithValue("@PaymentMethodId", PaymentMethodId);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@SlipNumber", SlipNumber);
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

        public static bool TryDeletePaymentById(TypeOfID type, int paymentId)
        {
            try
            {
                DeletePaymentById(type, paymentId);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public static void DeletePaymentById(TypeOfID type, int id)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "DELETE FROM T_PAYMENT WHERE PAYMENT_ID = @PaymentId";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@PaymentId", id);
            command.ExecuteNonQuery();
        }


        public static void ClearInvoiceIdFromPayment(int invoiceId)
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = "UPDATE T_PAYMENT SET InvoiceId = NULL WHERE INVOICE_ID = @InvoiceId";
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
    //public class _PaymentMethodClass
    //{
    //    public int PaymentMethodId { get; set; }
    //    public string MethodName { get; set; }
    //    public int DebitOrCreditId { get; set; }

    //    public static List<PaymentMethodClass> GetPaymentMethods()
    //    {
    //        var paymentMethods = new List<PaymentMethodClass>();
    //        string connenctionString = ConnectionInfo.Builder.ConnectionString;
    //        using var connection = new MySqlConnection(connenctionString);
    //        connection.Open();
    //        using var command = new MySqlCommand("SELECT * FROM T_PAYMENT_METHOD", connection);
    //        using var reader = command.ExecuteReader();
    //        while (reader.Read())
    //        {
    //            var method = new PaymentMethodClass
    //            {
    //                PaymentMethodId = reader.GetInt32("PAYMENT_METHOD_ID"),
    //                MethodName = reader.GetString("METHOD_NAME"),
    //                DebitOrCreditId = reader.GetInt32("DEBIT_OR_CREDIT_ID")
    //            };
    //            paymentMethods.Add(method);
    //        }
    //        return paymentMethods;
    //    }
    //    public static int AddPaymentMethod(PaymentMethodClass methodClass)
    //    {
    //        string connectionString = ConnectionInfo.Builder.ConnectionString;
    //        using var connection = new MySqlConnection(connectionString);
    //        connection.Open();
    //        using var command = new MySqlCommand("INSERT INTO T_PAYMENT_METHOD (METHOD_NAME, DEBIT_OR_CREDIT_ID) VALUES (@methodName, @debitOrCreditId)", connection);
    //        command.Parameters.AddWithValue("@methodName", methodClass.MethodName);
    //        command.Parameters.AddWithValue("@debitOrCreditId", 2);
    //        command.ExecuteNonQuery();
    //        return (int)command.LastInsertedId;
    //    }
    //    public void UpdatePaymentMethod()
    //    {
    //        string connectionString = ConnectionInfo.Builder.ConnectionString;
    //        using var connection = new MySqlConnection(connectionString);
    //        connection.Open();
    //        using var command = new MySqlCommand("UPDATE T_PAYMENT_METHOD SET METHOD_NAME=@methodName WHERE PAYMENT_METHOD_ID=@methodId", connection);
    //        command.Parameters.AddWithValue("@methodName", MethodName);
    //        command.Parameters.AddWithValue("@methodId", PaymentMethodId);
    //        command.Parameters.AddWithValue("@debitOrCreditId", 2);
    //        command.ExecuteNonQuery();
    //    }

    //    public void DeletePaymentMethod()
    //    {
    //        string connectionString = ConnectionInfo.Builder.ConnectionString;
    //        using var connection = new MySqlConnection(connectionString);
    //        connection.Open();
    //        using var command = new MySqlCommand("DELETE FROM T_PAYMENT_METHOD WHERE PAYMENT_METHOD_ID=@methodId", connection);
    //        command.Parameters.AddWithValue("@methodId", PaymentMethodId);
    //        command.ExecuteNonQuery();
    //    }
    //}

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
        public int? TransactionTypeId { get; set; } = null;
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
            string baseName = $"請求書_{invoice.DueDate?.ToString("yyyyMM")}_{invoice.Subject}_{invoice.CustomerName}.pdf";
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