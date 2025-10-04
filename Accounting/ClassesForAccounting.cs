using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using Invoice.Classes;
using System.Diagnostics;

namespace Invoice.Accounting
{
    public enum MakeType
    {
        file,
        prev
    }

    public static class FileNameHelper
    {
        public static string GenerateInvoiceFilename(string directory, InvoiceClass invoice)
        {
            string baseName = $"請求書_{invoice.IssueDate?.ToString("yyyyMM")}_{invoice.Subject}_{invoice.CustomerName}.pdf";
            string uniqueName = GenerateUniqueFileName(directory, baseName);
            return uniqueName;
        }
        public static string GenerateReceiptFileName(string directory, PaymentClass payment)
        {
            string basename = $"領収書_{payment.PaymentDate.ToString("yyyyMM")}_{payment.Subject}_{payment.CustomerName}.pdf";
            string uniqueName = GenerateUniqueFileName(directory, basename);
            return uniqueName;
        }

        public static string GenerateUniqueFileName(string directory, string baseName)
        {
            string fileName = baseName;
            string noExtName = Path.GetFileNameWithoutExtension(baseName);
            var files = Directory.GetFiles(directory, $"{noExtName}*", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
            {
                return $"{directory}\\{fileName}";
            }

            var countNumbers = new List<int>();
            var pattern = @$"(?:{noExtName}\s\()([\d]*)(?:\))";
            foreach (var file in files)
            {
                var match = Regex.Match(file, pattern);
                if (match.Success)
                {
                    countNumbers.Add(int.Parse(match.Groups[1].Value));
                }
            }
            var maxNumber = countNumbers.Count == 0 ? 0 : countNumbers.Max();
            fileName = $"{directory}\\{noExtName} ({maxNumber + 1}).pdf";

            return fileName;
        }
    }

    public class RiyouryouClass
    {
        public long InvoiceID { get; set; } //請求書ID
        public long RecipientID { get; set; } //利用者ID
        public string RecipientName { get; set; } = "";  //氏名
        public long Rent { get; set; } //家賃
        public long FoodExp { get; set; } //食費
        public long DailyExp { get; set; } //日用品費等
        public long Utilities { get; set; } //水道光熱費
        public long TotalAmmount { get; set; } //合計

        public RiyouryouClass
            (
             long invoiceID,
             long recipientID,
             string recipientName,
             long rent = 0,
             long foodExp = 0,
             long dailyExp = 0,
             long utilities = 0
            )
        {
            InvoiceID = invoiceID;
            RecipientID = recipientID;
            RecipientName = recipientName;
            Rent = rent;
            FoodExp = foodExp;
            DailyExp = dailyExp;
            Utilities = utilities;
            TotalAmmount = rent + foodExp + dailyExp + utilities;
        }

        public List<List<long>> ToList()
        {
            return
            [
                [Constants.AccountCodes.ServiceFees, Constants.ItemSubCodes.Rent, Rent],
                [Constants.AccountCodes.ServiceFees, Constants.ItemSubCodes.FoodExpense, FoodExp],
                [Constants.AccountCodes.ServiceFees, Constants.ItemSubCodes.Utilities, Utilities],
                [Constants.AccountCodes.DailyExpenses, Constants.ItemSubCodes.Rent, DailyExp]
            ];
        }
    }

