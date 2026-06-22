-- 4-G: 利用者残高はアプリ側 (CustomerClass.RecalculateAndPersistBalance) で一本化する。
-- T_BALANCE の AFTER トリガーは二重更新の原因になるため削除する。

DROP TRIGGER IF EXISTS update_customer_balance_after_insert;
DROP TRIGGER IF EXISTS update_customer_balance_after_update;
DROP TRIGGER IF EXISTS update_customer_balance_after_delete;
