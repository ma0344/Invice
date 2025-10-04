# Invoice アプリケーション

## 概要
Invoiceは、請求書管理および会計処理を行うWPFベースのデスクトップアプリケーションです。

## 🎯 主な機能
- 顧客管理
- 請求書発行
- 入金管理
- 会計データのCSV出力
- PDF帳票出力

## 🔧 技術スタック
- **フレームワーク**: .NET 8.0 (Windows)
- **UI**: WPF (Windows Presentation Foundation)
- **データベース**: MySQL
- **パターン**: MVVM, Unit of Work
- **ライブラリ**:
  - MySqlConnector
  - PDFsharp-MigraDoc
  - CsvHelper
  - ModernWpfUI

## 📚 ドキュメント

### コードレビュー関連
プロジェクト全体のコードレビューが実施され、包括的なドキュメントが作成されました：

- **[📋 REVIEW_SUMMARY.md](REVIEW_SUMMARY.md)** - レビュー完了サマリー（スタート地点として推奨）
- **[📊 CODE_REVIEW.md](CODE_REVIEW.md)** - 総合評価レポート（詳細な分析）
- **[🔧 IMPROVEMENT_EXAMPLES.md](IMPROVEMENT_EXAMPLES.md)** - 改善実施例（コード付き）
- **[📖 CODING_STANDARDS.md](CODING_STANDARDS.md)** - コーディング規約
- **[📁 REVIEW_DOCUMENTS_README.md](REVIEW_DOCUMENTS_README.md)** - ドキュメント概要

### ドキュメントの読み方

#### 初めての方
1. [REVIEW_SUMMARY.md](REVIEW_SUMMARY.md) - プロジェクトの全体像を把握
2. [CODING_STANDARDS.md](CODING_STANDARDS.md) - 開発時の参照用

#### コードレビュアー
1. [CODE_REVIEW.md](CODE_REVIEW.md) - 詳細な問題点の確認
2. [CODING_STANDARDS.md](CODING_STANDARDS.md) - レビュー基準

#### リファクタリング担当
1. [IMPROVEMENT_EXAMPLES.md](IMPROVEMENT_EXAMPLES.md) - 具体的な実装例
2. [CODE_REVIEW.md](CODE_REVIEW.md) - 優先度の確認

## 🚀 セットアップ

### 必要要件
- Windows 10/11
- .NET 8.0 SDK
- MySQL Server 5.7+
- Visual Studio 2022 (推奨)

### インストール手順
1. リポジトリをクローン
   ```bash
   git clone https://github.com/ma0344/Invice.git
   cd Invice
   ```

2. 依存関係の復元
   ```bash
   dotnet restore
   ```

3. データベース設定
   - `Connection.ini` ファイルを編集
   - MySQL接続情報を設定

4. ビルドと実行
   ```bash
   dotnet build
   dotnet run
   ```

## 📈 プロジェクト統計

- **総コード行数**: 約11,000行
- **C# ファイル数**: 39ファイル
- **コード品質スコア**: ⭐⭐⭐⭐☆ (4.0/5.0)

## 🔄 最近の改善

### 実装済み（最新）
- ✅ 変数名のスペルミス修正
- ✅ 冗長なコードの削除
- ✅ 命名規則の統一
- ✅ マジックナンバーの定数化
- ✅ 包括的なコードレビュードキュメントの作成

### 次のステップ
- 🔲 副作用を持つプロパティの修正
- 🔲 UI層とビジネスロジック層の分離
- 🔲 非同期プログラミングの導入

詳細は [REVIEW_SUMMARY.md](REVIEW_SUMMARY.md) を参照してください。

## 🤝 開発ガイドライン

### コーディング規約
[CODING_STANDARDS.md](CODING_STANDARDS.md) に定義されているコーディング規約に従ってください。

### コミット前のチェックリスト
- [ ] コードが [CODING_STANDARDS.md](CODING_STANDARDS.md) の規約に準拠している
- [ ] マジックナンバーが定数化されている（`Constants.cs`を使用）
- [ ] メソッド名がPascalCaseである
- [ ] XMLドキュメントコメントが追加されている（publicメンバー）
- [ ] リソースが適切に解放される（`using`を使用）

## 📞 サポート

問題や提案がある場合は、GitHubのIssuesセクションで報告してください。

## 📄 ライセンス

[LICENSE](LICENSE) ファイルを参照してください。

---

**最終更新**: 2025年
**メンテナー**: ma0344
