using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Invoice.Classes
{
    // T_INVOICE テーブルに対応するクラス
    public class InvoiceClass : INotifyPropertyChanged, ILoggable
    {
        private int _InvoiceId = 0;
        public int InvoiceId
        {
            get => _InvoiceId;
            set { _InvoiceId = value; OnPropertyChanged(nameof(InvoiceId)); }
        }

        private int _CustomerId = 0;
        public int CustomerId
        {
            get => _CustomerId;
            set { _CustomerId = value; OnPropertyChanged(nameof(CustomerId)); }
        }

        private DateTime? _IssueDate = DateTime.Now;
        public DateTime? IssueDate
        {
            get => _IssueDate;
            set { _IssueDate = value; OnPropertyChanged(nameof(IssueDate)); }
        }

        private DateTime? _DueDate = DateTime.Now;
        public DateTime? DueDate
        {
            get => _DueDate;
            set { _DueDate = value; OnPropertyChanged(nameof(DueDate)); }
        }

        private string? _Subject = "";
        public string? Subject
        {
            get => _Subject;
            set { _Subject = value; OnPropertyChanged(nameof(Subject)); }
        }

        private string? _SlipNumber = "";
        public string? SlipNumber
        {
            get => _SlipNumber;
            set { _SlipNumber = value; OnPropertyChanged(nameof(SlipNumber)); }
        }

        public int? SubTotal => InvoiceItems.Sum(x => x.Quantity * x.UnitPrice);
        public int? Tax => InvoiceItems.Sum(x => x.Tax);
        public int? InvoiceTotal => TransactionTypeId == TransactionTypeIdsProvider.BalanceId ? ItemsTotal : ItemsTotal - PaidByDeposit;
        public int ItemsTotal
        {
            get
            {
                var itemsTotal = InvoiceItems.Sum(x => x.Quantity * x.UnitPrice + x.Tax);
                PaidByDeposit = DepositUntilIssueDate < itemsTotal ? DepositUntilIssueDate : itemsTotal; // 副作用: 後続フェーズで除去予定
                return itemsTotal;
            }
        }

        private int _PaidByDeposit = 0;
        public int PaidByDeposit
        {
            get => _PaidByDeposit;
            set { _PaidByDeposit = value; OnPropertyChanged(nameof(PaidByDeposit)); }
        }

        private string? _Message = "";
        public string? Message
        {
            get => _Message;
            set { _Message = value; OnPropertyChanged(nameof(Message)); }
        }

        private int? _TransactionTypeId = 1;
        public int? TransactionTypeId
        {
            get => _TransactionTypeId;
            set { _TransactionTypeId = value; OnPropertyChanged(nameof(TransactionTypeId)); }
        }

        private DateTime? _PaymentDate = DateTime.Now;
        public DateTime? PaymentDate
        {
            get => _PaymentDate;
            set { _PaymentDate = value; OnPropertyChanged(nameof(PaymentDate)); }
        }

        private int _InvoiceStatusId = 0;
        public int InvoiceStatusId
        {
            get => _InvoiceStatusId;
            set { _InvoiceStatusId = value; OnPropertyChanged(nameof(InvoiceStatusId)); }
        }

        private string _CustomerName = "";
        public string CustomerName
        {
            get => _CustomerName;
            set { _CustomerName = value; OnPropertyChanged(nameof(CustomerName)); }
        }

        private string _InvoiceStatus = "";
        public string InvoiceStatus
        {
            get => _InvoiceStatus;
            set { _InvoiceStatus = value; OnPropertyChanged(nameof(InvoiceStatus)); }
        }

        private string? _IssueDateString = "";
        public string? IssueDateString
        {
            get => _IssueDateString;
            set { _IssueDateString = value; OnPropertyChanged(nameof(IssueDateString)); }
        }

        private int _DepositUntilIssueDate = 0;
        public int DepositUntilIssueDate
        {
            get => _DepositUntilIssueDate;
            set
            {
                _DepositUntilIssueDate = value;
                var afterPaidDeposit = DepositUntilIssueDate - ItemsTotal; // 当該請求額支払後 前受残高
                PaidByDeposit = afterPaidDeposit <= 0 ? DepositUntilIssueDate : ItemsTotal; // 前受精算額
                OnPropertyChanged(nameof(InvoiceTotal));
                OnPropertyChanged(nameof(DepositUntilIssueDate));
            }
        }

        private ObservableCollection<InvoiceItemClass> _InvoiceItems = [];
        public ObservableCollection<InvoiceItemClass> InvoiceItems
        {
            get => _InvoiceItems;
            set
            {
                UpdateCollectionEventHandlers(_InvoiceItems, value, InvoiceItems_CollectionChanged, InvoiceItem_PropertyChanged);
                _InvoiceItems = value;
                OnPropertyChanged(nameof(InvoiceItems));
                RecalculateTotals();
            }
        }

        public InvoiceClass()
        {
            _InvoiceItems.CollectionChanged += InvoiceItems_CollectionChanged;
        }

        private void InvoiceItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (InvoiceItemClass item in e.NewItems)
                {
                    item.PropertyChanged -= InvoiceItem_PropertyChanged;
                    item.PropertyChanged += InvoiceItem_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (InvoiceItemClass item in e.OldItems)
                {
                    item.PropertyChanged -= InvoiceItem_PropertyChanged;
                }
            }
            RecalculateTotals();
        }

        private void InvoiceItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(InvoiceItemClass.ItemId) ||
                e.PropertyName == nameof(InvoiceItemClass.ItemName) ||
                e.PropertyName == nameof(InvoiceItemClass.ItemTotal) ||
                e.PropertyName == nameof(InvoiceItemClass.Tax) ||
                e.PropertyName == nameof(InvoiceItemClass.ItemSubTotal) ||
                e.PropertyName == nameof(InvoiceItemClass.Quantity) ||
                e.PropertyName == nameof(InvoiceItemClass.UnitPrice))
            {
                RecalculateTotals();
                OnPropertyChanged(nameof(InvoiceItems));
            }
        }

        public void RecalculateTotals()
        {
            NotifyPropertiesChanged(nameof(DepositUntilIssueDate), nameof(InvoiceItems), nameof(Tax), nameof(PaidByDeposit), nameof(ItemsTotal), nameof(InvoiceTotal));
        }

        private void NotifyPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
                OnPropertyChanged(propertyName);
        }

        public static List<InvoiceClass> GetAllInvoice()
        {
            var invoices = new List<InvoiceClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var command = new MySqlCommand("SELECT * FROM T_INVOICE", connection);
            using var reader = command.ExecuteReader();

            // ローカル Culture (日本語 + 和暦) を使用して文字列を生成（スレッド全体へは適用しない）
            CultureInfo localCulture = new("ja-JP");
            localCulture.DateTimeFormat.Calendar = new JapaneseCalendar();
            localCulture.DateTimeFormat.ShortDatePattern = "ggy年M月d日";

            while (reader.Read())
            {
                var invoice = new InvoiceClass()
                {
                    InvoiceId = reader.GetInt32("INVOICE_ID"),
                    CustomerId = reader.GetInt32("CUSTOMER_ID"),
                    IssueDate = reader.IsDBNull("ISSUE_DATE") ? null : reader.GetDateTime("ISSUE_DATE"),
                    DueDate = reader.IsDBNull("DUE_DATE") ? null : reader.GetDateTime("DUE_DATE"),
                    Subject = reader.IsDBNull("SUBJECT") ? null : reader.GetString("SUBJECT"),
                    SlipNumber = reader.IsDBNull("SLIP_NUMBER") ? null : reader.GetString("SLIP_NUMBER"),
                    PaidByDeposit = reader.GetInt32("PAID_BY_DEPOSIT"),
                    Message = reader.IsDBNull("MESSAGE") ? null : reader.GetString("MESSAGE"),
                    TransactionTypeId = reader.IsDBNull("TRANSACTION_TYPE_ID") ? null : reader.GetInt32("TRANSACTION_TYPE_ID"),
                    PaymentDate = reader.IsDBNull("PAYMENT_DATE") ? null : reader.GetDateTime("PAYMENT_DATE"),
                    InvoiceStatusId = reader.GetInt32("INVOICE_STATUS_ID"),
                };
                invoice.IssueDateString = invoice.IssueDate?.ToString("ggy年M月d日", localCulture);
                invoices.Add(invoice);
            }
            return invoices;
        }

        public bool TryAddInvoice(UnitOfWork? unitOfWork = null)
        {
            return UnitOfWork.ExecuteWithTransaction(uow =>
            {
                AddInvoice(uow);
                InvoiceItemClass.AddInvoiceItems(InvoiceItems, InvoiceId, uow);
                if (TransactionTypeId == TransactionTypeIdsProvider.BalanceId)
                    BalanceClass.TryAddBalance(this, uow);
                else if (TransactionTypeId == TransactionTypeIdsProvider.DepositId)
                {
                    PaymentDate = IssueDate;
                    DepositClass.TryAddDeposit(this, uow);
                }
                else return false;
                return true;
            }, unitOfWork);
        }

        public void AddInvoice(UnitOfWork unitOfWork)
        {
            string query = @"INSERT INTO T_INVOICE (CUSTOMER_ID, ISSUE_DATE, DUE_DATE, SUBJECT, SLIP_NUMBER, ITEMS_TOTAL, SUBTOTAL, TAX, PAID_BY_DEPOSIT, TOTAL, MESSAGE, TRANSACTION_TYPE_ID, PAYMENT_DATE, INVOICE_STATUS_ID) " + "\r\n" + "VALUES (@CustomerId, @IssueDate, @DueDate, @Subject, @SlipNumber, @ItemsTotal, @Subtotal, @Tax, @PaidByDeposit, @InvoiceTotal, @Message, @TransactionTypeId, @PaymentDate, @InvoiceStatusId)";
            var command = unitOfWork.CreateCommand(query);
            AddParametersToCommand(command);
            command.Parameters.AddWithValue("@TransactionTypeId", TransactionTypeId);
            command.ExecuteNonQuery();
            InvoiceId = (int)command.LastInsertedId;
        }

        private void AddParametersToCommand(TrackedCommand command)
        {
            command.Parameters.AddWithValue("@CustomerId", CustomerId);
            command.Parameters.AddWithValue("@IssueDate", IssueDate);
            command.Parameters.AddWithValue("@DueDate", DueDate);
            command.Parameters.AddWithValue("@Subject", Subject);
            command.Parameters.AddWithValue("@SlipNumber", SlipNumber);
            command.Parameters.AddWithValue("@ItemsTotal", ItemsTotal);
            command.Parameters.AddWithValue("@Subtotal", SubTotal);
            command.Parameters.AddWithValue("@Tax", Tax);
            command.Parameters.AddWithValue("@PaidByDeposit", PaidByDeposit);
            command.Parameters.AddWithValue("@InvoiceTotal", InvoiceTotal);
            command.Parameters.AddWithValue("@Message", Message);
            command.Parameters.AddWithValue("@PaymentDate", PaymentDate);
            command.Parameters.AddWithValue("@InvoiceStatusId", InvoiceStatusId);
        }

        public bool TryUpdateInvoice(UnitOfWork? unitOfWork = null)
        {
            var res = UnitOfWork.ExecuteWithTransaction(uow =>
            {
                UpdateInvoice(uow);
                InvoiceItemClass.UpdateInvoiceItems(InvoiceId, InvoiceItems, uow);
                if (ItemsTotal - PaidByDeposit <= 0)
                {
                    BalanceClass.DeleteBalanceById(new IDs(invoiceId: InvoiceId, paymentId: null), uow);
                    DepositClass.TryUpdateDeposit(this, uow);
                }
                else
                {
                    if (PaidByDeposit >= 0)
                        DepositClass.TryUpdateDeposit(this, uow);
                    else
                        MessageBox.Show("前受金の更新に失敗しました。");
                }
                return true;
            }, unitOfWork);
            if (!res) return false;

            res = UnitOfWork.ExecuteWithTransaction(uow =>
            {
                var balances = BalanceClass.GetBalancesById(new IDs(invoiceId: InvoiceId, paymentId: null), new UnitOfWork());
                if (balances.Count > 1)
                {
                    MessageBox.Show("請求情報が重複しています。");
                    return false;
                }
                else
                {
                    BalanceClass.TryUpdateBalance(this, uow);
                    return true;
                }
            }, unitOfWork);
            if (!res) return false;
            return true;
        }

        public void UpdateInvoice(UnitOfWork unitOfWork)
        {
            string query = @"UPDATE T_INVOICE SET CUSTOMER_ID = @CustomerId, ISSUE_DATE = @IssueDate, DUE_DATE = @DueDate, SUBJECT = @Subject, SLIP_NUMBER = @SlipNumber, ITEMS_TOTAL = @ItemsTotal, SUBTOTAL = @Subtotal, TAX = @Tax, PAID_BY_DEPOSIT = @PaidByDeposit, TOTAL = @InvoiceTotal, MESSAGE = @Message, TRANSACTION_TYPE_ID = @TransactionTypeId, PAYMENT_DATE = @PaymentDate, INVOICE_STATUS_ID = @InvoiceStatusId WHERE INVOICE_ID = @InvoiceId";
            var command = unitOfWork.CreateCommand(query);
            AddParametersToCommand(command);
            command.Parameters.AddWithValue("@TransactionTypeId", InvoiceTotal > 0 ? TransactionTypeIdsProvider.BalanceId : TransactionTypeIdsProvider.DepositId);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.ExecuteNonQuery();
        }

        public void UpdateInvoiceStatus(int statusId, UnitOfWork unitOfWork)
        {
            string query = "UPDATE T_INVOICE SET INVOICE_STATUS_ID = @StatusId WHERE INVOICE_ID = @InvoiceId";
            var command = unitOfWork.CreateCommand(query);
            command.Parameters.AddWithValue("@StatusId", statusId);
            command.Parameters.AddWithValue("@InvoiceId", InvoiceId);
            command.ExecuteNonQuery();
        }

        public static bool DeleteInvoiceByInvoiceId(int id, UnitOfWork? unitOfWork = null)
        {
            return UnitOfWork.ExecuteWithTransaction(uow =>
            {
                InvoiceItemClass.DeleteInvoiceItemsByInvoiceId(id, uow);
                string query = "DELETE FROM T_INVOICE WHERE INVOICE_ID = @InvoiceId";
                var command = uow.CreateCommand(query);
                command.Parameters.AddWithValue("@InvoiceId", id);
                command.ExecuteNonQuery();
                DepositClass.DeleteDepositById(TypeOfID.Invoice, id, uow);
                BalanceClass.DeleteBalanceById(TypeOfID.Invoice, id, uow);
                return true;
            }, unitOfWork);
        }

        public InvoiceClass DeepClone()
        {
            var newInvoice = new InvoiceClass
            {
                InvoiceId = InvoiceId,
                CustomerId = CustomerId,
                IssueDate = IssueDate,
                DueDate = DueDate,
                Subject = Subject,
                SlipNumber = SlipNumber,
                PaidByDeposit = PaidByDeposit,
                Message = Message,
                TransactionTypeId = TransactionTypeId,
                PaymentDate = PaymentDate,
                InvoiceStatusId = InvoiceStatusId,
                CustomerName = CustomerName,
                InvoiceStatus = InvoiceStatus,
                IssueDateString = IssueDateString,
            };
            newInvoice.InvoiceItems = [];
            var items = InvoiceItems.OfType<InvoiceItemClass>().Select(i => i.DeepClone()).ToList();
            items.ForEach(i => newInvoice.InvoiceItems.Add(i));
            newInvoice.RecalculateTotals(); // 追加: クローン後合計再計算
            return newInvoice;
        }

        private static void UpdateCollectionEventHandlers<T>(ObservableCollection<T> oldCollection, ObservableCollection<T> newCollection, NotifyCollectionChangedEventHandler collectionChangedHandler, PropertyChangedEventHandler propertyChangedHandler) where T : INotifyPropertyChanged
        {
            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= collectionChangedHandler;
                foreach (var item in oldCollection)
                    item.PropertyChanged -= propertyChangedHandler;
            }
            if (newCollection != null)
            {
                newCollection.CollectionChanged += collectionChangedHandler;
                foreach (var item in newCollection)
                    item.PropertyChanged += propertyChangedHandler;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
