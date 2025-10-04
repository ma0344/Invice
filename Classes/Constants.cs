namespace Invoice.Classes
{
    /// <summary>
    /// アプリケーション全体で使用される定数を定義
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// トランザクション種別の定数
        /// </summary>
        public static class TransactionTypes
        {
            /// <summary>
            /// 残高 - Balance
            /// </summary>
            public const int Balance = 1;

            /// <summary>
            /// 預かり金 - Deposit
            /// </summary>
            public const int Deposit = 2;
        }

        /// <summary>
        /// 勘定科目コード
        /// </summary>
        public static class AccountCodes
        {
            /// <summary>
            /// 利用料 (家賃、食費、水道光熱費)
            /// </summary>
            public const long ServiceFees = 521;

            /// <summary>
            /// 日用品費等
            /// </summary>
            public const long DailyExpenses = 525;
        }

        /// <summary>
        /// 補助科目コード
        /// </summary>
        public static class ItemSubCodes
        {
            /// <summary>
            /// 家賃
            /// </summary>
            public const long Rent = 1;

            /// <summary>
            /// 食費
            /// </summary>
            public const long FoodExpense = 2;

            /// <summary>
            /// 水道光熱費
            /// </summary>
            public const long Utilities = 3;
        }
    }
}
