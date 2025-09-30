using MigraDoc.DocumentObjectModel.Tables;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Invoice.Classes
{
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

    // T_INVOICE_STATUS テーブルに対応するクラス
    public class InvoiceStatusClass : INotifyPropertyChanged, ILoggable
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
                if (_InvoiceStatusId != value)
                {
                    _InvoiceStatusId = value;
                    OnPropertyChanged(nameof(InvoiceStatusId));
                }

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

    public class CompanyInfo : ILoggable
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
    
    // T_TAX_TYPE テーブルに対応するクラス
    public class TaxTypeClass : INotifyPropertyChanged, ILoggable
    {
        private static Lazy<List<TaxTypeClass>> _lazyTaxTypes = new Lazy<List<TaxTypeClass>>(() =>
        {
            return GetTaxes();
        });
        public static List<TaxTypeClass> TaxTypes = _lazyTaxTypes.Value;

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
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
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
            using var command = new MySqlCommand("INSERT INTO T_TAX_TYPE (TAX_TYPE_NAME, TAX_RATE) " + "\r\n" + "VALUES (@name, @rate)", connection);
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

            return GetTaxes().FirstOrDefault(t => t.TaxTypeId == taxTypeId)?.TaxTypeName ?? "";
        }

        public TaxTypeClass GetTaxTypeClassByID(int taxTypeId)
        {

            return GetTaxes().FirstOrDefault(t => t.TaxTypeId == taxTypeId);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {


            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    // T_TRANSACTION_TYPE テーブルに対応するクラス
    public class TransactionTypeClass : ILoggable
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
            using var command = new MySqlCommand("INSERT INTO T_TRANSACTION_TYPE (TRANSACTION_NAME, DEBIT_OR_CREDIT_ID) " + "\r\n" + "VALUES (@name, @debitOrCreditId)", connection);
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

}
