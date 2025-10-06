using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice.Classes
{
    // T_PAYMENT テーブルに対応するクラス
    public class PaymentClass : INotifyPropertyChanged, ILoggable
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

        private string _Subject = string.Empty;
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

        private string _PaymentDateString = string.Empty;
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
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_PAYMENT WHERE PAYMENT_DATE BETWEEN @start AND @end", connection);
            command.Parameters.AddWithValue("@start", date);
            command.Parameters.AddWithValue("@end", date.AddMonths(1).AddDays(-1));
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var payment = new PaymentClass()
                {
                    PaymentId = reader.GetInt32("PAYMENT_ID"),
                    InvoiceId = reader.IsDBNull("INVOICE_ID") ? null : reader.GetInt32("INVOICE_ID"),
                    DepositId = reader.IsDBNull("DEPOSIT_ID") ? null : reader.GetInt32("DEPOSIT_ID"),
                    TransactionTypeId = reader.GetInt32("TRANSACTION_TYPE_ID"),
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


        public bool TryAddPayment(UnitOfWork? unitOfWork = null)
        {

            return UnitOfWork.ExecuteWithTransaction(uow =>
            {
                AddPayment(uow);
                if (TransactionTypeId == TransactionTypeIdsProvider.BalanceId)
                    BalanceClass.TryAddBalance(this, uow);
                else if (TransactionTypeId == TransactionTypeIdsProvider.DepositId)
                {
                    return DepositClass.TryAddDeposit(this, uow);
                }
                else return false;
                return true;
            }, unitOfWork);
        }
        public void AddPayment(UnitOfWork unitOfWork)
        {

            string query = @"INSERT INTO T_PAYMENT (INVOICE_ID, DEPOSIT_ID, TRANSACTION_TYPE_ID, CUSTOMER_ID, SLIP_NUMBER, PAYMENT_DATE, PAYMENT_AMOUNT, SUBJECT) " + "\r\n" + "VALUES (@InvoiceId, @DepositId, @TransactionTypeId, @CustomerId, @SlipNumber, @PaymentDate, @PaymentAmount, @Subject)";
            var command = unitOfWork.CreateCommand(query);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@DepositId", DepositId);
            command.Parameters.AddWithValue("@TransactionTypeId", TransactionTypeId);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@SlipNumber", SlipNumber);
            command.Parameters.AddWithValue("@PaymentDate", PaymentDate);
            command.Parameters.AddWithValue("@PaymentAmount", PaymentAmount);
            command.Parameters.AddWithValue("@Subject", Subject);
            command.ExecuteNonQuery();
            PaymentId = (int)command.LastInsertedId;

        }

        public bool TryUpdatePayment(UnitOfWork? unitOfWork = null)
        {

            return UnitOfWork.ExecuteWithTransaction(uow =>
            {
                UpdatePayment(uow);
                if (TransactionTypeId == TransactionTypeIdsProvider.BalanceId)
                    BalanceClass.TryUpdateBalance(this, uow);
                else if (TransactionTypeId == TransactionTypeIdsProvider.DepositId)
                {
                    DepositClass.TryUpdateDeposit(this, uow);
                }
                else return false;
                return true;
            }, unitOfWork);
        }

        public void UpdatePayment(UnitOfWork unitOfWork)
        {

            string query = @"UPDATE T_PAYMENT SET INVOICE_ID = @InvoiceId, DEPOSIT_ID = @DepositId, TRANSACTION_TYPE_ID = @TransactionTypeId, CUSTOMER_ID = @CustomerId, SLIP_NUMBER = @SlipNumber, PAYMENT_DATE = @PaymentDate, PAYMENT_AMOUNT = @PaymentAmount, SUBJECT = @Subject WHERE PAYMENT_ID = @PaymentId";
            var command = unitOfWork.CreateCommand(query);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@DepositId", DepositId);
            command.Parameters.AddWithValue("@TransactionTypeId", TransactionTypeId);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@SlipNumber", SlipNumber);
            command.Parameters.AddWithValue("@PaymentDate", PaymentDate);
            command.Parameters.AddWithValue("@PaymentAmount", PaymentAmount);
            command.Parameters.AddWithValue("@Subject", Subject);
            command.Parameters.AddWithValue("@PaymentId", PaymentId);
            command.ExecuteNonQuery();
        }

        public bool TryDeletePayment(UnitOfWork? unitOfWork = null)
        {
            unitOfWork ??= new UnitOfWork();
            return UnitOfWork.ExecuteWithTransaction(uow =>
            {
                DeletePayment(unitOfWork);
                return true;
            }, unitOfWork);
        }

        public void DeletePayment(UnitOfWork unitOfWork)
        {

            string query = "DELETE FROM T_PAYMENT WHERE PAYMENT_ID = @PaymentId";
            var command = unitOfWork.CreateCommand(query);
            command.Parameters.AddWithValue("@PaymentId", PaymentId);
            command.ExecuteNonQuery();
            DepositClass.DeleteDepositById(TypeOfID.Payment, PaymentId, unitOfWork);
            BalanceClass.DeleteBalanceById(TypeOfID.Payment, PaymentId, unitOfWork);
        }

        public static bool TryDeletePaymentById(TypeOfID type, int paymentId, UnitOfWork? unitOfWork = null)
        {
            return UnitOfWork.ExecuteWithTransaction(uow =>
            {
                DeletePaymentById(type, paymentId, uow);
                return true;
            }, unitOfWork);
        }

        public static void DeletePaymentById(TypeOfID type, int id, UnitOfWork unitOfWork)
        {
            string query = QueryBuilder.StringBuilder(command:"DELETE", tableName: "T_PAYMENT", type: type);
            var command = unitOfWork.CreateCommand(query);
            command.Parameters.AddWithValue("@PaymentId", id);
            command.ExecuteNonQuery();
        }


        public static void ClearInvoiceIdFromPayment(int invoiceId, UnitOfWork unitOfWork)
        {
            string query = "UPDATE T_PAYMENT SET INVOICE_ID = NULL WHERE INVOICE_ID = @InvoiceId";
            var command = unitOfWork.CreateCommand(query);
            command.Parameters.AddWithValue("@InvoiceId", invoiceId);
            command.ExecuteNonQuery();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {


            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
