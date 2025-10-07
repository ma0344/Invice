using MySqlConnector;
using System;
using System.Collections.Generic;

namespace Invoice.Classes
{
    /// <summary>
    /// T_APP_SETTINGS への書き込み用リポジトリ。
    /// </summary>
    public static class AppSettingsRepository
    {
        public static void Upsert(string key, string? value, UnitOfWork? unitOfWork = null)
        {
            UnitOfWork.ExecuteWithTransaction(uow =>
            {
                var cmd = uow.CreateCommand("INSERT INTO T_APP_SETTINGS(`KEY`,`VALUE`) VALUES(@k,@v) ON DUPLICATE KEY UPDATE `VALUE`=@v");
                cmd.Parameters.AddWithValue("@k", key);
                cmd.Parameters.AddWithValue("@v", value ?? string.Empty);
                cmd.ExecuteNonQuery();
                return true;
            }, unitOfWork);
        }

        public static void UpsertBulk(IDictionary<string, string?> values, UnitOfWork? unitOfWork = null)
        {
            UnitOfWork.ExecuteWithTransaction(uow =>
            {
                foreach (var kv in values)
                {
                    var cmd = uow.CreateCommand("INSERT INTO T_APP_SETTINGS(`KEY`,`VALUE`) VALUES(@k,@v) ON DUPLICATE KEY UPDATE `VALUE`=@v");
                    cmd.Parameters.AddWithValue("@k", kv.Key);
                    cmd.Parameters.AddWithValue("@v", kv.Value ?? string.Empty);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }, unitOfWork);
        }
    }
}
