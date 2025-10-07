using MySqlConnector;
using System;
using System.Collections.Generic;

namespace Invoice.Classes
{
    /// <summary>
    /// アプリ設定をDB(T_APP_SETTINGS: KEY, VALUE)から読み込むプロバイダ。
    /// 取得できない場合はコード既定値にフォールバックします。
    /// </summary>
    public static class AppSettingsProvider
    {
        private static readonly object _lock = new();
        private static bool _initialized;
        private static Dictionary<string, string> _kv = new(StringComparer.OrdinalIgnoreCase);

        public static void Reload()
        {
            lock (_lock)
            {
                _initialized = false;
                _kv.Clear();
            }
        }

        private static void EnsureLoaded()
        {
            if (_initialized) return;
            lock (_lock)
            {
                if (_initialized) return;
                try
                {
                    string cs = ConnectionInfo.Builder.ConnectionString;
                    using var con = new MySqlConnection(cs);
                    con.Open();
                    using var cmd = new MySqlCommand("SELECT `KEY`, `VALUE` FROM T_APP_SETTINGS", con);
                    using var reader = cmd.ExecuteReader();
                    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    while (reader.Read())
                    {
                        var key = reader.GetString(0);
                        var val = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        dict[key] = val;
                    }
                    _kv = dict;
                }
                catch
                {
                    _kv = new(StringComparer.OrdinalIgnoreCase);
                }
                finally
                {
                    _initialized = true;
                }
            }
        }

        private static string? GetString(string key)
        {
            EnsureLoaded();
            return _kv.TryGetValue(key, out var v) ? v : null;
        }
        private static int GetInt(string key, int fallback)
        {
            var s = GetString(key);
            return int.TryParse(s, out var i) ? i : fallback;
        }

        // Keys and strongly-typed accessors
        public static int InvoiceDueDay => GetInt("Invoice.DueDay", 15);

        public static int AccountingDebitAccountCodeBalance => GetInt("Accounting.DebitAccountCode.Balance", 134);
        public static int AccountingDebitAccountCodeDeposit => GetInt("Accounting.DebitAccountCode.Deposit", 310);
        public static int AccountingDepartmentCode => GetInt("Accounting.DepartmentCode", 211);
        public static int AccountingTaxHandlingCode => GetInt("Accounting.TaxHandlingCode", 3);
        public static int AccountingTaxRate => GetInt("Accounting.TaxRate", 10);

        public static string SpecialItemCode => GetString("Items.Special.InsuranceAdjustmentCode") ?? "99";
        public static string SpecialItemName => GetString("Items.Special.InsuranceAdjustmentName") ?? "特定障害者特別給付として国保連請求済み";
    }
}
