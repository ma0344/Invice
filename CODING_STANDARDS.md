# コーディング規約クイックリファレンス

## 概要
このドキュメントは、Invoiceプロジェクトで遵守すべきコーディング規約とベストプラクティスをまとめたものです。

## 1. 命名規則

### クラス名
- **PascalCase** を使用
- 名詞または名詞句
```csharp
// 良い例
public class CustomerClass { }
public class PaymentViewModel { }

// 悪い例
public class customer { }  // 小文字
public class DoPayment { }  // 動詞から始まる
```

### メソッド名
- **PascalCase** を使用
- 動詞または動詞句
```csharp
// 良い例
public void SaveCustomer() { }
public string GetTaxTypeName(int id) { }

// 悪い例
public void saveCustomer() { }  // camelCase
public string getTaxTypeName(int id) { }  // camelCase
```

### プロパティ名
- **PascalCase** を使用
- 名詞、名詞句、または形容詞
```csharp
// 良い例
public int CustomerId { get; set; }
public string CustomerName { get; set; }
public bool IsVisible { get; set; }

// 悪い例
public int customerId { get; set; }  // camelCase
```

### プライベートフィールド
- **_camelCase** (アンダースコアで始まる) を使用
```csharp
// 良い例
private readonly MySqlConnection _connection;
private List<string> _executedParams;

// 悪い例
private readonly MySqlConnection connection;  // アンダースコアなし
private List<string> _executedPalams;  // スペルミス
```

### 定数
- **PascalCase** を使用（静的読み取り専用フィールドの場合）
- **UPPER_CASE** も許容（真の定数の場合）
```csharp
// 良い例
public const int MaxRetryCount = 3;
public static readonly string DefaultConnectionString = "...";

// C#では通常PascalCaseを使用
public static class TransactionTypes
{
    public const int Balance = 1;
    public const int Deposit = 2;
}
```

### パラメータとローカル変数
- **camelCase** を使用
```csharp
// 良い例
public void ProcessPayment(int customerId, decimal paymentAmount)
{
    var totalAmount = paymentAmount * 1.1m;
}
```

## 2. コード構成

### usingディレクティブ
- ファイルの先頭に配置
- System名前空間を最初に
- アルファベット順に並べる

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;
using Invoice.Classes;
```

### クラスメンバーの順序
1. 定数
2. フィールド
3. プロパティ
4. コンストラクタ
5. メソッド（public → private）
6. イベントハンドラ

```csharp
public class ExampleClass
{
    // 1. 定数
    private const int MaxRetries = 3;
    
    // 2. フィールド
    private readonly IRepository _repository;
    private int _counter;
    
    // 3. プロパティ
    public int Id { get; set; }
    public string Name { get; set; }
    
    // 4. コンストラクタ
    public ExampleClass(IRepository repository)
    {
        _repository = repository;
    }
    
    // 5. パブリックメソッド
    public void PublicMethod() { }
    
    // 6. プライベートメソッド
    private void PrivateMethod() { }
    
    // 7. イベントハンドラ
    private void Button_Click(object sender, EventArgs e) { }
}
```

## 3. コーディングスタイル

### 中括弧
- K&Rスタイルではなく、**Allmanスタイル** を使用
```csharp
// 良い例（Allmanスタイル）
if (condition)
{
    DoSomething();
}

// 悪い例（K&Rスタイル）
if (condition) {
    DoSomething();
}
```

### インデント
- **スペース4つ** を使用（タブではない）

### 1行あたりの長さ
- 可能な限り **120文字以内** に収める

### 空白行
- メソッド間に1行
- 論理的なコードブロック間に1行

## 4. ベストプラクティス

### 4.1 マジックナンバーを避ける
```csharp
// 悪い例
if (transactionTypeId == 1)
{
    // ...
}

// 良い例
if (transactionTypeId == Constants.TransactionTypes.Balance)
{
    // ...
}
```

### 4.2 文字列比較
```csharp
// 悪い例
if (str == "")  // 空文字列との比較
if (str != null && str != "")  // 冗長

// 良い例
if (string.IsNullOrEmpty(str))
if (string.IsNullOrWhiteSpace(str))  // 空白文字も考慮
```

### 4.3 リソースの管理
```csharp
// 良い例 - usingステートメント
using var connection = new MySqlConnection(connectionString);
connection.Open();
// 自動的にDisposeされる

// または従来の形式
using (var connection = new MySqlConnection(connectionString))
{
    connection.Open();
    // ...
}  // ここでDisposeされる
```

### 4.4 null チェック
```csharp
// C# 8.0以降のnull許容参照型を活用
public string GetCustomerName(CustomerClass? customer)
{
    // 良い例 - null条件演算子
    return customer?.CustomerName ?? "Unknown";
    
    // または
    if (customer == null)
    {
        return "Unknown";
    }
    return customer.CustomerName;
}
```

### 4.5 LINQ の使用
```csharp
// 悪い例
var result = new List<int>();
foreach (var item in items)
{
    if (item > 10)
    {
        result.Add(item * 2);
    }
}

