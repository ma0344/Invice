using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoice.Classes
{
    // T_CUSTOMER テーブルに対応するクラス
    public class CustomerClass : INotifyPropertyChanged, ILoggable
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
            string connectionString = ConnectionInfo.Builder.ConnectionString;
            using var connection = new MySqlConnection(connectionString);
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
            UnitOfWork.ExecuteWithTransaction(uow =>
            {
                var query = "UPDATE T_CUSTOMER SET CUSTOMER_NAME=@name, CUSTOMER_KANA=@kana, BALANCE=@balance, VISIBLE=@visible WHERE CUSTOMER_ID=@id";
                var command = uow.CreateCommand(query);
                command.Parameters.AddWithValue("@name", CustomerName);
                command.Parameters.AddWithValue("@kana", CustomerKana);
                command.Parameters.AddWithValue("@balance", CustomerBalance);
                command.Parameters.Add("@visible", MySqlDbType.Bit).Value = CustomerVisible;
                command.Parameters.AddWithValue("@id", CustomerId);
                command.ExecuteNonQuery();
                return true;
            }, null);
            
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
            using var command = new MySqlCommand("INSERT INTO T_CUSTOMER (CUSTOMER_NAME, CUSTOMER_KANA, BALANCE, VISIBLE) " + "\r\n" + "VALUES (@name, @kana, @balance, @visible)", connection);
            command.Parameters.AddWithValue("@name", CustomerName);
            command.Parameters.AddWithValue("@kana", CustomerKana);
            command.Parameters.AddWithValue("@balance", CustomerBalance);
            command.Parameters.Add("@visible", MySqlDbType.Bit).Value = true;
            command.ExecuteNonQuery();
        }

        // 追加: 残高再計算 & 反映（T_BALANCEをソースオブトゥルースとする）
        public static void RecalculateAndPersistBalance(int customerId, UnitOfWork uow)
        {
            // T_BALANCE が無い場合 0
            var select = uow.CreateCommand(
                @"SELECT COALESCE(SUM(CASE WHEN DEBIT_OR_CREDIT_ID = 1 THEN TRANSACTION_AMOUNT WHEN DEBIT_OR_CREDIT_ID = 2 THEN -TRANSACTION_AMOUNT ELSE 0 END),0) FROM T_BALANCE WHERE CUSTOMER_ID = @cid");
            select.Parameters.AddWithValue("@cid", customerId);
            var result = select.ExecuteScalar();
            int newBalance = 0;
            if (result != null && result != DBNull.Value) newBalance = Convert.ToInt32(result);

            var update = uow.CreateCommand("UPDATE T_CUSTOMER SET BALANCE=@bal WHERE CUSTOMER_ID=@cid");
            update.Parameters.AddWithValue("@bal", newBalance);
            update.Parameters.AddWithValue("@cid", customerId);
            update.ExecuteNonQuery();
        }

    }

}
