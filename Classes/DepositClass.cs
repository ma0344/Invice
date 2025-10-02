using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice.Classes
{

    // T_DEPOSIT テーブルに対応するクラス
    public class DepositClass : ILoggable
    {
        public int DepositId { get; set; }
        public int? InvoiceId { get; set; }
        public int? PaymentId { get; set; }
        public int CustomerId { get; set; }
        public DateTime DepositDate { get; set; }
        public int DepositAmount { get; set; }
        public string SlipNumber { get; set; } = string.Empty;
        public int DebOrCreId { get; set; }

        public static List<DepositClass> GetDeposits(TypeOfID type = 0, int id = 0)
        {

            var deposits = new List<DepositClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            string query = QueryBuilder.StringBuilder(command
                : "SELECT *", tableName: "T_DEPOSIT", type);
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

        public static DepositClass? GetDeposit(TypeOfID type = 0, int? id = 0)
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


        public static bool TryAddDeposit(object? obj = null, UnitOfWork? unitOfWork = null)
        {
            DepositClass deposit = new();
            if (obj is PaymentClass payment)
            {// T_DEPOSITに入金情報を追加する
                deposit.InvoiceId = null;
                deposit.PaymentId = payment.PaymentId;
                deposit.CustomerId = payment.CustomerId;
                deposit.DepositDate = payment.PaymentDate;
                deposit.DepositAmount = payment.PaymentAmount;
                deposit.SlipNumber = payment.SlipNumber;
                deposit.DebOrCreId = 2;
            }
            else if (obj is InvoiceClass invoice)
            {// T_DEPOSITに請求情報を追加する
                deposit.InvoiceId = invoice.InvoiceId;
                deposit.PaymentId = null;
                deposit.CustomerId = invoice.CustomerId;
                deposit.DepositDate = invoice.IssueDate ?? DateTime.Now;
                deposit.DepositAmount = invoice.PaidByDeposit;
                deposit.SlipNumber = invoice.SlipNumber ?? "";
                deposit.DebOrCreId = 1;
            }
            return UnitOfWork.ExecuteWithTransaction(uow =>
            {
                // 前受額が0でない場合は、T_BALANCEに入金情報を追加する
                if (deposit.DepositAmount > 0)
                    if (AddDeposit(uow, deposit) == 0)
                        return false;
                if (BalanceClass.TryAddBalance(deposit, uow) == false)
                    return false;

                if (obj is PaymentClass payment)
                    payment.DepositId = deposit.DepositId;
                return true;
            }, unitOfWork);
        }

        public static int AddDeposit(UnitOfWork unitOfWork, DepositClass? deposit = null)
        {
            deposit ??= new();
            var query = "INSERT INTO T_DEPOSIT (INVOICE_ID, PAYMENT_ID, CUSTOMER_ID, DEPOSIT_DATE, DEPOSIT_AMOUNT, SLIP_NUMBER, DEBIT_OR_CREDIT_ID) " + "\r\n" + "VALUES (@InvoiceId, @PaymentId, @CustomerId, @DepositDate, @DepositAmount, @SlipNumber, @DebitOrCreditId)";
            var command = unitOfWork.CreateCommand(query);
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

        /// <summary>
        /// TryUpdateDeposit
        /// DepositClassのインスタンスを受け取り、T_DEPOSITテーブルを更新する
        /// 更新するレコードが存在しない場合は、T_DEPOSITテーブルに新しいレコードを追加する
        /// 更新データのAmountが0以下の場合は、T_DEPOSITテーブルからレコードを削除する
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool TryUpdateDeposit(object? obj, UnitOfWork? unitOfWork = null)
        {
            var deposit = new DepositClass();
            return UnitOfWork.ExecuteWithTransaction(uow =>
            {
                if (obj is PaymentClass payment)
                {
                    deposit = GetDeposit(TypeOfID.Payment, payment.PaymentId);
                    if (payment.PaymentAmount <= 0)
                    {
                        deposit?.DeleteDeposit(uow);
                        return true;
                    }

                    if (deposit == null)
                    {
                        TryAddDeposit(payment, uow);
                        return true;
                    }
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
                    deposit = GetDeposit(TypeOfID.Invoice, invoice.InvoiceId);
                    if (invoice.PaidByDeposit <= 0)
                    {
                        deposit?.DeleteDeposit(uow);
                        return true;
                    }

                    if (deposit == null)
                    {
                        TryAddDeposit(invoice, uow);
                        return true;
                    }
                    deposit.InvoiceId = invoice.InvoiceId;
                    deposit.PaymentId = null;
                    deposit.CustomerId = invoice.CustomerId;
                    deposit.DepositDate = invoice.PaymentDate ?? DateTime.Now;
                    deposit.DepositAmount = invoice.PaidByDeposit;
                    deposit.SlipNumber = invoice.SlipNumber ?? "";
                    deposit.DebOrCreId = 1;
                }

                UpdateDeposit(deposit, uow);
                BalanceClass.TryUpdateBalance(deposit, uow);
                return true;
            }, unitOfWork);
        }


        public static void UpdateDeposit(DepositClass deposit, UnitOfWork unitOfWork)
        {

            var whereClause = deposit.SlipNumber.StartsWith('R') ? "PAYMENT_ID" : "INVOICE_ID";
            var query = $"UPDATE T_DEPOSIT SET INVOICE_ID=@InvoiceId, PAYMENT_ID=@PaymentId, CUSTOMER_ID=@CustomerId, DEPOSIT_DATE=@DepositDate, DEPOSIT_AMOUNT=@DepositAmount, SLIP_NUMBER=@SlipNumber, DEBIT_OR_CREDIT_ID=@DebitOrCreditId WHERE {whereClause}=@Id";
            var command = unitOfWork.CreateCommand(query);

            command.Parameters.AddWithValue("@InvoiceId", deposit.InvoiceId);
            command.Parameters.AddWithValue("@PaymentId", deposit.PaymentId);
            command.Parameters.AddWithValue("@CustomerId", deposit.CustomerId);
            command.Parameters.AddWithValue("@DepositDate", deposit.DepositDate);
            command.Parameters.AddWithValue("@DepositAmount", deposit.DepositAmount);
            command.Parameters.AddWithValue("@SlipNumber", deposit.SlipNumber);
            command.Parameters.AddWithValue("@DebitOrCreditId", deposit.DebOrCreId);

            var id = whereClause == "PAYMENT_ID" ? deposit.PaymentId : deposit.InvoiceId;
            command.Parameters.AddWithValue("@Id", id);

            command.ExecuteNonQuery();
        }

        public void DeleteDeposit(UnitOfWork unitOfWork)
        {
            var command = unitOfWork.CreateCommand();
            command.CommandText = "DELETE FROM T_DEPOSIT WHERE DEPOSIT_ID = @DepositId";
            command.Parameters.AddWithValue("@DepositId", DepositId);
            command.ExecuteNonQuery();
        }

        public static bool DeleteDepositById(TypeOfID type, int? id, UnitOfWork? unitOfWork = null)
        {
            if (id == null) return false;
            return UnitOfWork.ExecuteWithTransaction(uow =>
            {
                var command = QueryBuilder.CommandBuilder("DELETE", "T_DEPOSIT", type, (int)id, uow);
                command.ExecuteNonQuery();
                BalanceClass.DeleteBalanceById(type, (int)id, uow);
                return true;
            }, unitOfWork);

        }
        public static void DeleteDepositById(IDs ids, UnitOfWork unitOfWork)
        {
            string query = "DELETE FROM T_DEPOSIT WHERE ";
            var command = CommandBuilder.Builder(ids, query, unitOfWork);
            BalanceClass.DeleteBalanceById(ids, unitOfWork);
            command.ExecuteNonQuery();
        }


    }
}