// 良い例
var result = items
    .Where(item => item > 10)
    .Select(item => item * 2)
    .ToList();
```

### 4.6 例外処理
```csharp
// 悪い例
try
{
    // ...
}
catch (Exception)
{
    // 例外を無視
}

// 良い例
try
{
    // ...
}
catch (MySqlException ex)
{
    // 特定の例外を適切に処理
    Logger.LogError(ex, "データベースエラーが発生しました");
    throw;  // 必要に応じて再スロー
}
```

### 4.7 非同期メソッド
```csharp
// 非同期メソッドには "Async" サフィックスを付ける
public async Task<List<CustomerClass>> GetCustomersAsync()
{
    // await を使用
    await using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();
    // ...
}
```

## 5. コメント規約

### XMLドキュメントコメント
- すべてのpublicメンバーに記述
```csharp
/// <summary>
/// 顧客情報をデータベースから取得します。
/// </summary>
/// <param name="customerId">顧客ID</param>
/// <returns>顧客情報。見つからない場合はnull</returns>
/// <exception cref="DatabaseException">データベースエラーが発生した場合</exception>
public CustomerClass GetCustomer(int customerId)
{
    // ...
}
```

### インラインコメント
- 複雑なロジックの説明に使用
- 「何を」ではなく「なぜ」を説明
```csharp
// 悪い例 - 「何を」を説明
// カウンターをインクリメント
counter++;

// 良い例 - 「なぜ」を説明
// 再試行回数を追跡するため、カウンターをインクリメント
counter++;

// または複雑なビジネスロジックの説明
// 請求日までの預かり金を計算し、請求総額を超えない範囲で適用
PaidByDeposit = DepositUntilIssueDate < itemsTotal 
    ? DepositUntilIssueDate 
    : itemsTotal;
```

### TODO コメント
```csharp
// TODO: エラーハンドリングを改善
// HACK: 一時的な回避策 - 次のスプリントで修正予定
// FIXME: バグあり - 特定の条件下でnull参照例外が発生
```

## 6. データベースアクセス

### パラメータ化されたクエリを使用
```csharp
// 良い例 - SQLインジェクション対策
var command = new MySqlCommand(
    "SELECT * FROM T_CUSTOMER WHERE CUSTOMER_ID = @id", 
    connection);
command.Parameters.AddWithValue("@id", customerId);

// 悪い例 - SQLインジェクションのリスク
var command = new MySqlCommand(
    $"SELECT * FROM T_CUSTOMER WHERE CUSTOMER_ID = {customerId}", 
    connection);
```

### Unit of Workパターンの使用
```csharp
// 良い例
UnitOfWork.ExecuteWithTransaction(uow =>
{
    var command = uow.CreateCommand(query);
    command.Parameters.AddWithValue("@param", value);
    command.ExecuteNonQuery();
    return true;
});
```

## 7. 避けるべきアンチパターン

### 副作用を持つプロパティ
```csharp
// 悪い例
public int Total
{
    get
    {
        CalculateSomething();  // 副作用
        UpdateDatabase();      // 副作用
        return _total;
    }
}

// 良い例
public int Total => _total;

public void Calculate()
{
    CalculateSomething();
    UpdateDatabase();
}
```

### ビジネスロジックでのUI依存
```csharp
// 悪い例
public void SaveCustomer()
{
    // ビジネスロジック
    MessageBox.Show("保存しました");  // UI依存
}

// 良い例
public bool SaveCustomer()
{
    // ビジネスロジックのみ
    return true;
}

// UI層で
if (SaveCustomer())
{
    MessageBox.Show("保存しました");
}
```

## 8. チェックリスト

コードをコミットする前に確認：

- [ ] すべてのpublicメンバーにXMLドキュメントコメントがある
- [ ] マジックナンバーが定数化されている
- [ ] 命名規則に従っている
- [ ] 適切なエラーハンドリングが実装されている
- [ ] リソースが適切に解放される（using使用）
- [ ] SQLインジェクション対策がされている
- [ ] 不要なコメントが削除されている
- [ ] コードが整形されている（インデント、空白行など）
- [ ] ビジネスロジックがUI層と分離されている
- [ ] 可能な限り非同期処理を使用している

## 参考資料

- [C# コーディング規則](https://learn.microsoft.com/ja-jp/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET API 設計ガイドライン](https://learn.microsoft.com/ja-jp/dotnet/standard/design-guidelines/)
- [C# におけるパターンとプラクティス](https://learn.microsoft.com/ja-jp/dotnet/architecture/modern-web-apps-azure/)
