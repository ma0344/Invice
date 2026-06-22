using MySqlConnector;
using System.Text;

namespace Invoice.Classes
{
    /// <summary>
    /// 起動時に DB の参照整合性・残高一致を検査する（4-K）。
    /// </summary>
    public static class DatabaseIntegrityChecker
    {
        public static IReadOnlyList<string> Check()
        {
            var issues = new List<string>();
            string connectionString = ConnectionInfo.Builder.ConnectionString;

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                issues.AddRange(CheckOrphanRows(connection, "孤児の請求明細", @"
                    SELECT COUNT(*) FROM T_INVOICE_ITEMS ii
                    LEFT JOIN T_INVOICE i ON ii.INVOICE_ID = i.INVOICE_ID
                    WHERE i.INVOICE_ID IS NULL"));

                issues.AddRange(CheckOrphanRows(connection, "孤児の入金（請求参照）", @"
                    SELECT COUNT(*) FROM T_PAYMENT p
                    LEFT JOIN T_INVOICE i ON p.INVOICE_ID = i.INVOICE_ID
                    WHERE p.INVOICE_ID IS NOT NULL AND i.INVOICE_ID IS NULL"));

                issues.AddRange(CheckOrphanRows(connection, "孤児の残高（請求参照）", @"
                    SELECT COUNT(*) FROM T_BALANCE b
                    LEFT JOIN T_INVOICE i ON b.INVOICE_ID = i.INVOICE_ID
                    WHERE b.INVOICE_ID IS NOT NULL AND i.INVOICE_ID IS NULL"));

                issues.AddRange(CheckOrphanRows(connection, "孤児の残高（入金参照）", @"
                    SELECT COUNT(*) FROM T_BALANCE b
                    LEFT JOIN T_PAYMENT p ON b.PAYMENT_ID = p.PAYMENT_ID
                    WHERE b.PAYMENT_ID IS NOT NULL AND p.PAYMENT_ID IS NULL"));

                issues.AddRange(CheckOrphanRows(connection, "無効な利用者IDの請求", @"
                    SELECT COUNT(*) FROM T_INVOICE i
                    LEFT JOIN T_CUSTOMER c ON i.CUSTOMER_ID = c.CUSTOMER_ID
                    WHERE c.CUSTOMER_ID IS NULL"));

                issues.AddRange(CheckBalanceMismatches(connection));
            }
            catch (Exception ex)
            {
                issues.Add($"データベース整合性チェックに失敗しました: {ex.Message}");
            }

            return issues;
        }

        public static void RunAndNotify()
        {
            var issues = Check();
            if (issues.Count == 0)
                return;

            var message = new StringBuilder();
            message.AppendLine("データベース整合性チェックで問題が見つかりました:");
            foreach (var issue in issues)
                message.AppendLine($"・{issue}");

            DomainEvents.RaiseInfo(message.ToString().TrimEnd());
        }

        private static IEnumerable<string> CheckOrphanRows(MySqlConnection connection, string label, string countQuery)
        {
            using var command = new MySqlCommand(countQuery, connection);
            var count = Convert.ToInt32(command.ExecuteScalar());
            if (count > 0)
                yield return $"{label}: {count} 件";
        }

        private static IEnumerable<string> CheckBalanceMismatches(MySqlConnection connection)
        {
            const string query = @"
                SELECT c.CUSTOMER_ID, c.CUSTOMER_NAME, c.BALANCE AS stored_balance,
                       COALESCE(SUM(
                           CASE
                               WHEN b.DEBIT_OR_CREDIT_ID = 1 THEN b.TRANSACTION_AMOUNT
                               WHEN b.DEBIT_OR_CREDIT_ID = 2 THEN -b.TRANSACTION_AMOUNT
                               ELSE 0
                           END), 0) AS calc_balance
                FROM T_CUSTOMER c
                LEFT JOIN T_BALANCE b ON c.CUSTOMER_ID = b.CUSTOMER_ID
                GROUP BY c.CUSTOMER_ID, c.CUSTOMER_NAME, c.BALANCE
                HAVING stored_balance <> calc_balance";

            using var command = new MySqlCommand(query, connection);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var customerId = reader.GetInt32("CUSTOMER_ID");
                var customerName = reader.GetString("CUSTOMER_NAME");
                var stored = reader.GetInt32("stored_balance");
                var calc = reader.GetInt32("calc_balance");
                yield return $"利用者残高不一致 (ID={customerId} {customerName}): 保存値={stored}, 計算値={calc}";
            }
        }
    }
}
