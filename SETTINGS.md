# アプリ設定ファイル仕様（settings.ini / DB）

本ドキュメントは、アプリ設定値の取得方式を示します。現在は「DB 優先（推奨） > 設定ファイル > 既定値」の順で解決します。

- DB: `T_APP_SETTINGS (KEY, VALUE)` から読み込み（推奨）
- 設定ファイル: `settings.ini`（任意、存在しなくても可）
- 既定値: コード内のフォールバック

プロバイダ構成

- `Classes/AppSettingsProvider.cs`
  - DBの `T_APP_SETTINGS` を読み込みキャッシュ
  - 強く型付けされたアクセサを提供
  - `Reload()` で再読込
- `Classes/SettingsManager.cs`
  - INI ライクな Key=Value を読み込み（互換用途）
- その他 Provider
  - `TransactionTypeIdsProvider`, `InvoiceStatusIdsProvider`

テーブル定義（推奨）

```
CREATE TABLE T_APP_SETTINGS (
  `KEY`   VARCHAR(100) PRIMARY KEY,
  `VALUE` VARCHAR(500) NULL,
  `UPDATED_AT` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
```

キー一覧（DB/ini 共通）

- Invoice.DueDay
  - 型: int、既定: 15
  - 用途: 請求期限日（発行月の翌月のX日）
- Accounting.DebitAccountCode.Balance / Accounting.DebitAccountCode.Deposit
  - 型: int、既定: 134 / 310
  - 用途: 借方科目コード（売掛/前受）
- Accounting.DepartmentCode / Accounting.TaxHandlingCode / Accounting.TaxRate
  - 型: int、既定: 211 / 3 / 10
  - 用途: 部門コード、税処理コード、税率
- Items.Special.InsuranceAdjustmentCode / Items.Special.InsuranceAdjustmentName
  - 型: string、既定: "99" / "特定障害者特別給付として国保連請求済み"
  - 用途: 特例品目の識別（会計CSVの集計用）
- OutputDirectory（ini のみ）
  - 型: string、既定: デスクトップ
  - 用途: PDF 出力先（UI 操作で上書き可能にする場合は別テーブル推奨）

反映タイミング

- DB値変更後は `AppSettingsProvider.Reload()` で即時反映（UIから呼べるボタンを用意可）
- 取引種別/ステータスの名称変更は各 Provider の `Reload()` を実行

注意事項

- 値の妥当性はアプリ側でガード（例: `Invoice.DueDay` は対象月の日数にクリップ）
- 機微情報は設定値に含めない（接続文字列は別管理）

将来拡張

- 項目ごとに `SCOPE` 列（Global/Company/Tenant/User）を追加し、スコープで出し分け
- 変更履歴テーブルで監査性を担保
- UI に設定画面を追加し、DB へ直接保存
