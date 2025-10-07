namespace Invoice.Classes
{
    /// <summary>
    /// 借方/貸方（DEBIT_OR_CREDIT）を示すID。DB化も可能だが値の意味が不変のため定数で提供。
    /// 必要であれば Provider 化（T_DEBIT_OR_CREDIT から解決）に差し替え可能。
    /// </summary>
    public static class DebitOrCreditIds
    {
        public const int Debit = 1;  // 借方
        public const int Credit = 2; // 貸方
    }
}