    public class AccountingDataClassMap : ClassMap<AccountingDataClass>
    {
        public AccountingDataClassMap()
        {
            Map(m => m.VoucherNumber).Index(0).Name("伝票番号"); // 伝票番号
            Map(m => m.LineNumber).Index(1).Name("行番号"); // 行番号
            Map(m => m.VoucherDate).Index(2).Name("伝票日付"); // 伝票日付
            Map(m => m.DebitAccountCode).Index(3).Name("借方科目コード"); // 借方科目コード
            Map(m => m.DebitAccountName).Index(4).Name("借方科目名称"); // 借方科目名称
            Map(m => m.DebitSubCode).Index(5).Name("借方補助コード"); // 借方補助コード
            Map(m => m.DebitSubName).Index(6).Name("借方補助科目名称"); // 借方補助科目名称
            Map(m => m.DebitDepartmentCode).Index(7).Name("借方部門コード"); // 借方部門コード
            Map(m => m.DebitDepartmentName).Index(8).Name("借方部門名称"); // 借方部門名称
            Map(m => m.DebitTaxDivisionCode).Index(9).Name("借方課税区分コード"); // 借方課税区分コード
            Map(m => m.DebitBusinessCategoryCode).Index(10).Name("借方事業分類コード"); // 借方事業分類コード
            Map(m => m.DebitTaxHandlingCode).Index(11).Name("借方税処理コード"); // 借方税処理コード
            Map(m => m.DebitTaxRate).Index(12).Name("借方税率"); // 借方税率
            Map(m => m.DebitAmount).Index(13).Name("借方金額"); // 借方金額
            Map(m => m.DebitTax).Index(14).Name("借方消費税"); // 借方消費税
            Map(m => m.CreditAccountCode).Index(15).Name("貸方科目コード"); // 貸方科目コード
            Map(m => m.CreditAccountName).Index(16).Name("貸方科目名称"); // 貸方科目名称
            Map(m => m.CreditSubCode).Index(17).Name("貸方補助コード"); // 貸方補助コード
            Map(m => m.CreditSubName).Index(18).Name("貸方補助科目名称"); // 貸方補助科目名称
            Map(m => m.CreditDepartmentCode).Index(19).Name("貸方部門コード"); // 貸方部門コード
            Map(m => m.CreditDepartmentName).Index(20).Name("貸方部門名称"); // 貸方部門名称
            Map(m => m.CreditTaxDivisionCode).Index(21).Name("貸方課税区分コード"); // 貸方課税区分コード
            Map(m => m.CreditBusinessCategoryCode).Index(22).Name("貸方事業分類コード"); // 貸方事業分類コード
            Map(m => m.CreditTaxHandlingCode).Index(23).Name("貸方税処理コード"); // 貸方税処理コード
            Map(m => m.CreditTaxRate).Index(24).Name("貸方税率"); // 貸方税率
            Map(m => m.CreditAmount).Index(25).Name("貸方金額"); // 貸方金額
            Map(m => m.CreditTax).Index(26).Name("貸方消費税"); // 貸方消費税
            Map(m => m.TransactionDescription).Index(27).Name("取引摘要"); // 取引摘要
            Map(m => m.AuxiliaryMemo).Index(28).Name("補助摘要"); // 補助摘要
            Map(m => m.Memo).Index(29).Name("メモ"); // メモ
            Map(m => m.Tag1).Index(30).Name("付箋１"); // 付箋１
            Map(m => m.Tag2).Index(31).Name("付箋２"); // 付箋２
            Map(m => m.VoucherType).Index(32).Name("伝票種別"); // 伝票種別
        }
    }

    public class AccountingDataClass : AccountingDataBaseClass, ILoggable
    {
        public List<RiyouryouClass>? riyouryouClassList;
        public RequiredData? reqData;
        public string outputFileName = "";
        public long rowCounter = 0;

        public AccountingDataClass()
        {
        }
        public string CreateCsv(MakeType type, DateTime targetDate, List<InvoiceClass> selectedInvoices, string dirName = "")
        {


            reqData = new RequiredData(targetDate, DateTime.DaysInMonth(targetDate.Year, targetDate.Month));
            string returnString = string.Empty;
            switch (type)
            {
                case MakeType.file:
                    outputFileName = $"{dirName}\\";
                    outputFileName += reqData.ProcessDate.ToString("利用料請求仕訳 yyyy年 MM月 請求分") + ".csv";
                    if (CreateCsvFile(reqData, selectedInvoices) == string.Empty)
                    {
                        MessageBox.Show("指定された日付のデータがありませんでした");
                    }
                    else
                    {
                        returnString = outputFileName;
                    }
                    break;
                case MakeType.prev:
                    string previewString = PreviewCsv(reqData, selectedInvoices);
                    if (previewString == "false")
                    {
                        MessageBox.Show("指定された日付のデータがありませんでした");
                    }
                    else
                    {
                        returnString = previewString;
                    }
                    break;
            }
            return returnString;
        }

        public string CreateCsvFile(RequiredData reqData, List<InvoiceClass> invoices)
        {

            var convertedData = ConvertData(invoices);
            WriteCsv(convertedData, outputFileName);
            return outputFileName;
        }
        public string PreviewCsv(RequiredData reqData, List<InvoiceClass> invoices)
        {

            var convertedData = ConvertData(invoices);
            return GetPreviewString(convertedData);
        }




