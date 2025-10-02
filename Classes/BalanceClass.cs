using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Invoice.Classes
{
    // T_BALANCE テーブルに対応するクラス
    public class BalanceClass : ILoggable
    {
        public int BalanceId { get; set; }
        public int CustomerId { get; set; }
        public int? InvoiceId { get; set; }
        public int? PaymentId { get; set; }
        public int? DepositId { get; set; }
        public string SlipNumber { get; set; } = string.Empty;
        public int DebOrCreId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int TransactionTypeId { get; set; }
        public int TransactionAmount { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string TransactionTypeName { get; set; } = string.Empty;
        public string DateString => TransactionDate.ToShortDateString();
        public decimal? DebitAmount => DebOrCreId == 1 ? TransactionAmount : null;
        public decimal? CreditAmount => DebOrCreId == 2 ? TransactionAmount : null;

        // データベースから全てのレコードを取得
        public static List<BalanceClass> GetAllBalances()
        {
            var balances = new List<BalanceClass>();
            var unitOfWork = new UnitOfWork();
            var command = unitOfWork.CreateCommand("SELECT * FROM T_BALANCE");
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

        public static int GetBalanceIdById(TypeOfID type, int id, UnitOfWork unitOfWork)
        {
            List<int> ints = [];
            var command = QueryBuilder.CommandBuilder("SELECT *", "T_BALANCE", type, id, unitOfWork);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ints.Add(reader.GetInt32("BALANCE_ID"));
            }
            switch (ints.Count)
            {
                case 0: return 0;
                case 1: return ints[0];
                default:
                    string idType = type switch
                    {
                        TypeOfID.Customer => "CUSTOMER_ID",
                        TypeOfID.Invoice => "INVOICE_ID",
                        TypeOfID.Payment => "PAYMENT_ID",
                        TypeOfID.Deposit => "DEPOSIT_ID",
                        _ => throw new ArgumentException("Invalid type")
                    };
                    unitOfWork.Rollback();
                    throw new ArgumentException($"T_BALANCE.{idType}にIDが '{id}' であるレコードが複数存在します");
            }
        }

        public static List<BalanceClass> GetBalancesById(IDs ids, UnitOfWork unitOfWork, [CallerMemberName] string _1 = "", [CallerLineNumber] long _2 = 0)
        {
            var balances = new List<BalanceClass>();
            string query = "SELECT * FROM T_BALANCE WHERE ";

            using var reader = CommandBuilder.Builder(ids, query, unitOfWork).ExecuteReader();
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

        // 新しいレコードを追加
        public static bool TryAddBalance(object obj, UnitOfWork unitOfWork)
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
                    balance.SlipNumber = invoice.SlipNumber ?? "";
                    balance.TransactionDate = invoice.IssueDate ?? DateTime.Now;
                    balance.TransactionTypeId = invoice.TransactionTypeId ?? 0;
                    balance.TransactionAmount = invoice.InvoiceTotal ?? 0;
                    if (invoice.InvoiceTotal == 0)
                    {
                        // 削除 + 再集計
                        DeleteBalanceById(new IDs(invoiceId: invoice.InvoiceId, paymentId: null), unitOfWork);
                        CustomerClass.RecalculateAndPersistBalance(invoice.CustomerId, unitOfWork);
                        return true;
                    }
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
                AddBalance(balance, unitOfWork);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{nameof(BalanceClass)}.{MethodBase.GetCurrentMethod()!.Name} : {ex.Message}");
                return false;
            }
        }
    
        public static void AddBalance(BalanceClass balance, UnitOfWork unitOfWork)
        {
            string query = @"INSERT INTO T_BALANCE (CUSTOMER_ID, INVOICE_ID, PAYMENT_ID, DEPOSIT_ID, SLIP_NUMBER, DEBIT_OR_CREDIT_ID, TRANSACTION_DATE, TRANSACTION_TYPE_ID, TRANSACTION_AMOUNT) " + "\r\n" + "VALUES (@CustomerId, @InvoiceId, @PaymentId, @DepositId, @SlipNumber, @DebOrCreId, @TransactionDate, @TransactionTypeId, @TransactionAmount)";
            var command = unitOfWork.CreateCommand(query);
            command.Parameters.AddWithValue("@CustomerId", balance.CustomerId);
            command.Parameters.AddWithValue("@InvoiceId", balance.InvoiceId);
            command.Parameters.AddWithValue("@PaymentId", balance.PaymentId);
            command.Parameters.AddWithValue("@DepositId", balance.DepositId);
            command.Parameters.AddWithValue("@SlipNumber", balance.SlipNumber);
            command.Parameters.AddWithValue("@DebOrCreId", balance.DebOrCreId);
            command.Parameters.AddWithValue("@TransactionDate", balance.TransactionDate);
            command.Parameters.AddWithValue("@TransactionTypeId", balance.TransactionTypeId);
            command.Parameters.AddWithValue("@TransactionAmount", balance.TransactionAmount);
            command.ExecuteNonQuery();
            balance.BalanceId = (int)command.LastInsertedId;

            CustomerClass.RecalculateAndPersistBalance(balance.CustomerId, unitOfWork);
        }

        // レコードを更新
        public void UpdateBalance(UnitOfWork unitOfWork)
        {
            string query = @"UPDATE T_BALANCE SET CUSTOMER_ID = @CustomerId, INVOICE_ID = @InvoiceId, PAYMENT_ID = @PaymentId, DEPOSIT_ID = @DepositId, DEBIT_OR_CREDIT_ID = @DebOrCreId, SLIP_NUMBER = @SlipNumber, TRANSACTION_DATE = @TransactionDate, TRANSACTION_TYPE_ID = @TransactionTypeId, TRANSACTION_AMOUNT = @TransactionAmount WHERE BALANCE_ID = @BalanceId";
            var command = unitOfWork.CreateCommand(query);
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.Parameters.AddWithValue("@PaymentId", PaymentId);
            command.Parameters.AddWithValue("@DepositId", DepositId);
            command.Parameters.AddWithValue("@DebOrCreId", DebOrCreId);
            command.Parameters.AddWithValue("@SlipNumber", SlipNumber);
            command.Parameters.AddWithValue("@TransactionDate", TransactionDate);
            command.Parameters.AddWithValue("@TransactionTypeId", TransactionTypeId);
            command.Parameters.AddWithValue("@TransactionAmount", TransactionAmount);
            command.Parameters.AddWithValue("@BalanceId", BalanceId);
            command.ExecuteNonQuery();

            CustomerClass.RecalculateAndPersistBalance(CustomerId, unitOfWork);
        }

        public static bool TryUpdateBalance(object? obj, UnitOfWork? unitOfWork = null)
        {
            return UnitOfWork.ExecuteWithTransaction(uow =>
            {
                var balance = new BalanceClass();

                if (obj is PaymentClass payment)
                {
                    balance.BalanceId = GetBalancesById(new IDs(paymentId: payment.PaymentId), uow)[0].BalanceId;
                    if (balance.BalanceId == 0)
                    {
                        TryAddBalance(payment, uow);
                        return true;
                    }
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
                    var balances = GetBalancesById(new IDs(invoiceId: invoice.InvoiceId, paymentId: null), uow);
                    if (balances.Count == 0)
                    {
                        TryAddBalance(invoice, uow);
                        return true;
                    }
                    balance.BalanceId = balances.FirstOrDefault(b => (b.InvoiceId == invoice.InvoiceId && b.DebOrCreId == 1))!.BalanceId;
                    balance.CustomerId = invoice.CustomerId;
                    balance.InvoiceId = invoice.InvoiceId;
                    balance.PaymentId = null;
                    balance.DepositId = null;
                    balance.DebOrCreId = 1;
                    balance.SlipNumber = invoice.SlipNumber ?? "";
                    balance.TransactionDate = invoice.IssueDate ?? DateTime.Now;
                    balance.TransactionTypeId = 1;
                    balance.TransactionAmount = invoice.InvoiceTotal ?? 0;
                }
                else if (obj is DepositClass deposit)
                {
                    balance.BalanceId = GetBalanceIdById(TypeOfID.Deposit, deposit.DepositId, uow);
                    if (balance.BalanceId == 0)
                    {
                        TryAddBalance(deposit, uow);
                        return true;
                    }
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

                if (balance.TransactionAmount == 0)
                {
                    balance.DeleteBalance(uow);
                }
                else
                {
                    balance.UpdateBalance(uow);
                }
                return true;
            }, unitOfWork);
        }

        public static void DeleteBalanceById(TypeOfID type, int id, UnitOfWork unitOfWork, [CallerMemberName] string _1 = "", [CallerLineNumber] long _2 = 0)
        {
            // 対象顧客特定
            string col = type switch
            {
                TypeOfID.Customer => "CUSTOMER_ID",
                TypeOfID.Invoice => "INVOICE_ID",
                TypeOfID.Payment => "PAYMENT_ID",
                TypeOfID.Deposit => "DEPOSIT_ID",
                _ => throw new ArgumentException("Invalid type")
            };
            var getCmd = unitOfWork.CreateCommand($"SELECT DISTINCT CUSTOMER_ID FROM T_BALANCE WHERE {col}=@id");
            getCmd.Parameters.AddWithValue("@id", id);
            List<int> cids = [];
            using (var r = getCmd.ExecuteReader())
                while (r.Read()) cids.Add(r.GetInt32(0));

            string query = QueryBuilder.StringBuilder(command: "DELETE", tableName: "T_BALANCE", type);
            var command = unitOfWork.CreateCommand(query);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();

            foreach (var cid in cids.Distinct())
                CustomerClass.RecalculateAndPersistBalance(cid, unitOfWork);
        }
        /// <summary>
        /// DeleteBalanceById
        /// T_BALANCE テーブルからレコードを削除するメソッド
        /// 各引数がnullの場合は、条件としてnullを追加する
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <param name="paymentId"></param>
        /// <param name="depositId"></param>
        /// <param name="balanceId"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void DeleteBalanceById(IDs ids, UnitOfWork unitOfWork, [CallerMemberName] string _1 = "", [CallerLineNumber] long _2 = 0)
        {
            // 削除前に該当顧客取得
            string selectQuery = "SELECT DISTINCT CUSTOMER_ID FROM T_BALANCE WHERE ";
            var selectCmd = CommandBuilder.Builder(ids, selectQuery, unitOfWork);
            List<int> cids = [];
            using (var r = selectCmd.ExecuteReader())
                while (r.Read()) cids.Add(r.GetInt32(0));

            string deleteQuery = "DELETE FROM T_BALANCE WHERE ";
            CommandBuilder.Builder(ids, deleteQuery, unitOfWork).ExecuteNonQuery();

            foreach (var cid in cids.Distinct())
                CustomerClass.RecalculateAndPersistBalance(cid, unitOfWork);
        }

        // レコードを削除
        public void DeleteBalance(UnitOfWork unitOfWork)
        {
            var customerId = CustomerId;
            string query = "DELETE FROM T_BALANCE WHERE BALANCE_ID = @BalanceId";
            var command = unitOfWork.CreateCommand(query);
            command.Parameters.AddWithValue("@BalanceId", BalanceId);
            command.ExecuteNonQuery();

            CustomerClass.RecalculateAndPersistBalance(customerId, unitOfWork);
        }
    }

}
