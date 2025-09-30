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

        public int? SubTotal
        {
            get
            {
                return InvoiceItems.Sum(x => x.Quantity * x.UnitPrice);
            }
        }
        public int? Tax
        {
            get
            {
                return InvoiceItems.Sum(x => x.Tax);
            }
        }
        public int? InvoiceTotal
        {
            get
            {
                return TransactionTypeId == 1 ? ItemsTotal : ItemsTotal - PaydByDeposit;
            }
        }
        public int ItemsTotal
        {
            get
            {
                var itemsTotal = InvoiceItems.Sum(x => x.Quantity * x.UnitPrice + x.Tax);
                PaydByDeposit = DepositUntilIssueDate < itemsTotal ? DepositUntilIssueDate : itemsTotal;
                return itemsTotal;
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
                //RecalculateTotals();
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

        private int _DepositUntilIssueDate = 0;
        public int DepositUntilIssueDate
        {
            get => _DepositUntilIssueDate;
            set
            {
                _DepositUntilIssueDate = value;
                var afterPaydDeposit = DepositUntilIssueDate - ItemsTotal;// 当該請求額支払後 前受残高
                PaydByDeposit = afterPaydDeposit <= 0 ? DepositUntilIssueDate : ItemsTotal;// 前受精算額（前受が不足の場合は前受
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
                //if (_InvoiceItems != null)
                //{
                //    // 古いコレクションのイベント購読を解除
                //    _InvoiceItems.CollectionChanged -= InvoiceItems_CollectionChanged;
                //    foreach (var item in _InvoiceItems)
                //    {
                //        item.PropertyChanged -= InvoiceItem_PropertyChanged;
                //    }
                //}

                _InvoiceItems = value;

                //if (_InvoiceItems != null)
                //{
                //    // 新しいコレクションのイベントを購読
                //    _InvoiceItems.CollectionChanged += InvoiceItems_CollectionChanged;
                //    foreach (var item in _InvoiceItems)
                //    {
                //        item.PropertyChanged += InvoiceItem_PropertyChanged;
                //    }
                //}

                OnPropertyChanged(nameof(InvoiceItems));
                RecalculateTotals(); // 再計算
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

            RecalculateTotals(); // 再計算
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
                RecalculateTotals(); // 再計算
                OnPropertyChanged(nameof(InvoiceItems));
            }
        }

        public void RecalculateTotals()
        {
            NotifyPropertiesChanged(nameof(DepositUntilIssueDate), nameof(InvoiceItems), nameof(Tax), nameof(PaydByDeposit), nameof(ItemsTotal), nameof(InvoiceTotal));
        }


        private void NotifyPropertiesChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        public static List<InvoiceClass> GetAllInvoice()
        {
            var invoices = new List<InvoiceClass>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
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
                var invoice = new InvoiceClass()
                {
                    InvoiceId = reader.GetInt32("INVOICE_ID"),
                    CustomerId = reader.GetInt32("CUSTOMER_ID"),
                    IssueDate = reader.IsDBNull("ISSUE_DATE") ? null : reader.GetDateTime("ISSUE_DATE"),
                    DueDate = reader.IsDBNull("DUE_DATE") ? null : reader.GetDateTime("DUE_DATE"),
                    Subject = reader.IsDBNull("SUBJECT") ? null : reader.GetString("SUBJECT"),
                    SlipNumber = reader.IsDBNull("SLIP_NUMBER") ? null : reader.GetString("SLIP_NUMBER"),
                    PaydByDeposit = reader.GetInt32("PAYD_BY_DEPOSIT"),
                    Message = reader.IsDBNull("MESSAGE") ? null : reader.GetString("MESSAGE"),
                    TransactionTypeId = reader.IsDBNull("TRANSACTION_TYPE_ID") ? null : reader.GetInt32("TRANSACTION_TYPE_ID"),
                    PaymentDate = reader.IsDBNull("PAYMENT_DATE") ? null : reader.GetDateTime("PAYMENT_DATE"),
                    InvoiceStatusId = reader.GetInt32("INVOICE_STATUS_ID"),
                };
                invoice.IssueDateString = invoice.IssueDate?.ToShortDateString() ?? null;
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
                if (TransactionTypeId == 1)
                    BalanceClass.TryAddBalance(this, uow);
                else if (TransactionTypeId == 2)
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

            string query = @"INSERT INTO T_INVOICE (CUSTOMER_ID, ISSUE_DATE, DUE_DATE, SUBJECT, SLIP_NUMBER, ITEMS_TOTAL, SUBTOTAL, TAX, PAYD_BY_DEPOSIT, TOTAL, MESSAGE, TRANSACTION_TYPE_ID, PAYMENT_DATE, INVOICE_STATUS_ID) " + "\r\n" + "VALUES (@CustomerId, @IssueDate, @DueDate, @Subject, @SlipNumber, @ItemsTotal, @Subtotal, @Tax, @PaydByDeposit, @InvoiceTotal, @Message, @TransactionTypeId, @PaymentDate, @InvoiceStatusId)";

            var command = unitOfWork.CreateCommand(query);
            AddParamatersToCommand(command);
            command.Parameters.AddWithValue("@TransactionTypeId", TransactionTypeId);
            command.ExecuteNonQuery();
            InvoiceId = (int)command.LastInsertedId;

        }

        private void AddParamatersToCommand(TrackedCommand command)
        {
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
            command.Parameters.AddWithValue("@PaymentDate", PaymentDate);
            command.Parameters.AddWithValue("@InvoiceStatusId", InvoiceStatusId);
        }

        public bool TryUpdateInvoice(UnitOfWork? unitOfWork = null)
        {

            var res = UnitOfWork.ExecuteWithTransaction(uow =>
            {
                UpdateInvoice(uow);
                // 請求額に関わらず、請求情報は必ず存在する。
                InvoiceItemClass.UpdateInvoiceItems(InvoiceId, InvoiceItems, uow);
                if (ItemsTotal - PaydByDeposit <= 0) // *A
                {   /// ItemsTotal - PaydByDeposit <= 0 は 0円請求書
                    /// *A UpdateInvoiceにて、請求額が0円に更新された＝全額を前受金で処理するための更新であるため、
                    ///    請求情報はInvoiceTotalが0円、PaydByDepositが1円以上のものとなり
                    /// *B InvoiceClass経由のT_BALANCEは不要となる。
                    /// *C 売掛請求と前受清算が同時に存在する（＝更新前の請求情報が「前受残高不足の請求書」）更新の場合は、
                    ///    前受情報を更新・追加（TryUpdateDeposit()内で行われる）し、
                    ///    DepositClass内において、T_BALANCEも追加、もしくは更新を行う。
                    /// 更新前の請求情報が「売掛のみの請求書」を更新する場合、
                    /// 前受情報とDepositClass経由のT_BALANCEは存在しない＝ doposit == null ため何もしない *F
                    BalanceClass.DeleteBalanceById(new IDs(invoiceId: InvoiceId, depositId: null), uow); // *B
                    DepositClass.TryUpdateDeposit(this, uow); // *D
                    // *F
                }
                else //
                {/// ItemsTotal - PaydByDeposit > 0 は 請求書
                 /// 請求額が1円以上に更新された請求書のパターンは
                 /// 売掛のみ　　　(PaydByDeposit == 0) *f
                 /// 売掛と前受金　(PaydByDeposit > 0) *i
                 /// いずれの場合も必ずInvoiceClass経由のT_BALANCEが必要
                    // T_DEPOSITのレコードはPaydByDepositの値により(*y)、削除、追加、更新(TryUpdateDeposit()内で行われる)のいずれかを行う必要がある。
                    if (PaydByDeposit >= 0) // *y
                        DepositClass.TryUpdateDeposit(this, uow); // *h
                    else
                        MessageBox.Show("前受金の更新に失敗しました。");
                }
                return true;
            }, unitOfWork);

            if (!res) return false;

            var balance = BalanceClass.GetBalancesById(new IDs(invoiceId: InvoiceId, depositId: null), new UnitOfWork());
            if (balance.Count <= 1) // *x
            {
                BalanceClass.TryUpdateBalance(this);
                return true;
            }
            else
                MessageBox.Show("請求情報が重複しています。");
            return false;
        }


        public void UpdateInvoice(UnitOfWork unitOfWork)
        {
            string query = @"UPDATE T_INVOICE SET CUSTOMER_ID = @CustomerId, ISSUE_DATE = @IssueDate, DUE_DATE = @DueDate, SUBJECT = @Subject, SLIP_NUMBER = @SlipNumber, ITEMS_TOTAL = @ItemsTotal, SUBTOTAL = @Subtotal, TAX = @Tax, PAYD_BY_DEPOSIT = @PaydByDeposit, TOTAL = @InvoiceTotal, MESSAGE = @Message, TRANSACTION_TYPE_ID = @TransactionTypeId, PAYMENT_DATE = @PaymentDate, INVOICE_STATUS_ID = @InvoiceStatusId WHERE INVOICE_ID = @InvoiceId";
            var command = unitOfWork.CreateCommand(query);
            AddParamatersToCommand(command);
            command.Parameters.AddWithValue("@TransactionTypeId", InvoiceTotal > 0 ? 1 : 2);
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
                PaydByDeposit = PaydByDeposit,
                Message = Message,
                TransactionTypeId = TransactionTypeId,
                PaymentDate = PaymentDate,
                InvoiceStatusId = InvoiceStatusId,
                CustomerName = CustomerName,
                InvoiceStatus = InvoiceStatus,
                IssueDateString = IssueDateString,
                // 必要に応じて他のプロパティもコピー
            };
            //var items = InvoiceItemClass.GetInvoiceItemsByInvoiceId(InvoiceId);
            newInvoice.InvoiceItems = [];

            var items = InvoiceItems.OfType<InvoiceItemClass>().Select(i => i.DeepClone()).ToList();
            items.ForEach(i => newInvoice.InvoiceItems.Add(i));

            return newInvoice;
        }

        private static void UpdateCollectionEventHandlers<T>(ObservableCollection<T> oldCollection, ObservableCollection<T> newCollection, NotifyCollectionChangedEventHandler collectionChangedHandler, PropertyChangedEventHandler propertyChangedHandler) where T : INotifyPropertyChanged
        {
            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= collectionChangedHandler;
                foreach (var item in oldCollection)
                {
                    item.PropertyChanged -= propertyChangedHandler;
                }
            }

            if (newCollection != null)
            {
                newCollection.CollectionChanged += collectionChangedHandler;
                foreach (var item in newCollection)
                {
                    item.PropertyChanged += propertyChangedHandler;
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
    }
}
