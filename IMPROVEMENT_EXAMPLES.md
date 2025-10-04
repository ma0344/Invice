# 改善実施例とベストプラクティス

## 概要
このドキュメントでは、CODE_REVIEW.mdで指摘した問題点の具体的な改善例を示します。

## 既に実装済みの改善

### 1. 変数名のスペルミス修正
**修正前:**
```csharp
private readonly List<string> _executedPalams = new();
```

**修正後:**
```csharp
private readonly List<string> _executedParams = new();
```

### 2. 冗長なコード削除
**修正前:**
```csharp
command.CommandText = commandText != "" ? commandText : string.Empty;
```

**修正後:**
```csharp
command.CommandText = commandText;
```

**理由:** 空文字列 (`""`) と `string.Empty` は同じなので、三項演算子は不要です。

### 3. 不要な.ToString()呼び出しの削除
**修正前:**
```csharp
MessageBox.Show($"エラーが発生しました: {e.Message}\r\nスタックトレース\r\n{e.StackTrace.ToString()}");
```

**修正後:**
```csharp
MessageBox.Show($"エラーが発生しました: {e.Message}\r\nスタックトレース\r\n{e.StackTrace}");
```

**理由:** `StackTrace` プロパティは既に文字列型なので、`.ToString()` は冗長です。

### 4. メソッド名の命名規則修正
**修正前:**
```csharp
public string getTaxTypeName(int taxTypeId)
```

**修正後:**
```csharp
public string GetTaxTypeName(int taxTypeId)
```

**理由:** C#の命名規則では、publicメソッドはPascalCaseで命名します。

### 5. マジックナンバーの定数化
新しいファイル `Classes/Constants.cs` を作成し、アプリケーション全体で使用する定数を定義しました。

**修正前:**
```csharp
if (TransactionTypeId == 1)
    BalanceClass.TryAddBalance(this, uow);
else if (TransactionTypeId == 2)
    DepositClass.TryAddDeposit(this, uow);
```

**修正後:**
```csharp
if (TransactionTypeId == Constants.TransactionTypes.Balance)
    BalanceClass.TryAddBalance(this, uow);
else if (TransactionTypeId == Constants.TransactionTypes.Deposit)
    DepositClass.TryAddDeposit(this, uow);
```

**利点:**
- コードの可読性向上
- 定数の一元管理
- タイプミスの防止
- リファクタリングの容易化

## 今後実装を推奨する改善例

### 1. 副作用を持つプロパティの修正

**現在の問題コード (InvoiceClass.cs):**
```csharp
public int ItemsTotal
{
    get
    {
        var itemsTotal = InvoiceItems.Sum(x => x.Quantity * x.UnitPrice + x.Tax);
        PaidByDeposit = DepositUntilIssueDate < itemsTotal ? DepositUntilIssueDate : itemsTotal; // 副作用
        return itemsTotal;
    }
}
```

**推奨される修正:**
```csharp
// プロパティは純粋な計算のみ
public int ItemsTotal => InvoiceItems.Sum(x => x.Quantity * x.UnitPrice + x.Tax);

// 副作用のある処理はメソッドに移動
public void UpdatePaidByDeposit()
{
    var itemsTotal = ItemsTotal;
    PaidByDeposit = DepositUntilIssueDate < itemsTotal ? DepositUntilIssueDate : itemsTotal;
}
```

### 2. UI層とビジネスロジック層の分離

**現在の問題コード:**
```csharp
// Classes/DatabaseHelper.cs (ビジネスロジック層)
catch (Exception e)
{
    MessageBox.Show($"エラーが発生しました: {e.Message}");
    return false;
}
```

**推奨される修正:**

#### オプション1: カスタム例外を使用
```csharp
// Classes/Exceptions/DatabaseException.cs
public class DatabaseException : Exception
{
    public DatabaseException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

// Classes/DatabaseHelper.cs
catch (Exception e)
{
    if (unitOfWork == null)
    {
        uow.Rollback();
        uow.Dispose();
    }
    throw new DatabaseException("データベース操作中にエラーが発生しました", e);
}

// Pages/PaymentPage.xaml.cs (UI層)
try
{
    // ビジネスロジック呼び出し
}
catch (DatabaseException ex)
{
    MessageBox.Show($"エラーが発生しました: {ex.Message}\n詳細: {ex.InnerException?.Message}");
}
```

#### オプション2: 結果オブジェクトパターン
```csharp
// Classes/OperationResult.cs
public class OperationResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public Exception Exception { get; set; }
    
    public static OperationResult Ok() => new OperationResult { Success = true };
    public static OperationResult Fail(string message, Exception ex = null) 
        => new OperationResult { Success = false, ErrorMessage = message, Exception = ex };
}

// Classes/DatabaseHelper.cs
public static OperationResult ExecuteWithTransaction(Func<UnitOfWork, bool> action, UnitOfWork? unitOfWork = null)
{
    var uow = unitOfWork ?? new UnitOfWork();
    try
    {
        bool result = action(uow);
        if (unitOfWork == null && result)
        {
            uow.Commit();
        }
        return result ? OperationResult.Ok() : OperationResult.Fail("処理が失敗しました");
    }
    catch (Exception e)
    {
        if (unitOfWork == null)
        {
            uow.Rollback();
        }
        return OperationResult.Fail("エラーが発生しました", e);
    }
    finally
    {
        if (unitOfWork == null)
        {
            uow.Dispose();
        }
    }
}

// Pages/PaymentPage.xaml.cs (UI層)
var result = UnitOfWork.ExecuteWithTransaction(uow => { /* ... */ });
if (!result.Success)
{
    MessageBox.Show($"エラー: {result.ErrorMessage}");
}
```

### 3. リポジトリパターンの導入

