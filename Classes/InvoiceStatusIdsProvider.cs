using MySqlConnector;
using System;
using System.Collections.Generic;

namespace Invoice.Classes
{
    /// <summary>
    /// T_INVOICE_STATUS からステータス名→ID を解決。settings.ini の名称で上書き可能。
    /// 見つからない場合は名称既定値（日本語）で検索し、それでも無ければ 0 を返す。
    /// </summary>
    public static class InvoiceStatusIdsProvider
    {
        private static readonly object _lock = new();
        private static bool _initialized;
        private static Dictionary<string, int> _nameToId = new(StringComparer.OrdinalIgnoreCase);

        private static string DraftNameSetting => SettingsManager.Get("InvoiceStatus.DraftName") ?? "作成中";
        private static string BilledNameSetting => SettingsManager.Get("InvoiceStatus.BilledName") ?? "請求済";
        private static string DepositedNameSetting => SettingsManager.Get("InvoiceStatus.DepositedName") ?? "入金済";
        private static string PrepaidNameSetting => SettingsManager.Get("InvoiceStatus.PrepaidName") ?? "前受済";

        public static void Reload()
        {
            lock (_lock)
            {
                _initialized = false;
                _nameToId.Clear();
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
                    using var command = new MySqlCommand("SELECT INVOICE_STATUS_ID, INVOICE_STATUS FROM T_INVOICE_STATUS", connection);
                    using var reader = command.ExecuteReader();
                    var nameToId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    while (reader.Read())
                    {
                        int id = reader.GetInt32("INVOICE_STATUS_ID");
                        string name = reader.GetString("INVOICE_STATUS");
                        if (!nameToId.ContainsKey(name)) nameToId[name] = id;
                    }
                    _nameToId = nameToId;
                }
                catch
                {
                    _nameToId = new(StringComparer.OrdinalIgnoreCase);
                }
                finally
                {
                    _initialized = true;
                }
            }
        }

        private static int GetIdByPreferredName(string preferredName)
        {
            EnsureInitialized();
            if (!string.IsNullOrWhiteSpace(preferredName) && _nameToId.TryGetValue(preferredName, out var id))
                return id;
            return 0;
        }

        public static int DraftId => GetIdByPreferredName(DraftNameSetting);
        public static int BilledId => GetIdByPreferredName(BilledNameSetting);
        public static int DepositedId => GetIdByPreferredName(DepositedNameSetting);
        public static int PrepaidId => GetIdByPreferredName(PrepaidNameSetting);
    }
}
