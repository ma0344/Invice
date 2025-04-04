using Invoice;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using System.Diagnostics;
using System.Globalization;
using System.IO;


namespace Invoice
{
    public class FontType
    {
        public string Black { get { return $"{familyName} Black"; } }
        public string Bold { get { return $"{familyName} Bold"; } }
        public string ExtraBold { get { return $"{familyName} ExtraBold"; } }
        public string ExtraLight { get { return $"{familyName} ExtraLight"; } }
        public string Light { get { return $"{familyName} Light"; } }
        public string Medium { get { return $"{familyName} Medium"; } }
        public string Regular { get { return $"{familyName} Regular"; } }
        public string SemiBold { get { return $"{familyName} SemiBold"; } }
        public string VariableFont_wght { get { return $"{familyName} VariableFont_wght"; } }
        public string Thin { get { return $"{familyName} Thin"; } }

        private string _familyName;
        public string familyName { get { return _familyName; } set { _familyName = value; } }


    }

    class ReceiptPdfGenerator
    {
        private CultureInfo cultureInfo = new("ja-JP");
        private CompanyInfo companyInfo = CompanyInfo.GetCompanyInfo();

        public ReceiptPdfGenerator()
        {

            GlobalFontSettings.FontResolver = FontResolver.Instance;

            //RegisterFonts();
        }

        private void RegisterFonts()
        {
            var fontResolver = oldFontResolver.Instance;
            var fontDir = "Invoice.Fonts";
            fontResolver.AddFont("Noto Serif JP", "Black", false, fontDir);
            fontResolver.AddFont("Noto Serif JP", "Bold", false, fontDir);
            fontResolver.AddFont("Noto Serif JP", "ExtraBold", false, fontDir);
            fontResolver.AddFont("Noto Serif JP", "ExtraLight", false, fontDir);
            fontResolver.AddFont("Noto Serif JP", "Light", false, fontDir);
            fontResolver.AddFont("Noto Serif JP", "Medium", false, fontDir);
            fontResolver.AddFont("Noto Serif JP", "Regular", false, fontDir);
            fontResolver.AddFont("Noto Serif JP", "SemiBold", false, fontDir);
            fontResolver.AddFont("Noto Sans JP", "Black", false, fontDir);
            fontResolver.AddFont("Noto Sans JP", "Bold", false, fontDir);
            fontResolver.AddFont("Noto Sans JP", "ExtraBold", false, fontDir);
            fontResolver.AddFont("Noto Sans JP", "ExtraLight", false, fontDir);
            fontResolver.AddFont("Noto Sans JP", "Light", false, fontDir);
            fontResolver.AddFont("Noto Sans JP", "Medium", false, fontDir);
            fontResolver.AddFont("Noto Sans JP", "Regular", false, fontDir);
            fontResolver.AddFont("Noto Sans JP", "SemiBold", false, fontDir);
            fontResolver.AddFont("Noto Sans JP", "Thin", false, fontDir);
            fontResolver.AddFont("Bodoni MT", "Condenced", false, false, "BOD_CR.TTF");
            fontResolver.AddFont("Bodoni MT Poster Compressed", "Poster Compressed", false, false, "BOD_POST.TTF");
            fontResolver.AddFont("Gloucester MT", "Extra Condensed", false, false, "GLECB.TTF");
            fontResolver.AddFont("Palace Script MT", "", false, false, "PALSCRI.TTF");
            GlobalFontSettings.FontResolver = fontResolver;
        }

