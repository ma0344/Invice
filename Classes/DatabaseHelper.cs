using MySqlConnector;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Invoice.Classes
{
    public interface ILoggable
    {
    }

    public static class LoggableExtensions
    {
        private static readonly bool sw = false;

        public static void LogMethodEntry(this ILoggable loggable, [CallerMemberName] string methodName = "")
        {
            if (!sw) return;
            if (methodName == "OnPropertyChanged") return;
            Debug.WriteLine($"Entered: {loggable.GetType().Name}.{methodName}");
        }

        public static void LogMethodExit(this ILoggable loggable, [CallerMemberName] string methodName = "")
        {
            if (!sw) return;
            if (methodName == "OnPropertyChanged") return;
            Debug.WriteLine($"Exited: {loggable.GetType().Name}.{methodName}");
        }
    }



    public class UnitOfWork : IDisposable
    {
        private readonly MySqlConnection _connection;
        private readonly MySqlTransaction _transaction;
        private readonly List<string> _executedParams = new();
        private readonly List<string> _executedCommands = new(); // 実行されたコマンドを記録

        public UnitOfWork()
        {
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            _connection = new MySqlConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public TrackedCommand CreateCommand(string commandText = "", [CallerMemberName] string callerName = "")
        {
            var command = _connection.CreateCommand();
            Debug.WriteLine(callerName);
            command.Transaction = _transaction;
            command.CommandText = commandText;
            // コマンドの実行時に記録するためのイベントを設定
            return new TrackedCommand(command, _executedCommands, _executedParams);
        }

        public void Commit()
        {
            // コミット前に実行されたコマンドをログに出力
            Debug.WriteLine("---------------------------------");
            Debug.WriteLine("以下のコマンドがコミットされます:");
            _executedParams.ForEach(cmd => Debug.WriteLine($"{cmd}\r\n"));
            Debug.WriteLine("---------------------------------");
            _transaction.Commit();
        }

        public void Rollback()
        {
            // コミット前に実行されたコマンドをログに出力
            Debug.WriteLine("以下のコマンドがロールバックされます:");
            foreach (var cmd in _executedCommands)
            {
                Debug.WriteLine(cmd);
            }

            _transaction.Rollback();
        }

        public void Dispose()
        {
            _transaction.Dispose();
            _connection.Dispose();
        }

        public static bool ExecuteWithTransaction(Func<UnitOfWork, bool> action, UnitOfWork? unitOfWork = null)
        {
            var uow = unitOfWork ?? new UnitOfWork();
            try
            {
                // 処理を実行
                bool result = action(uow);

                // 呼び出し元がトランザクションを管理していない場合、コミット
                if (unitOfWork == null && result)
                {
                    uow.Commit();
                }

                return result;
            }
            catch (Exception e)
            {
                // エラー発生時にロールバック
                if (unitOfWork == null)
                {
                    uow.Rollback();
                }

                // UIとの分離: ドメインイベントで通知
                DomainEvents.RaiseError($"エラーが発生しました: {e.Message}", e);
                return false;
            }
            finally
            {
                // 呼び出し元がトランザクションを管理していない場合、UnitOfWork を破棄
                if (unitOfWork == null)
                {
                    uow.Dispose();
                }
            }
        }

    }


    public class TrackedCommand
    {
        private readonly MySqlCommand _innerCommand;
        private readonly List<string> _executedCommands;
        private readonly List<string> _executedParams;

        public TrackedCommand(MySqlCommand innerCommand, List<string> executedCommands, List<string> executedParams)
        {
            _innerCommand = innerCommand;
            _executedCommands = executedCommands;
            _executedParams = executedParams;
        }

        public void AddParameter(string parameterName, object value)
        {
            _innerCommand.Parameters.AddWithValue(parameterName, value);
            _executedParams.Add(_innerCommand.Parameters.ToString() ?? "");
        }

        public int ExecuteNonQuery()
        {
            var cmdText = _innerCommand.CommandText.ToString();
            foreach (MySqlParameter parameter in Parameters)
            {
                var value = parameter.Value?.ToString();
                cmdText = cmdText.Replace(parameter.ParameterName.ToString(), string.IsNullOrWhiteSpace(value) ? "null" : value);
            }
            _executedCommands.Add(_innerCommand.CommandText);
            _executedParams.Add(cmdText);
            
            return _innerCommand.ExecuteNonQuery();
        }

        public MySqlDataReader ExecuteReader()
        {
            var cmdText = _innerCommand.CommandText.ToString();
            foreach (MySqlParameter parameter in Parameters)
            {
                var value = parameter.Value?.ToString();
                cmdText = cmdText.Replace(parameter.ParameterName.ToString(), string.IsNullOrWhiteSpace(value) ? "null" : value);
            }
            _executedCommands.Add(_innerCommand.CommandText);
            _executedParams.Add(cmdText);
            return _innerCommand.ExecuteReader();
        }

        public object ExecuteScalar()
        {
            _executedCommands.Add(_innerCommand.CommandText);
            return _innerCommand.ExecuteScalar();
        }

        public string CommandText
        {
            get => _innerCommand.CommandText;
            set => _innerCommand.CommandText = value;
        }

        public long LastInsertedId => _innerCommand.LastInsertedId;

        public MySqlParameterCollection Parameters => _innerCommand.Parameters;
    }

    public class IDs
    {
        public int? CustomerId { get; set; } = 0;
        public int? InvoiceId { get; set; } = 0;
        public int? PaymentId { get; set; } = 0;
        public int? DepositId { get; set; } = 0;
        public int? BalanceId { get; set; } = 0;
        public IDs(int? customerId = 0, int? invoiceId = 0, int? paymentId = 0, int? depositId = 0, int? balanceId = 0)
        {
            CustomerId = customerId;
            InvoiceId = invoiceId;
            PaymentId = paymentId;
            DepositId = depositId;
            BalanceId = balanceId;
        }
    }

    public enum TypeOfID
    {
        Instans = 0,
        Customer = 1,
        Invoice = 2,
        Payment = 3,
        Deposit = 4
    }


    public class InvoiceItemsClass_ : ObservableCollection<InvoiceItemClass>, INotifyPropertyChanged, ILoggable
    {

        public int ItemsTotal { get => Items.Sum(i => i.ItemTotal); }

        public int Tax { get => Items.Sum(i => i.Tax); }

        public int ItemsSubTotal { get => Items.Sum(i => i.ItemSubTotal); }


        public InvoiceItemsClass_(List<InvoiceItemClass>? items = null)
        {
            CollectionChanged += InvoiceItems_CollectionChanged;
            if (items != null)
            {
                AddRange(items);
            }
        }
        public new void Add(InvoiceItemClass item)
        {

            base.Add(item);
            item.PropertyChanged += Item_PropertyChanged;
        }
        public void AddRange(List<InvoiceItemClass> items)
        {

            foreach (var item in items)
            {
                base.Add(item);
                item.PropertyChanged += Item_PropertyChanged;
            }
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {

            PropertyChanged?.Invoke(this, e);
        }

        private void InvoiceItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {

            base.OnCollectionChanged(e);
            if (e.NewItems != null)
            {
                foreach (InvoiceItemClass item in e.NewItems)
                {
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

            if (e.PropertyName == nameof(InvoiceItemClass.ItemTotal) || e.PropertyName == nameof(InvoiceItemClass.Tax))
            {
                RecalculateTotals(); // 再計算
            }
        }
        private void RecalculateTotals()
        {

            OnPropertyChanged(nameof(ItemsTotal));
            OnPropertyChanged(nameof(Tax));
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class QueryBuilder
    {
        public static string StringBuilder(string command = "SELECT *", string tableName = "", TypeOfID type = 0)
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
        public static TrackedCommand CommandBuilder(string commandStr = "SELECT *", string tableName = "", TypeOfID type = 0, int id = 0, UnitOfWork? unitOfWork = null)
        {

            string query = StringBuilder(commandStr, tableName, type);
            var command = unitOfWork?.CreateCommand(query) ?? new UnitOfWork().CreateCommand(query);
            if (type != 0)
            {
                
                command.Parameters.AddWithValue("@id", id);
            }
            return command;
        }
    }

    public static class CommandBuilder
    {
        public static TrackedCommand Builder(IDs ids, string query, UnitOfWork unitOfWork, [CallerMemberName] string callerName = "")
        {
            string condition = "";
            if (ids.InvoiceId != 0)
            {
                condition += ids.InvoiceId != null ? "INVOICE_ID = @InvoiceId" : "INVOICE_ID IS null";
            }
            if (ids.PaymentId != 0)
            {
                if (condition != "") condition += " AND ";
                condition += ids.PaymentId != null ? "PAYMENT_ID = @PaymentId" : "PAYMENT_ID IS null";
            }
            if (ids.DepositId != 0)
            {
                if (condition != "") condition += " AND ";
                condition += ids.DepositId != null ? "DEPOSIT_ID = @DepositId" : "DEPOSIT_ID IS null";
            }
            if (ids.BalanceId != 0)
            {
                if (condition != "") condition += " AND ";
                condition += ids.BalanceId != null ? "BALANCE_ID = @BalanceId" : "BALANCE_ID IS null";
            }
            if (condition == "")
            {
                throw new ArgumentException("At least one parameter must be provided.");
            }
            query += condition;

            
            var command = unitOfWork.CreateCommand();
            command.CommandText = query;
            // 引数がnullの場合は条件としてnullを追加する
            if (ids.InvoiceId != 0 && ids.InvoiceId != null)
            {
                command.Parameters.AddWithValue("@InvoiceId", ids.InvoiceId);
            }
            if (ids.PaymentId != 0 && ids.PaymentId != null)
            {
                command.Parameters.AddWithValue("@PaymentId", ids.PaymentId);
            }
            if (ids.DepositId != 0 && ids.DepositId != null)
            {
                command.Parameters.AddWithValue("@DepositId", ids.DepositId);
            }
            if (ids.BalanceId != 0 && ids.BalanceId != null)
            {
                command.Parameters.AddWithValue("@BalanceId", ids.BalanceId);
            }

            return command;
        }

    }



}