        public List<AccountingDataBaseClass> ConvertData(List<InvoiceClass> invoices)
        {

            rowCounter = 0;
            AccountingDataBaseClass convertedLineData;
            var convertedData = new List<AccountingDataBaseClass>();
            foreach (InvoiceClass orgInvoice in invoices)
            {
                var invoice = orgInvoice.DeepClone();
                if (reqData is not null)
                {
                    rowCounter++;
                    convertedLineData = new AccountingDataBaseClass
                    (
                        SlipNumber: reqData.VoucherNumber,
                        LineNum: rowCounter,
                        IssueDateString: reqData.VoucherDateString,
                        DebitAccCode: invoice.TransactionTypeId == 1 ? 134 : 310,
                        DebitSubCode: invoice.CustomerId,
                        DebitDeptCode: 211,
                        DebitAmount: invoice.ItemsTotal,
                        Description: $"利用料 {invoice.CustomerName}",
                        AuxMemo: invoice.IssueDate!.Value.ToString("ggy年M月")
                    );
                    convertedData.Add(convertedLineData);
                    var accountingDic = convertedLineData.AccountingDic;
                    foreach (var item in invoice.InvoiceItems)
                    {
                        if (item.ItemCode == "99" || item.ItemName == "特定障害者特別給付として国保連請求済み") continue;
                        rowCounter++;
                        var creditCode = GetAccountCode(item.ItemName);
                        var creditSubCode = GetItemSubCode(item.ItemName);

                        if (item.ItemName.Contains("家賃") && invoice.InvoiceItems.Any(i => (i.ItemCode == "99" || i.ItemName == "特定障害者特別給付として国保連請求済み")))
                        {
                            item.UnitPrice = item.UnitPrice * item.Quantity + invoice.InvoiceItems.First(i => (i.ItemCode == "99" || i.ItemName == "特定障害者特別給付として国保連請求済み")).ItemTotal;
                            item.Quantity = 1;
                        }
                        convertedLineData = new AccountingDataBaseClass
                        (
                            SlipNumber: reqData.VoucherNumber,
                            LineNum: rowCounter,
                            IssueDateString: reqData.VoucherDateString,
                            CreditAccCode: creditCode,
                            CreditSubCode: creditSubCode,
                            CreditDeptCode: 211,
                            CreditAmount: item.ItemTotal,
                            Description: $"{item.ItemName}" + (item.Quantity > 1 ? $" × {item.Quantity}"
                            : ""),
                            AuxMemo: invoice.IssueDate!.Value.ToString("ggy年M月")
                        );
                        convertedData.Add(convertedLineData);

                    }
                }
            }
            return convertedData;
        }

        private long GetAccountCode(string itemName)
        {

            return itemName switch
            {
                string name when name.Contains("家賃") => Constants.AccountCodes.ServiceFees,
                string name when name.Contains('食') => Constants.AccountCodes.ServiceFees,
                string name when name.Contains("日用品") => Constants.AccountCodes.DailyExpenses,
                string name when name.Contains("水道") => Constants.AccountCodes.ServiceFees,
                _ => 0 // デフォルト値
            };
        }

        private long GetItemSubCode(string itemName)
        {

            return itemName switch
            {
                string name when name.Contains("家賃") => Constants.ItemSubCodes.Rent,
                string name when name.Contains('食') => Constants.ItemSubCodes.FoodExpense,
                string name when name.Contains("日用品") => Constants.ItemSubCodes.Rent,
                string name when name.Contains("水道") => Constants.ItemSubCodes.Utilities,
                _ => 0 // デフォルト値
            };

        }