        public void CreateReceiptPdf(PaymentClass paymentData, string filePath)
        {
            var sans = new FontType() { familyName = "Noto Sans JP" };
            var serif = new FontType() { familyName = "Noto Serif JP" };

            string defaultFont = "Noto Serif JP ExtraBold";
            string alphabetFont = "Bodoni MT Condenced";

            cultureInfo.DateTimeFormat.Calendar = new JapaneseCalendar();
            cultureInfo.DateTimeFormat.ShortDatePattern = "ggy年M月d日";
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            // ドキュメントの作成
            Document document = new Document();
            document.Info.Title = $"{Path.GetFileNameWithoutExtension(filePath)}";
            document.Info.Subject = $"{paymentData.Subject}";
            document.Info.Author = "Author";


            // スタイルの定義
            DefineStyles(document);

            // セクションの追加
            Section section = document.AddSection();

            // フォントの設定
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.TopMargin = Unit.FromCentimeter(0);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(0);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(0);
            section.PageSetup.RightMargin = Unit.FromCentimeter(0);
            section.PageSetup.HeaderDistance = Unit.FromCentimeter(0);
            section.PageSetup.FooterDistance = Unit.FromCentimeter(0);

            double gapHeight = 99;

            for (var a = 0; a <= 1; a++)
            {
                var gap = gapHeight * a;

                TextFrame lineFrame = section.AddTextFrame();
                lineFrame.Width = Unit.FromMillimeter(210);
                lineFrame.RelativeHorizontal = RelativeHorizontal.Page;
                lineFrame.Left = Unit.FromMillimeter(0);
                lineFrame.RelativeVertical = RelativeVertical.Page;
                lineFrame.Top = Unit.FromMillimeter(99 + gap);
                Paragraph lineParagraph = lineFrame.AddParagraph();
                lineParagraph.Format.Borders.Top.Width = 0.25;
                lineParagraph.Format.Borders.Top.Color = Colors.Black;
                lineParagraph.Format.LeftIndent = Unit.FromMillimeter(19);
                lineParagraph.Format.RightIndent = Unit.FromMillimeter(19);

                TextFrame topFrame = section.AddTextFrame();
                topFrame.Width = Unit.FromMillimeter(210);
                topFrame.RelativeHorizontal = RelativeHorizontal.Page;
                topFrame.Left = Unit.FromMillimeter(1.2);
                topFrame.RelativeVertical = RelativeVertical.Page;
                topFrame.Top = Unit.FromMillimeter(10 + gap);

                Table topTable = topFrame.AddTable();
                topTable.BottomPadding = 0;
                topTable.LeftPadding = 0;
                topTable.RightPadding = 0;
                topTable.TopPadding = 0;
                topTable.Borders.Width = 0;
                Column leftMargine = topTable.AddColumn(Unit.FromMillimeter(20));
                Column titleColumn = topTable.AddColumn(Unit.FromMillimeter(70));
                Column logoVolumn = topTable.AddColumn(Unit.FromMillimeter(100));
                Column rightMargine = topTable.AddColumn(Unit.FromMillimeter(20));
                Row r = topTable.AddRow();
                r.VerticalAlignment = VerticalAlignment.Bottom;
                // タイトル

                Paragraph paragraph = r.Cells[1].AddParagraph();
                var titleText = "領収証";
                
                FormattedText formattedText;
                foreach (char c in titleText)
                {
                    formattedText = paragraph.AddFormattedText(c.ToString());
                    formattedText.Font.Size = 20.5;
                    formattedText.Font.Bold = true;
                    formattedText.AddSpace(2);
                }
                if (a != 0)
                {
                    formattedText = paragraph.AddFormattedText("(控)");
                    formattedText.Font.Size = 20;
                    formattedText.Font.Bold = true;
                }


                // 名前
                TextFrame customerNameFrame = section.AddTextFrame();
                customerNameFrame.Width = Unit.FromMillimeter(80);
                customerNameFrame.RelativeHorizontal = RelativeHorizontal.Page;
                customerNameFrame.RelativeVertical = RelativeVertical.Page;
                customerNameFrame.Top = Unit.FromMillimeter(9 + gap);
                customerNameFrame.Left = Unit.FromMillimeter(80);

                Table customerNameTable = customerNameFrame.AddTable();
                Column CustomerNameColumn = customerNameTable.AddColumn(Unit.FromMillimeter(70));
                Column CustomerNameSuffix = customerNameTable.AddColumn(Unit.FromMillimeter(10));
                Row customerNameRow = customerNameTable.AddRow();
                customerNameTable.Borders.Bottom.Width = 0.5d;
                customerNameTable.Borders.Bottom.Color = Colors.Black;

                Paragraph customerName = customerNameRow.Cells[0].AddParagraph($"{paymentData.CustomerName.Replace(" ","　")}"); ;
                customerNameRow.Cells[0].Format.Alignment = ParagraphAlignment.Center;
                customerName.Format.Font.Bold = false;
                customerName.Format.Font.Size = 23.5;
                Paragraph customerSuffix = customerNameRow.Cells[1].AddParagraph("様");
                customerNameRow.Cells[1].VerticalAlignment = VerticalAlignment.Bottom;
                customerSuffix.Format.Font.Size = 17;
                
                // 伝票番号
                TextFrame ReceiptNumberFrame = section.AddTextFrame();
                ReceiptNumberFrame.Width = Unit.FromMillimeter(40);
                ReceiptNumberFrame.RelativeHorizontal = RelativeHorizontal.Page;
                ReceiptNumberFrame.Left = Unit.FromMillimeter(160);
                ReceiptNumberFrame.RelativeVertical = RelativeVertical.Page;
                ReceiptNumberFrame.Top = Unit.FromMillimeter(16.4 + gap);

                Table receiptNumberTable = ReceiptNumberFrame.AddTable();
                Column numLabelCol = receiptNumberTable.AddColumn(Unit.FromMillimeter(8));
                Column receiptNumberCol = receiptNumberTable.AddColumn(Unit.FromMillimeter(25));
                receiptNumberCol.Borders.Bottom.Width = 0.1;
                receiptNumberCol.Borders.Bottom.Color = Colors.Black;
                Row receiptNumberRow = receiptNumberTable.AddRow();
                numLabelCol.RightPadding = Unit.FromMillimeter(0);
                receiptNumberRow.Cells[0].VerticalAlignment = VerticalAlignment.Bottom;
                receiptNumberRow.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                receiptNumberRow.Cells[1].Format.Alignment = ParagraphAlignment.Center;
                var numLabel = receiptNumberRow.Cells[0].AddParagraph("№");
                numLabel.Format.Font.Size = 8;
                var receiptNumber = receiptNumberRow.Cells[1].AddParagraph($"{paymentData.SlipNumber}");
                receiptNumber.Format.Font.Size = 9;


                // 合計金額の表示
                TextFrame totalArea = section.AddTextFrame();
                totalArea.Width = Unit.FromMillimeter(150);
                totalArea.Height = Unit.FromMillimeter(15);
                totalArea.RelativeHorizontal = RelativeHorizontal.Page;
                totalArea.Left = Unit.FromMillimeter(28);
                totalArea.RelativeVertical = RelativeVertical.Page;
                totalArea.Top = Unit.FromMillimeter(30 + gap);

                Table totalTable = totalArea.AddTable();
                totalTable.Shading.Color = Color.FromArgb((byte)0xFF, (byte)0xDC, (byte)0xDC, (byte)0xDC);
                Column totalCol1 = totalTable.AddColumn(Unit.FromMillimeter(30));
                Column totalCol2 = totalTable.AddColumn(Unit.FromMillimeter(120));
                Row totalRow = totalTable.AddRow();
                totalRow.VerticalAlignment = VerticalAlignment.Center;
                totalRow.Cells[0].Format.Alignment = ParagraphAlignment.Center;
                var totalLabel = totalRow.Cells[0].AddParagraph("領 収 額");
                totalLabel.Format.Font.Name = sans.Regular;
                totalLabel.Format.Font.Bold = true;
                totalLabel.Format.Font.Size = 16;
                var total = totalRow.Cells[1].AddParagraph();
                total.Format.Alignment = ParagraphAlignment.Right;
                total.Format.RightIndent = Unit.FromMillimeter(5);
                total.Format.Font.Name = sans.Light;
                total.Format.Font.Size = 28;
                total.AddFormattedText(paymentData.PaymentAmount.ToString("金###,###円"), TextFormat.Bold);


                // Subject
                TextFrame subjectArea = section.AddTextFrame();
                subjectArea.Width = Unit.FromMillimeter(115);
                subjectArea.Height = Unit.FromMillimeter(20);
                subjectArea.RelativeHorizontal = RelativeHorizontal.Page;
                subjectArea.Left = Unit.FromMillimeter(50);
                subjectArea.RelativeVertical = RelativeVertical.Page;
                subjectArea.Top = Unit.FromMillimeter(48 + gap);
                Paragraph subject = subjectArea.AddParagraph($"但　{paymentData.Subject}");
                subject.Format.Font.Size = 10;
                subject.Format.Font.Name = serif.Light;
                


                // 日付
                TextFrame dateFrame = section.AddTextFrame();
                dateFrame.Width = Unit.FromMillimeter(60);
                dateFrame.RelativeHorizontal = RelativeHorizontal.Page;
                dateFrame.Left = Unit.FromMillimeter(65);
                dateFrame.RelativeVertical = RelativeVertical.Page;
                dateFrame.Top = Unit.FromMillimeter(58 + gap);
                var dateParagraph = dateFrame.AddParagraph();
                DateTextCreator(dateParagraph, paymentData.PaymentDateString, 8, 14);

                TextFrame suffixFrame = section.AddTextFrame();
                suffixFrame.Width = Unit.FromMillimeter(40);
                suffixFrame.RelativeHorizontal = RelativeHorizontal.Page;
                suffixFrame.Left = Unit.FromMillimeter(110);
                suffixFrame.RelativeVertical = RelativeVertical.Page;
                suffixFrame.Top = Unit.FromMillimeter(60.5 + gap);
                var dateSuffix = suffixFrame.AddParagraph("上記正に領収いたしました");
                dateSuffix.Format.Font.Size = 8;
                dateSuffix.Format.Alignment = ParagraphAlignment.Left;

                //ボトム
                TextFrame bottomArea = section.AddTextFrame();
                bottomArea.Width = Unit.FromMillimeter(172);
                bottomArea.Height = Unit.FromMillimeter(15);
                bottomArea.RelativeHorizontal = RelativeHorizontal.Page;
                bottomArea.RelativeVertical = RelativeVertical.Page;
                bottomArea.Top = Unit.FromMillimeter(70 + gap);
                bottomArea.Left = Unit.FromMillimeter(20);

                Table bottomTable = bottomArea.AddTable();
                bottomTable.LeftPadding = 0;
                bottomTable.TopPadding = 0;
                bottomTable.RightPadding = 0;
                bottomTable.BottomPadding = 0;
                bottomTable.Borders.Width = 0;

                Column bottomCol0 = bottomTable.AddColumn(Unit.FromMillimeter(80));
                Column bottomCol1 = bottomTable.AddColumn(Unit.FromMillimeter(18));
                Column bottomCol2 = bottomTable.AddColumn(Unit.FromMillimeter(75));
                Row bottomRow = bottomTable.AddRow();
                bottomRow.Height = Unit.FromMillimeter(20);
                var bottomC0 = bottomRow.Cells[0].AddParagraph();

                bottomRow.Cells[0].VerticalAlignment = VerticalAlignment.Center;
                bottomC0.Format.Alignment = ParagraphAlignment.Left;

                // ロゴマーク
                var logo = bottomC0.AddImage("Image/CSM_LOGO.jpg");
                logo.Width = Unit.FromMillimeter(80);

                TextFrame companyInfoArea = bottomRow.Cells[2].AddTextFrame();
                Table companyInfoTable = companyInfoArea.AddTable();
                companyInfoTable.Borders.Width = 0;
                companyInfoTable.Borders.Color = Color.FromArgb((byte)0xff, (byte)0x00, (byte)0x00, (byte)0x00);
                Column companyInfoColumn = companyInfoTable.AddColumn(Unit.FromMillimeter(65));

                Row companyNameRow = companyInfoTable.AddRow();
                Row presidentNameRow = companyInfoTable.AddRow();
                Row addressRow = companyInfoTable.AddRow();
                Row phoneNumberRow = companyInfoTable.AddRow();

                var companyNameCell = companyNameRow.Cells[0].AddParagraph($"{companyInfo.CompanyName}");
                companyNameCell.Format.Font.Size = 9;
                companyNameCell.Format.Alignment = ParagraphAlignment.Right;

                // 代表者名
                var presidentNameCell = presidentNameRow.Cells[0].AddParagraph(companyInfo.PresidentName.Replace("　","　"));
                presidentNameCell.Format.Font.Size = 12.5;
                presidentNameCell.Format.Alignment = ParagraphAlignment.Right;

                

                var addressArea = addressRow.Cells[0].AddTextFrame();
                addressArea.Height = Unit.FromMillimeter(4);
                Table addressTable = addressArea.AddTable();
                Column addressCol0 = addressTable.AddColumn(Unit.FromMillimeter(27));
                Column addressCol1 = addressTable.AddColumn(Unit.FromMillimeter(38));
                addressRow = addressTable.AddRow();
                // 法人郵便番号
                var phoneticCell = addressRow.Cells[0].AddParagraph($"〒{companyInfo.CompanyPostalcode}");
                phoneticCell.Format.Font.Size = 6;
                phoneticCell.Format.LeftIndent = Unit.FromMillimeter(0);
                phoneticCell.Format.Alignment = ParagraphAlignment.Right;
                // 法人住所
                addressRow.Cells[0].VerticalAlignment = VerticalAlignment.Center;
                var addressCell = addressRow.Cells[1].AddParagraph(companyInfo.CompanyAddress);
                addressCell.Format.Font.Size = 7.5;
                addressCell.Format.LeftIndent = Unit.FromMillimeter(0);
                addressCell.Format.Alignment = ParagraphAlignment.Right;

                // 法人電話番号1
                var phoneNumberCell = phoneNumberRow.Cells[0].AddParagraph($"℡ {companyInfo.CompanyPhone}");
                phoneNumberCell.Format.Font.Size = 7.5;
                phoneNumberCell.Format.LeftIndent = Unit.FromMillimeter(0);
                phoneNumberCell.Format.Alignment = ParagraphAlignment.Right;
                //社判
                if (a == 0)
                {
                    TextFrame stampFrame = section.AddTextFrame();
                    stampFrame.Width = Unit.FromMillimeter(25);
                    stampFrame.RelativeHorizontal = RelativeHorizontal.Page;
                    stampFrame.Left = Unit.FromMillimeter(170);
                    stampFrame.RelativeVertical = RelativeVertical.Page;
                    stampFrame.Top = Unit.FromMillimeter(61 + gap);
                    var stampParagraph = stampFrame.AddParagraph();
                    var stamp = stampFrame.AddImage("Image/stamp3.png");
                    stamp.Width = Unit.FromMillimeter(24.5);
                }


                //foreach (char c in paymentData.Message)
                //{
                //    messageCell.AddFormattedText(c.ToString(), font);
                //}
                //bottomRow.Cells[0].Borders.Color = Colors.Black;
                //bottomRow.Cells[0].Borders.Width = 0.5;



                //var bottomC2 = bottomRow.Cells[2].AddTextFrame();
                //bottomC2.Height = Unit.FromMillimeter(20);
                //Table totalsTable = bottomC2.AddTable();
                //totalsTable.Format.Font.Size = 9;
                //totalsTable.LeftPadding = 0;
                //totalsTable.TopPadding = 0;
                //totalsTable.RightPadding = 0;
                //totalsTable.BottomPadding = 0;
                //totalsTable.Borders.Width = 0.5;
                //Column totalsTableCol0 = totalsTable.AddColumn(Unit.FromMillimeter(20));
                //totalsTableCol0.Format.Shading.Color = Colors.LightGray;
                //Column totalsTableCol1 = totalsTable.AddColumn(Unit.FromMillimeter(30));

                //var subTotalRow = totalsTable.AddRow();
                //subTotalRow.Cells[0].Shading.Color = Colors.LightGray;
                //subTotalRow.VerticalAlignment = VerticalAlignment.Center;
                //subTotalRow.Height = Unit.FromMillimeter(6);
                //var totalC0 = subTotalRow.Cells[0].AddParagraph("小計");
                //totalC0.Format.Shading.Color = Colors.LightGray;
                //totalC0.Format.LeftIndent = Unit.FromMillimeter(3);
                ////var totalC1 = subTotalRow.Cells[1].AddParagraph($"{paymentData..ToString("#,0")}");
                ////totalC1.Format.RightIndent = Unit.FromMillimeter(1);
                ////totalC1.Format.Alignment = ParagraphAlignment.Right;

                //var gTotalRow = totalsTable.AddRow();
                //gTotalRow.VerticalAlignment = VerticalAlignment.Center;
                //gTotalRow.Height = Unit.FromMillimeter(6);
                //gTotalRow.Cells[0].Shading.Color = Colors.LightGray;
                //totalC0 = gTotalRow.Cells[0].AddParagraph("合計");
                //totalC0.Format.LeftIndent = Unit.FromMillimeter(3);
                //var totalC1 = gTotalRow.Cells[1].AddParagraph($"{paymentData.Total.ToString("#,0")}");
                //totalC1.Format.Alignment = ParagraphAlignment.Right;
                //totalC1.Format.RightIndent = Unit.FromMillimeter(1);

                //var bankNumRow = bottomTable.AddRow();
                //var bottomR1C0 = bankNumRow.Cells[0].AddParagraph("振込先：COMMON SENSE MATSUMOTO合同会社　松本信用金庫　南支店　普通　0293337");
                //bottomR1C0.Format.Font.Size = 8;
                //bottomR1C0.Format.Alignment = ParagraphAlignment.Right;
                //bankNumRow.VerticalAlignment = VerticalAlignment.Bottom;

            }
            // ドキュメントのレンダリングと保存
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);

            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();
            pdfRenderer.PdfDocument.Save(filePath);