**現在の問題:**
各エンティティクラス内にデータアクセスロジックが含まれています。

**推奨される修正:**

```csharp
// Classes/Repositories/IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<int> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// Classes/Repositories/CustomerRepository.cs
public class CustomerRepository : IRepository<CustomerClass>
{
    public async Task<CustomerClass> GetByIdAsync(int id)
    {
        string connectionString = ConnectionInfo.Builder.ConnectionString;
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();
        
        var command = new MySqlCommand("SELECT * FROM T_CUSTOMER WHERE CUSTOMER_ID = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        
        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new CustomerClass
            {
                CustomerId = reader.GetInt32("CUSTOMER_ID"),
                CustomerName = reader.GetString("CUSTOMER_NAME"),
                CustomerKana = reader.GetString("CUSTOMER_KANA"),
                CustomerBalance = reader.GetInt32("BALANCE"),
                CustomerVisible = reader.GetBoolean("VISIBLE")
            };
        }
        return null;
    }
    
    // その他のメソッド実装...
}
```

### 4. 非同期プログラミングの導入

**現在の同期コード:**
```csharp
public static List<CustomerClass> GetCustomers()
{
    var customers = new List<CustomerClass>();
    string connectionString = ConnectionInfo.Builder.ConnectionString;
    using var connection = new MySqlConnection(connectionString);
    connection.Open();
    using var command = new MySqlCommand("SELECT * FROM T_CUSTOMER", connection);
    using var reader = command.ExecuteReader();
    // ...
}
```

**推奨される非同期コード:**
```csharp
public static async Task<List<CustomerClass>> GetCustomersAsync()
{
    var customers = new List<CustomerClass>();
    string connectionString = ConnectionInfo.Builder.ConnectionString;
    await using var connection = new MySqlConnection(connectionString);
    await connection.OpenAsync();
    await using var command = new MySqlCommand("SELECT * FROM T_CUSTOMER", connection);
    await using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        // ...
    }
    return customers;
}
```

**UI層での使用:**
```csharp
private async void LoadCustomersButton_Click(object sender, RoutedEventArgs e)
{
    try
    {
        var customers = await CustomerClass.GetCustomersAsync();
        CustomerListView.ItemsSource = customers;
    }
    catch (Exception ex)
    {
        MessageBox.Show($"顧客情報の読み込みに失敗しました: {ex.Message}");
    }
}
```

### 5. 依存性注入の導入

**推奨されるアプローチ:**

```csharp
// App.xaml.cs
public partial class App : Application
{
    private ServiceProvider _serviceProvider;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();
        
        // リポジトリの登録
        services.AddScoped<IRepository<CustomerClass>, CustomerRepository>();
        services.AddScoped<IRepository<InvoiceClass>, InvoiceRepository>();
        
        // ViewModelsの登録
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<CustomerViewModel>();
        services.AddTransient<InvoiceViewModel>();
        
        // MainWindowの登録
        services.AddTransient<MainWindow>();
        
        _serviceProvider = services.BuildServiceProvider();
        
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

// ViewModels/CustomerViewModel.cs
public class CustomerViewModel
{
    private readonly IRepository<CustomerClass> _customerRepository;
    
    // コンストラクタインジェクション
    public CustomerViewModel(IRepository<CustomerClass> customerRepository)
    {
        _customerRepository = customerRepository;
    }
    
    public async Task LoadCustomersAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        // ...
    }
}
```

### 6. ユニットテストの追加

**テストプロジェクトのセットアップ:**
```bash
dotnet new xunit -n Invoice.Tests
dotnet add Invoice.Tests reference Invoice.csproj
dotnet add Invoice.Tests package Moq
dotnet add Invoice.Tests package FluentAssertions
```

**テスト例:**
```csharp
// Invoice.Tests/Classes/PaymentClassTests.cs
public class PaymentClassTests
{
    [Fact]
    public void TryAddPayment_WithBalanceType_ShouldAddBalance()
    {
        // Arrange
        var payment = new PaymentClass
        {
            TransactionTypeId = Constants.TransactionTypes.Balance,
            PaymentAmount = 10000,
            CustomerId = 1
        };
        
        // Act
        var result = payment.TryAddPayment();
        
        // Assert
        result.Should().BeTrue();
        payment.PaymentId.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public void TryAddPayment_WithDepositType_ShouldAddDeposit()
    {
        // Arrange
        var payment = new PaymentClass
        {
            TransactionTypeId = Constants.TransactionTypes.Deposit,
            PaymentAmount = 5000,
            CustomerId = 1
        };
        
        // Act
        var result = payment.TryAddPayment();
        
        // Assert
        result.Should().BeTrue();
    }
}
```

## 段階的な改善ロードマップ

### フェーズ 1: 基本的なコード品質改善 (完了)
- ✅ 変数名の修正
- ✅ 冗長なコードの削除
- ✅ 命名規則の統一
- ✅ マジックナンバーの定数化

### フェーズ 2: アーキテクチャの改善 (推奨)
- [ ] 副作用を持つプロパティの修正
- [ ] UI層とビジネスロジック層の分離
- [ ] カスタム例外クラスの導入

### フェーズ 3: 近代的なパターンの導入
- [ ] リポジトリパターンの導入
- [ ] 非同期プログラミングへの移行
- [ ] 依存性注入の導入

### フェーズ 4: テストとドキュメント
- [ ] ユニットテストプロジェクトの作成
- [ ] 統合テストの追加
- [ ] XMLドキュメントコメントの完成

## まとめ

このドキュメントでは、コードレビューで指摘した問題点に対する具体的な改善例を示しました。
既に実装された基本的な改善により、コードの品質と可読性が向上しています。

今後の改善を段階的に進めることで、より保守性が高く、テスト可能で、拡張性のあるコードベースへと発展させることができます。