        public static string GetPreviewString(List<AccountingDataBaseClass> convertedData)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ShouldQuote = (args) => args.FieldType == typeof(string),
            };

            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, config);
            csv.Context.RegisterClassMap<AccountingDataClassMap>();
            csv.WriteHeader<AccountingDataClass>();
            csv.NextRecord();
            foreach (var data in convertedData)
            {
                csv.WriteRecord(data);
                csv.NextRecord();
            }
            return writer.ToString();

        }
        public void WriteCsv(List<AccountingDataBaseClass> convertedData, string outputPath)
        {


            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                ShouldQuote = (args) => args.FieldType == typeof(string),
            };

            try
            {
                string message = "";
                using (var writer = new StreamWriter(outputPath, false, Encoding.GetEncoding("shift_jis")))
                using (var csv = new CsvWriter(writer, config))
                {
                    csv.Context.RegisterClassMap<AccountingDataClassMap>();
                    csv.WriteHeader<AccountingDataClass>();
                    csv.NextRecord();
                    foreach (var data in convertedData)
                    {
                        csv.WriteRecord(data);
                        csv.NextRecord();
                    }
                }
                message = "利用料請求仕訳のCSVファイルを出力しました。\r\n";
                message += "\r\n\t\t仕訳行数 ： " + rowCounter + " 行";

                Clipboard.SetText(outputFileName);
                message += "\r\n\r\nファイルパスをクリップボードにコピーしました。\r\n";
                MessageBox.Show(message, "処理終了");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{GetType().Name}.{MethodBase.GetCurrentMethod()!.Name} : {ex.Message}");
            }
        }

    }

    public class AccountingDataBaseClass
    {
        public Dictionary<string, string> AccountingDic { get; set; }
        public Dictionary<string, string> DepartmentDic { get; set; }
        public long VoucherNumber { get; set; } // 伝票番号
        public long LineNumber { get; set; } // 行番号
        public string VoucherDate { get; set; } = "";  // 伝票日付
        public long DebitAccountCode { get; set; } // 借方科目コード
        public string DebitAccountName { get; set; } = "";  // 借方科目名称
        public long DebitSubCode { get; set; } // 借方補助コード
        public string DebitSubName { get; set; } = "";  // 借方補助科目名称
        public long DebitDepartmentCode { get; set; } // 借方部門コード
        public string DebitDepartmentName { get; set; } = "";  // 借方部門名称
        public long DebitTaxDivisionCode { get; set; } // 借方課税区分コード
        public long DebitBusinessCategoryCode { get; set; } // 借方事業分類コード
        public long DebitTaxHandlingCode { get; set; } // 借方税処理コード
        public long DebitTaxRate { get; set; } // 借方税率
        public long DebitAmount { get; set; } // 借方金額
        public long DebitTax { get; set; } // 借方消費税
        public long CreditAccountCode { get; set; } // 貸方科目コード
        public string CreditAccountName { get; set; } = "";  // 貸方科目名称
        public long CreditSubCode { get; set; } // 貸方補助コード
        public string CreditSubName { get; set; } = "";  // 貸方補助科目名称
        public long CreditDepartmentCode { get; set; } // 貸方部門コード
        public string CreditDepartmentName { get; set; } = "";  // 貸方部門名称
        public long CreditTaxDivisionCode { get; set; } // 貸方課税区分コード
        public long CreditBusinessCategoryCode { get; set; } // 貸方事業分類コード
        public long CreditTaxHandlingCode { get; set; } // 貸方税処理コード
        public long CreditTaxRate { get; set; } // 貸方税率
        public long CreditAmount { get; set; } // 貸方金額
        public long CreditTax { get; set; } // 貸方消費税
        public string TransactionDescription { get; set; } = "";  // 取引摘要
        public string AuxiliaryMemo { get; set; } = "";  // 補助摘要
        public string Memo { get; set; } = "";  // メモ
        public long Tag1 { get; set; } = 0; // 付箋１
        public long Tag2 { get; set; } = 0; // 付箋２
        public long VoucherType { get; set; } = 0; // 伝票種別



        public AccountingDataBaseClass(
            long SlipNumber = 0,// 伝票番号
            long LineNum = 0,// 行番号
            string IssueDateString = "",// 伝票日付
            long DebitAccCode = 0,// 借方科目コード
            long DebitSubCode = 0,// 借方補助コード
            long DebitDeptCode = 0,// 借方部門コード
            long DebitAmount = 0,// 借方金額
            long CreditAccCode = 0,// 貸方科目コード
            long CreditSubCode = 0,// 貸方補助コード
            long CreditDeptCode = 0,// 貸方部門コード
            long CreditAmount = 0,// 貸方金額
            string Description = "",// 取引摘要
            string AuxMemo = "")// 補助摘要
        {
            AccountingDic = GetDictionaly(@"AccountingTitleDictionary.csv");
            DepartmentDic = GetDictionaly(@"DepartmentDictionary.csv");

            VoucherNumber = SlipNumber;
            LineNumber = LineNum;
            VoucherDate = IssueDateString;
            DebitAccountCode = DebitAccCode;
            DebitAccountName = DebitAccCode == 0 ? "" : AccountingDic[DebitAccCode.ToString()];
            this.DebitSubCode = DebitSubCode;
            DebitSubName = DebitSubCode == 0 ? "" : AccountingDic[DebitAccCode.ToString() + "." + DebitSubCode.ToString()];
            DebitDepartmentCode = DebitDeptCode;
            DebitDepartmentName = DebitDeptCode == 0 ? "" : DepartmentDic[DebitDeptCode.ToString()];
            DebitTaxDivisionCode = 0;
            DebitBusinessCategoryCode = 0;
            DebitTaxHandlingCode = 3;
            DebitTaxRate = 10;
            this.DebitAmount = DebitAmount;
            DebitTax = 0;
            CreditAccountCode = CreditAccCode;
            CreditAccountName = CreditAccCode == 0 ? "" : AccountingDic[CreditAccCode.ToString()];
            this.CreditSubCode = CreditSubCode;
            CreditSubName = CreditSubCode == 0 ? "" : AccountingDic[CreditAccCode.ToString() + "." + CreditSubCode.ToString()];
            CreditDepartmentCode = CreditDeptCode;
            CreditDepartmentName = CreditDeptCode == 0 ? "" : DepartmentDic[CreditDeptCode.ToString()];
            CreditTaxDivisionCode = 0;
            CreditBusinessCategoryCode = 0;
            CreditTaxHandlingCode = 3;
            CreditTaxRate = 10;
            this.CreditAmount = CreditAmount;
            CreditTax = 0;
            TransactionDescription = Description;
            AuxiliaryMemo = AuxMemo;
        }
        private static Dictionary<string, string> GetDictionaly(string fileName)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
            using var reader = new StreamReader(fileName);
            using var csv = new CsvReader(reader, csvConfig);
            var records = csv.GetRecords<KeyValueRecord>();
            return records.ToDictionary(record => record.Key, record => record.Value);
        }
    }

    public class RequiredData
    {
        public DateTime StartDate { get; set; }
        public string StartDateString { get; set; }
        public DateTime EndDate { get; set; }
        public string EndDateString { get; set; }
        public DateTime ProcessDate { get; set; }
        public string VoucherDateString { get; set; }
        public long VoucherNumber { get; set; }

        public RequiredData(DateTime targetDate, int processDay = 1)
        {
            string targetDateString = targetDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            StartDate = new DateTime(targetDate.Year, targetDate.Month, 1);
            StartDateString = StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            EndDate = new DateTime(StartDate.Year, StartDate.Month, DateTime.DaysInMonth(StartDate.Year, StartDate.Month));
            EndDateString = EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ProcessDate = new DateTime(EndDate.Year, EndDate.Month, processDay);
            VoucherDateString = GetVoucherDateString(ProcessDate);
            VoucherNumber = GetVoucherNumberString(ProcessDate);
        }
        public static string GetVoucherDateString(DateTime date)
        {
            DateTime outDate = date;
            var culture = new CultureInfo("ja-JP");
            culture.DateTimeFormat.Calendar = new JapaneseCalendar();
            return GetEraAlphabet(outDate) + outDate.ToString(".yy/MM/dd", culture);
        }
        public static long GetVoucherNumberString(DateTime date)
        {
            DateTime outDate = date;
            var culture = new CultureInfo("ja-JP");
            culture.DateTimeFormat.Calendar = new JapaneseCalendar();
            return long.Parse(outDate.ToString("yMMdd", culture));
        }
        public static string GetEraAlphabet(DateTime date)
        {
            var culture = new CultureInfo("ja-JP");
            culture.DateTimeFormat.Calendar = new JapaneseCalendar();

            // 元号の英字表記へのマッピング
            var eraMappings = new Dictionary<string, string>
            {
                {"令和", "R"},
                {"平成", "H"},
                {"昭和", "S"},
                {"大正", "T"},
                {"明治", "M"}
            };

            // 日付から元号のフルネームを取得
            string eraFullName = culture.DateTimeFormat.GetEraName(culture.DateTimeFormat.Calendar.GetEra(date));

            // マッピングから対応する英字表記を取得

            if (eraMappings.TryGetValue(eraFullName, out string? eraAlphabet))
            {
                return eraAlphabet;
            }

            return "?"; // 未知の元号の場合
        }


    }

    public class KeyValueRecord
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}
