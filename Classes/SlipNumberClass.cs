using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice.Classes
{

    // T_SLIP_NUMBER_INFO テーブルに対応するクラス
    public class SlipNumberClass : INotifyPropertyChanged, ILoggable
    {
        private int _SlipNumberID = 0;
        public int SlipNumberID
        {
            get => _SlipNumberID;
            set
            {
                _SlipNumberID = value;
                OnPropertyChanged(nameof(SlipNumberID));
            }
        }

        private DateTime _SlipTerm;
        public DateTime SlipTerm
        {
            get { return _SlipTerm; }
            set { _SlipTerm = new DateTime(value.Year, value.Month, 1); }
        }
        private string _InvoicePrefix = "I";
        public string InvoicePrefix { get => _InvoicePrefix; set => _InvoicePrefix = value; }

        public string InvoiceNumber
        {
            get => $"{InvoicePrefix}{_SlipTerm:yyMM_}{InvoiceLatest:0000}";
        }

        private string _ReceiptPrefix = "R";
        public string ReceiptPrefix { get => _ReceiptPrefix; set => _ReceiptPrefix = value; }

        public string ReceiptNumber
        {
            get => $"{ReceiptPrefix}{_SlipTerm:yyMM_}{ReceiptLatest:0000}";
        }

        private int _InvoiceLatest = 1;
        public int InvoiceLatest
        {
            get => _InvoiceLatest;
            set
            {
                _InvoiceLatest = value;
                OnPropertyChanged(nameof(InvoiceLatest));
                OnPropertyChanged(nameof(InvoiceNumber));
            }
        }

        private int _ReceiptLatest = 1;
        public int ReceiptLatest
        {
            get => _ReceiptLatest;
            set
            {
                _ReceiptLatest = value;
                OnPropertyChanged(nameof(ReceiptLatest));
                OnPropertyChanged(nameof(ReceiptNumber));
            }
        }


        public static SlipNumberClass GetSlipNumberByMonth(DateTime date)
        {
            var term = new DateTime(date.Year, date.Month, 1);
            var info = new SlipNumberClass();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_SLIP_NUMBER_INFO WHERE SLIP_TERM = @SlipTerm", connection);
            command.Parameters.AddWithValue("@SlipTerm", term);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                info.SlipNumberID = reader.GetInt32("SLIP_NUMBER_ID");
                info.SlipTerm = reader.GetDateTime("SLIP_TERM");
                info.InvoicePrefix = reader.GetString("INVOICE_PREFIX");
                info.ReceiptPrefix = reader.GetString("RECEIPT_PREFIX");
                info.InvoiceLatest = reader.GetInt32("INVOICE_LATEST");
                info.ReceiptLatest = reader.GetInt32("RECEIPT_LATEST");
            }

            return info;
        }

        public static SlipNumberClass AddSlipNumberByMonth(DateTime slipTerm)
        {
            var term = new DateTime(slipTerm.Year, slipTerm.Month, 1);
            var slipNumberClass = new SlipNumberClass()
            {
                SlipTerm = term
            };

            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("INSERT INTO T_SLIP_NUMBER_INFO (SLIP_TERM, INVOICE_PREFIX, RECEIPT_PREFIX, INVOICE_LATEST, RECEIPT_LATEST) " + "\r\n" + "VALUES (@slipTerm, @invoicePrefix, @receiptPrefix, @invoiceLatest, @receiptLatest)", connection);
            command.Parameters.AddWithValue("@slipTerm", slipNumberClass.SlipTerm);
            command.Parameters.AddWithValue("@invoicePrefix", slipNumberClass.InvoicePrefix);
            command.Parameters.AddWithValue("@receiptPrefix", slipNumberClass.ReceiptPrefix);
            command.Parameters.AddWithValue("@invoiceLatest", slipNumberClass.InvoiceLatest);
            command.Parameters.AddWithValue("@receiptLatest", slipNumberClass.ReceiptLatest);
            command.ExecuteNonQuery();
            slipNumberClass.SlipNumberID = (int)command.LastInsertedId;
            return slipNumberClass;

        }
        public void UpdateSlipNumberInfo()
        {

            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand(
                "UPDATE T_SLIP_NUMBER_INFO SET INVOICE_PREFIX = @invoicePrefix, RECEIPT_PREFIX = @receiptPrefix, INVOICE_LATEST = @invoiceLatest, RECEIPT_LATEST = @receiptLatest WHERE SLIP_NUMBER_ID = @slipNumberId", connection);
            command.Parameters.AddWithValue("@slipNumberId", SlipNumberID);
            command.Parameters.AddWithValue("@slipTerm", SlipTerm);
            command.Parameters.AddWithValue("@invoicePrefix", InvoicePrefix);
            command.Parameters.AddWithValue("@receiptPrefix", ReceiptPrefix);
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

        }
    }

    public class SlipNumbers : SlipNumberClass, ILoggable
    {
        private ObservableCollection<SlipNumberClass> _SlipNumberList = [];
        public ObservableCollection<SlipNumberClass> SlipNumberList
        {
            get => _SlipNumberList;
            set
            {
                _SlipNumberList = value;
            }
        }
        public SlipNumbers()
        {
            _SlipNumberList = GetSlipNumbers();
        }
        public static ObservableCollection<SlipNumberClass> GetSlipNumbers()
        {
            ObservableCollection<SlipNumberClass> slipNumberClasses = new ObservableCollection<SlipNumberClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_SLIP_NUMBER_INFO ORDER BY SLIP_TERM ASC", connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var slipNumberInfo = new SlipNumberClass
                {
                    SlipNumberID = reader.GetInt32("SLIP_NUMBER_ID"),
                    SlipTerm = reader.GetDateTime("SLIP_TERM"),
                    InvoicePrefix = reader.GetString("INVOICE_PREFIX"),
                    ReceiptPrefix = reader.GetString("RECEIPT_PREFIX"),
                    InvoiceLatest = reader.GetInt32("INVOICE_LATEST"),
                    ReceiptLatest = reader.GetInt32("RECEIPT_LATEST"),
                };
                slipNumberClasses.Add(slipNumberInfo);
            }
            return slipNumberClasses;

        }

        public SlipNumberClass GetSlipNumber(DateTime? _date = null)
        {

            var date = _date is DateTime ? (DateTime)_date : DateTime.Today;
            date = new DateTime(year: date.Year, month: date.Month, day: 1);
            var slipNumber = SlipNumberList.FirstOrDefault(s => s.SlipTerm == date);
            if (slipNumber == null)
            {
                slipNumber = AddSlipNumberByMonth(date);
                SlipNumberList.Add(slipNumber);
            }
            return slipNumber;
        }
        public void SlipnumberInfoReload()
        {

            _SlipNumberList = GetSlipNumbers();
        }


    }

}