            // PDFを自動的に開く（オプション）
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });


        }

        private void DateTextCreator(Paragraph paragraph, string DateString,double StringSize, double NumberSize)
        {
            var length = DateString.Length;
            var charArray = DateString.ToCharArray();
            for (var i = 0; i < length; i++)
            {
                char c = charArray[i];
                FormattedText text = paragraph.AddFormattedText(c.ToString()); 
                if(char.IsAscii(c) && c >= 48 && c <= 57)
                {
                    text.Font.Size = NumberSize;
                    Debug.WriteLine("Number");
                    if(i < length - 1)
                    {
                        var nextChar = charArray[i + 1];
                        if (!char.IsAscii(nextChar) && (nextChar !>= 48 || nextChar !<= 57))
                        {
                            text.AddSpace(2);
                        }
                    }
                }
                else
                {
                    text.Font.Size = StringSize;
                    Debug.WriteLine("String");
                    if (i < length - 1)
                    {
                        var nextChar = charArray[i + 1];
                        if (char.IsAscii(nextChar) && nextChar >= 48 && nextChar <= 57)
                        {
                            text.AddSpace(4);
                        }

                    }

                }
            }

        } 

        private void AddTextWithAlphabetFont(Paragraph paragraph, string text, string defaultFont, string alphabetFont)
        {
            foreach (char c in text)
            {
                FormattedText formattedText = paragraph.AddFormattedText(c.ToString());

                if (char.IsLetter(c) && c < 128) // 英字判定（ASCII範囲）
                {
                    formattedText.Font.Name = alphabetFont;
                    formattedText.Font.Size = 11;
                }
                else
                {
                    formattedText.Font.Name = defaultFont;
                    formattedText.Font.Size = 9;
                    formattedText.Font.Bold = false;
                }
            }
        }

        private void DefineStyles(Document document)
        {
            var sans = new FontType() { familyName = "Noto Sans JP" };
            var serif = new FontType() { familyName = "Noto Serif JP" };
            // 通常のスタイル
            Style style = document.Styles["Normal"];
            style.Font.Name = serif.Regular;  // 日本語フォントを指定
            style.Font.Size = 11;
            style.Font.Bold = false;
            // Light スタイルの定義

            Style compless = document.Styles.AddStyle("Compless", "Normal");
            compless.Font.Name = "Bodoni MT Condenced";
            compless.Font.Size = 7.5;
            compless.Font.Bold = false;

            Style lightStyle = document.Styles.AddStyle("Light", "Normal");
            lightStyle.Font.Name = serif.Light;
            lightStyle.Font.Size = 11;
            lightStyle.Font.Bold = false;
            lightStyle.Font.Italic = false;
            lightStyle.Font.Color = Colors.Black;


            // Light スタイルの定義
            Style regularStyle = document.Styles.AddStyle("Regular", "Normal");
            lightStyle.Font.Name = serif.Regular;
            lightStyle.Font.Size = 11;
            lightStyle.Font.Bold = false;
            lightStyle.Font.Italic = false;
            lightStyle.Font.Color = Colors.Black;

            // Medium スタイルの定義
            Style mediumStyle = document.Styles.AddStyle("Medium", "Normal");
            mediumStyle.Font.Name = serif.Medium;
            mediumStyle.Font.Size = 11;
            mediumStyle.Font.Bold = false;
            mediumStyle.Font.Italic = false;
            // Medium ウェイトを指定する方法は後述

            // Bold スタイルの定義
            Style boldStyle = document.Styles.AddStyle("Bold", "Normal");
            boldStyle.Font.Name = serif.Bold;
            boldStyle.Font.Size = 11;
            boldStyle.Font.Bold = true; // Bold プロパティを使用

            // タイトルのスタイル
            style = document.Styles.AddStyle("Title", "Normal");
            style.Font.Name = serif.ExtraBold;
            style.Font.Size = 18;
            style.Font.Bold = true;

            Style tytleStyle = document.Styles["Title"];
            tytleStyle.Font.Name = serif.ExtraBold;

            style.ParagraphFormat.Alignment = ParagraphAlignment.Center;


        }
    }
}
