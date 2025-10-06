using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Invoice.Classes
{
    /// <summary>
    /// T_TRANSACTION_TYPE から取引種別IDを解決し、settings.ini で名称を上書きできるプロバイダ。
    /// 見つからない場合は Constants.TransactionTypes にフォールバックします。
    /// </summary>
    public static class TransactionTypeIdsProvider
    {
        private static readonly object _lock = new();
        private static bool _initialized;
        private static Dictionary<string, int> _nameToId = new(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<int, string> _idToName = new();

        private static string BalanceNameSetting => SettingsManager.Get("TransactionTypes.BalanceName") ?? "売掛金";
        private static string DepositNameSetting => SettingsManager.Get("TransactionTypes.DepositName") ?? "前受金";

        /// <summary>
        /// 取引種別 マスターを再読込
        /// </summary>
        public static void Reload()
        {
            lock (_lock)
            {
                _initialized = false;
                _nameToId.Clear();
                _idToName.Clear();
            }
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            lock (_lock)
            {
                if (_initialized) return;
                try
                {
                    string connectionString = ConnectionInfo.Builder.ConnectionString;
                    using var connection = new MySqlConnection(connectionString);
                    connection.Open();
                    using var command = new MySqlCommand("SELECT TRANSACTION_TYPE_ID, TRANSACTION_NAME FROM T_TRANSACTION_TYPE", connection);
                    using var reader = command.ExecuteReader();
                    var nameToId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    var idToName = new Dictionary<int, string>();
                    while (reader.Read())
                    {
                        int id = reader.GetInt32("TRANSACTION_TYPE_ID");
                        string name = reader.GetString("TRANSACTION_NAME");
                        if (!nameToId.ContainsKey(name)) nameToId[name] = id;
                        if (!idToName.ContainsKey(id)) idToName[id] = name;
                    }
                    _nameToId = nameToId;
                    _idToName = idToName;
                }
                catch
                {
                    // DB 取得失敗時はフォールバックのみで動作
                    _nameToId = new(StringComparer.OrdinalIgnoreCase);
                    _idToName = new();
                }
                finally
                {
                    _initialized = true;
                }
            }
        }

        private static int GetIdByPreferredName(string preferredName, int fallbackId)
        {
            EnsureInitialized();
            if (!string.IsNullOrWhiteSpace(preferredName) && _nameToId.TryGetValue(preferredName, out var id))
                return id;
            return fallbackId;
        }

        public static int BalanceId => GetIdByPreferredName(BalanceNameSetting, Constants.TransactionTypes.Balance);
        public static int DepositId => GetIdByPreferredName(DepositNameSetting, Constants.TransactionTypes.Deposit);

        public static bool TryGetIdByName(string name, out int id)
        {
            EnsureInitialized();
            return _nameToId.TryGetValue(name, out id);
        }

        public static bool TryGetNameById(int id, out string? name)
        {
            EnsureInitialized();
            if (_idToName.TryGetValue(id, out var n)) { name = n; return true; }
            name = null; return false;
        }
    }
}
