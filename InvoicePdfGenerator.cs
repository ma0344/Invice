using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Invoice;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
using MigraDoc.DocumentObjectModel.Visitors;

using System.Windows.Media.Media3D;
using PdfSharp.Snippets.Font;
using ModernWpf.Controls;
using PdfSharp.Snippets.Drawing;
using MigraDoc.DocumentObjectModel.Shapes;
using System.Security.Permissions;




public class FontType
{
    public string Black {get{return $"{familyName} Black";}}
    public string Bold {get{return $"{familyName} Bold";}}
    public string ExtraBold {get{return $"{familyName} ExtraBold";}}
    public string ExtraLight {get{return $"{familyName} ExtraLight";}}
    public string Light {get{return $"{familyName} Light";}}
    public string Medium {get{return $"{familyName} Medium";}}
    public string Regular {get{return $"{familyName} Regular";}}
    public string SemiBold {get{return $"{familyName} SemiBold";}}
    public string VariableFont_wght {get{return $"{familyName} VariableFont_wght";}}
    public string Thin { get { return $"{familyName} Thin"; } }

    private string _familyName;
    public string familyName { get { return _familyName; } set { _familyName = value; } }


}


    public class InvoicePdfGenerator
{
    private CultureInfo cultureInfo = new("ja-JP");
    private CompanyInfo companyInfo = CompanyInfo.GetCompanyInfo();

    public InvoicePdfGenerator()
    {

        GlobalFontSettings.FontResolver = FontResolver.Instance;

        //RegisterFonts();
    }

    private void RegisterFonts()
    {
        var fontResolver = oldFontResolver.Instance;
        var fontDir = "Invoice.Fonts";
        fontResolver.AddFont("Noto Serif JP", "Black",false,fontDir);
        fontResolver.AddFont("Noto Serif JP", "Bold",false,fontDir);
        fontResolver.AddFont("Noto Serif JP", "ExtraBold",false,fontDir);
        fontResolver.AddFont("Noto Serif JP", "ExtraLight",false,fontDir);
        fontResolver.AddFont("Noto Serif JP", "Light",false, fontDir);
        fontResolver.AddFont("Noto Serif JP", "Medium",false,fontDir);
        fontResolver.AddFont("Noto Serif JP", "Regular",false,fontDir);
        fontResolver.AddFont("Noto Serif JP", "SemiBold",false,fontDir);
        fontResolver.AddFont("Noto Sans JP", "Black",false,fontDir);
        fontResolver.AddFont("Noto Sans JP", "Bold",false,fontDir);
        fontResolver.AddFont("Noto Sans JP", "ExtraBold",false,fontDir);
        fontResolver.AddFont("Noto Sans JP", "ExtraLight",false,fontDir);
        fontResolver.AddFont("Noto Sans JP", "Light",false,fontDir);
        fontResolver.AddFont("Noto Sans JP", "Medium",false,fontDir);
        fontResolver.AddFont("Noto Sans JP", "Regular",false,fontDir);
        fontResolver.AddFont("Noto Sans JP", "SemiBold",false,fontDir);
        fontResolver.AddFont("Noto Sans JP", "Thin",false,fontDir);
        fontResolver.AddFont("Bodoni MT", "Condenced", false, false, "BOD_CR.TTF");
        fontResolver.AddFont("Bodoni MT Poster Compressed", "Poster Compressed", false, false, "BOD_POST.TTF");
        fontResolver.AddFont("Gloucester MT", "Extra Condensed", false, false, "GLECB.TTF");
        fontResolver.AddFont("Palace Script MT", "", false, false, "PALSCRI.TTF");
        GlobalFontSettings.FontResolver = fontResolver;
    }

    public void CreateInvoicePdf(InvoiceClass invoiceData ,string filePath)
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
        document.Info.Title = "Hello, MigraDoc";
        document.Info.Subject = "This is MigraDoc Sample.";
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

        Paragraph centerLineParagraph = new();
        centerLineParagraph.Format.Borders.Top.Width = 0.25;
        centerLineParagraph.Format.Borders.Top.Color = Colors.Black;
        centerLineParagraph.Format.Borders.Top.Style = BorderStyle.Single;
        centerLineParagraph.Format.LeftIndent = Unit.FromMillimeter(19);
        centerLineParagraph.Format.RightIndent = Unit.FromMillimeter(19);
        centerLineParagraph.Format.SpaceBefore = Unit.FromMillimeter(148.5);
        section.Headers.Primary.Add(centerLineParagraph.Clone());

        double gapHeight = 148.5;

        for (var a = 0; a <= 1; a++)
        {
            var gap = gapHeight * a;
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
            var title = r.Cells[1].AddParagraph(a == 0 ? "御　請　求　書" : "御　請　求　書（控）") ;
            r.Cells[1].VerticalAlignment = VerticalAlignment.Center;
            title.Format.SpaceAfter = Unit.FromMillimeter(2);          
            r.Height = Unit.FromMillimeter(5);
            title.Format.Font.Size = 16;
            title.Format.Font.Bold = true;
            title.Format.Alignment = ParagraphAlignment.Left;

            var logoParagraph = r.Cells[2].AddParagraph();

            r.Cells[2].VerticalAlignment = VerticalAlignment.Bottom;
            logoParagraph.Format.Alignment = ParagraphAlignment.Right;

            // ロゴマーク
            var logo = logoParagraph.AddImage("Image/CSM_LOGO.jpg");
            logo.Width = Unit.FromMillimeter(55);

            // 下線
            Paragraph underline = topFrame.AddParagraph();
            underline.Format.Borders.Top.Width = 0.85d;
            underline.Format.Borders.Top.Color = Colors.Black;
            underline.Format.Borders.Top.Style = BorderStyle.Single;
            underline.Format.LeftIndent = Unit.FromMillimeter(19);
            underline.Format.RightIndent = Unit.FromMillimeter(19);
            underline.Format.SpaceBefore = Unit.FromMillimeter(-0.8);

            // 顧客情報
            TextFrame customerNameFrame = section.AddTextFrame();
            customerNameFrame.Width = Unit.FromMillimeter(60);
            customerNameFrame.RelativeHorizontal = RelativeHorizontal.Page;
            customerNameFrame.RelativeVertical = RelativeVertical.Page;
            customerNameFrame.Top = Unit.FromMillimeter(24 + gap);
            customerNameFrame.Left = Unit.FromMillimeter(25);


            // 名前
            Paragraph customerName = customerNameFrame.AddParagraph($"　{invoiceData.CustomerName}　様"); ;
            customerName.Format.Borders.Bottom.Width = 0.5d;
            customerName.Format.Borders.Bottom.Color = Colors.Black;
            customerName.Format.Borders.Bottom.Style = BorderStyle.Single;
            customerName.Format.Font.Bold = false;
            customerName.Format.Font.Size = 17.5;
            customerName.Format.SpaceAfter = Unit.FromCentimeter(1);


            // 合計金額の表示
            TextFrame totalArea = section.AddTextFrame();
            totalArea.Width = Unit.FromMillimeter(210);
            totalArea.RelativeHorizontal = RelativeHorizontal.Page;
            totalArea.Left = Unit.FromMillimeter(1.2);
            totalArea.RelativeVertical = RelativeVertical.Page;
            totalArea.Top = Unit.FromMillimeter(40 + gap);

            Table totalTable = totalArea.AddTable();
            Column totalCol1 = totalTable.AddColumn(Unit.FromMillimeter(25));
            Column totalCol2 = totalTable.AddColumn(Unit.FromMillimeter(30));
            Column totalCol3 = totalTable.AddColumn(Unit.FromMillimeter(65));
            Row totalRow = totalTable.AddRow();
            var totalLabel = totalRow.Cells[1].AddParagraph("合計金額");
            totalLabel.Format.Font.Bold = true;
            totalLabel.Format.Font.Size = 13;
            var total = totalRow.Cells[2].AddParagraph();
            total.Format.Alignment = ParagraphAlignment.Right;
            total.Format.RightIndent = Unit.FromMillimeter(5);
            total.Format.Font.Size = 16;
            total.AddFormattedText(invoiceData.Total.ToString("c") + "-", TextFormat.Bold);
            Paragraph totalUnderLine = totalArea.AddParagraph();
            totalUnderLine.Format.Font.Size = 1;
            totalUnderLine.Format.Borders.Top.Width = 0.5d;
            totalUnderLine.Format.Borders.Top.Color = Colors.Black;

            // Subject
            totalUnderLine.Format.Borders.Top.Style = BorderStyle.Single;
            totalUnderLine.Format.Borders.Bottom.Width = 0.5d;
            totalUnderLine.Format.Borders.Bottom.Color = Colors.Black;
            totalUnderLine.Format.Borders.Bottom.Style = BorderStyle.Single;
            totalUnderLine.Format.LeftIndent = Unit.FromMillimeter(24);
            totalUnderLine.Format.RightIndent = Unit.FromMillimeter(90);
            Paragraph subject = totalArea.AddParagraph($"下記のとおり{invoiceData.Subject}ご請求申し上げます");
            subject.Format.Borders.Top.Width = Unit.FromMillimeter(2);
            subject.Format.Borders.Top.Color = Colors.Transparent;
            subject.Format.LeftIndent = Unit.FromMillimeter(28);


            // 日付と請求書番号
            TextFrame infoFrame = section.AddTextFrame();
            infoFrame.Width = Unit.FromMillimeter(70);
            infoFrame.RelativeHorizontal = RelativeHorizontal.Page;
            infoFrame.Left = Unit.FromMillimeter(137);
            infoFrame.RelativeVertical = RelativeVertical.Page;
            infoFrame.Top = Unit.FromMillimeter(22 + gap);

            Table infoTable = infoFrame.AddTable();
            infoTable.LeftPadding = 0;
            infoTable.TopPadding = 0;
            infoTable.RightPadding = 0;
            infoTable.BottomPadding = 0;
            infoTable.Borders.Width = 0;
            Column infoMainColumn = infoTable.AddColumn(Unit.FromMillimeter(55));
            Row infoR = infoTable.AddRow();
            var slipNumCell = infoR.Cells[0].AddTextFrame();
            slipNumCell.Height = Unit.FromMillimeter(10);
            Table slipNumCellTable = slipNumCell.AddTable();
            slipNumCellTable.LeftPadding = 0;
            slipNumCellTable.TopPadding = 0;
            slipNumCellTable.RightPadding = 0;
            slipNumCellTable.BottomPadding = 0;
            slipNumCellTable.Borders.Width = 0;

            Column slipNumLabelCol = slipNumCellTable.AddColumn(Unit.FromMillimeter(28));
            Column slipNumCol = slipNumCellTable.AddColumn(Unit.FromMillimeter(26.8));
            Row slipNumRow = slipNumCellTable.AddRow();
            var slipNumC1 = slipNumRow.Cells[0].AddParagraph("請求書番号：");
            slipNumC1.Format.Font.Size = 8;
            slipNumC1.Format.Alignment = ParagraphAlignment.Right;
            var slipNumC2 = slipNumRow.Cells[1].AddParagraph($"№{invoiceData.SlipNumber}");
            slipNumC2.Format.Font.Size = 8;
            slipNumC2.Format.Alignment = ParagraphAlignment.Right;
            slipNumRow = slipNumCellTable.AddRow();
            slipNumC1 = slipNumRow.Cells[0].AddParagraph("発行日：");
            slipNumC1.Format.Font.Size = 8;
            slipNumC1.Format.Alignment = ParagraphAlignment.Right;
            slipNumC2 = slipNumRow.Cells[1].AddParagraph($"{invoiceData.DueDate.ToShortDateString()}");
            slipNumC2.Format.Font.Size = 8;
            slipNumC2.Format.Alignment = ParagraphAlignment.Right;
            // 法人名
            infoR = infoTable.AddRow();
            var companyInfoCell = infoR.Cells[0].AddParagraph();
            AddTextWithAlphabetFont(companyInfoCell, $"{companyInfo.CompanyName}", defaultFont, alphabetFont);


            companyInfoCell.Format.Font.Size = 7.5;
            companyInfoCell.Format.Alignment = ParagraphAlignment.Right;
            // 代表者名
            infoR = infoTable.AddRow();
            companyInfoCell = infoR.Cells[0].AddParagraph(companyInfo.PresidentName);
            companyInfoCell.Format.Font.Size = 9;
            companyInfoCell.Format.Alignment = ParagraphAlignment.Right;
            // 法人郵便番号
            infoR = infoTable.AddRow();
            companyInfoCell = infoR.Cells[0].AddParagraph($"〒{companyInfo.CompanyPostalcode}");
            companyInfoCell.Format.Font.Size = 7.5;
            companyInfoCell.Format.LeftIndent = Unit.FromMillimeter(11.5);
            companyInfoCell.Format.Alignment = ParagraphAlignment.Left;
            // 法人住所
            infoR = infoTable.AddRow();
            companyInfoCell = infoR.Cells[0].AddParagraph(companyInfo.CompanyAddress);
            companyInfoCell.Format.Font.Size = 7.5;
            companyInfoCell.Format.LeftIndent = Unit.FromMillimeter(11.5);
            companyInfoCell.Format.Alignment = ParagraphAlignment.Left;
            // 法人電話番号
            infoR = infoTable.AddRow();
            companyInfoCell = infoR.Cells[0].AddParagraph($"℡ {companyInfo.CompanyPhone}");
            companyInfoCell.Format.Font.Size = 7.5;
            companyInfoCell.Format.LeftIndent = Unit.FromMillimeter(11.5);
            companyInfoCell.Format.Alignment = ParagraphAlignment.Left;
            //社判
            if ( a == 0)
            {
                TextFrame stampFrame = section.AddTextFrame();
                stampFrame.Width = Unit.FromMillimeter(25);
                stampFrame.RelativeHorizontal = RelativeHorizontal.Page;
                stampFrame.Left = Unit.FromMillimeter(172);
                stampFrame.RelativeVertical = RelativeVertical.Page;
                stampFrame.Top = Unit.FromMillimeter(28 + gap);

                var stampParagraph = stampFrame.AddParagraph();
                var stamp = stampFrame.AddImage("Image/stamp3.png");
                stamp.Width = Unit.FromMillimeter(24.5);
            }

            //単位表示
            TextFrame unitFrame = section.AddTextFrame();
            Paragraph unitPara = unitFrame.AddParagraph("単位：円");
            unitPara.Format.Font.Size = 10;
            unitFrame.Width = Unit.FromMillimeter(30);
            unitFrame.RelativeHorizontal = RelativeHorizontal.Page;
            unitFrame.RelativeVertical = RelativeVertical.Page;
            unitFrame.Top = Unit.FromMillimeter(63 + gap);
            unitFrame.Left = Unit.FromMillimeter(175);

            // 請求詳細エリア

            TextFrame invoiceItemArea = section.AddTextFrame();
            invoiceItemArea.Width = Unit.FromMillimeter(172);
            invoiceItemArea.RelativeHorizontal = RelativeHorizontal.Page;
            invoiceItemArea.RelativeVertical = RelativeVertical.Page;
            invoiceItemArea.Top = Unit.FromMillimeter(68 + gap);
            invoiceItemArea.Left = Unit.FromMillimeter(20);
            // テーブルの作成
            Table table = invoiceItemArea.AddTable();
            table.Borders.Width = 0;

            // 列の定義
            Column columnNo = table.AddColumn(Unit.FromMillimeter(15));
            Column columnDescription = table.AddColumn(Unit.FromMillimeter(90));
            Column columnQuantity = table.AddColumn(Unit.FromMillimeter(8));
            Column columnUnit = table.AddColumn(Unit.FromMillimeter(8));
            Column columnUnitPrice = table.AddColumn(Unit.FromMillimeter(25));
            Column columnAmount = table.AddColumn(Unit.FromMillimeter(25));

            // ヘッダー行の追加
            Row headerRow = table.AddRow();
            headerRow.Format.Font.Size = 8;
            headerRow.Format.Alignment = ParagraphAlignment.Center;
            headerRow.Shading.Color = Colors.LightGray;
            headerRow.Cells[0].AddParagraph("No.");
            headerRow.Cells[1].AddParagraph("品目名");
            headerRow.Cells[2].AddParagraph("数量");
            headerRow.Cells[3].AddParagraph("単位");
            headerRow.Cells[4].AddParagraph("単価");
            headerRow.Cells[5].AddParagraph("金額");
            headerRow.Format.Font.Bold = false;

            // 明細行の追加
            for (var i = 0; i <= 7; i++)
            {
                Row row = table.AddRow();
                row.Height = Unit.FromMillimeter(4.8);
                if ((i + 1) % 2 == 0)
                {
                    row.Shading.Color = Color.FromArgb((byte)0x44, (byte)0xD3, (byte)0xD3, (byte)0xD3);
                    row.Borders.Top.Width = 0.2;
                    row.Borders.Top.Color = Color.FromArgb((byte)0x55, (byte)0xD3, (byte)0xD3, (byte)0xD3);
                    row.Borders.Bottom.Width = 0.2;
                    row.Borders.Bottom.Color = Color.FromArgb((byte)0x55, (byte)0xD3, (byte)0xD3, (byte)0xD3);
                    row.Borders.Left.Width = 0;
                    row.Borders.Right.Width = 0;
                }
                row.Borders.Left.Width = 0;
                row.Borders.Right.Width = 0;
                row.VerticalAlignment = VerticalAlignment.Center;
                row.Format.Font.Size = 8;
                row.Format.Font.Name = serif.Medium;


                if (i <= invoiceData.InvoiceItems.Count - 1)
                {
                    var item = invoiceData.InvoiceItems[i];
                    var para = row.Cells[0].AddParagraph(item.ItemOrder.ToString());
                    para.Format.Alignment = ParagraphAlignment.Right;
                    para = row.Cells[1].AddParagraph(item.ItemName);
                    para.Format.Borders.Width = 0;
                    para = row.Cells[2].AddParagraph(item.Quantity.ToString());
                    para.Format.Alignment = ParagraphAlignment.Center;
                    para.Format.Borders.Width = 0;
                    para = row.Cells[3].AddParagraph(item.Unit.ToString());
                    para.Format.Alignment = ParagraphAlignment.Center;
                    para.Format.Borders.Width = 0;
                    para = row.Cells[4].AddParagraph(item.UnitPrice.ToString("#,0"));
                    para.Format.Borders.Width = 0;
                    para.Format.Alignment = ParagraphAlignment.Right;
                    para = row.Cells[5].AddParagraph(item.ItemTotal.ToString("#,0"));
                    para.Format.Borders.Width = 0;
                    para.Format.Alignment = ParagraphAlignment.Right;
                }
            }

            //ボトム
            TextFrame bottomArea = section.AddTextFrame();
            bottomArea.Width = Unit.FromMillimeter(172);
            bottomArea.Height = Unit.FromMillimeter(28);
            bottomArea.RelativeHorizontal = RelativeHorizontal.Page;
            bottomArea.RelativeVertical = RelativeVertical.Page;
            bottomArea.Top = Unit.FromMillimeter(113 + gap);
            bottomArea.Left = Unit.FromMillimeter(19);

            Table bottomTable = bottomArea.AddTable();
            bottomTable.LeftPadding = 0;
            bottomTable.TopPadding = -0.5;
            bottomTable.RightPadding = 0;
            bottomTable.BottomPadding = 0;
            bottomTable.Borders.Width = 0;

            Column bottomCol0 = bottomTable.AddColumn(Unit.FromMillimeter(120));
            Column bottomCol1 = bottomTable.AddColumn(Unit.FromMillimeter(1));
            Column bottomCol2 = bottomTable.AddColumn(Unit.FromMillimeter(50));
            Row bottomRow = bottomTable.AddRow();
            bottomRow.Height = Unit.FromMillimeter(20);
            var bottomC0 = bottomRow.Cells[0].AddTextFrame();
            bottomC0.Height = Unit.FromMillimeter(10);
            var messageTable = bottomC0.AddTable();
            Column messageCol = messageTable.AddColumn(Unit.FromMillimeter(120));
            Row labelRow = messageTable.AddRow();
            labelRow.Height = Unit.FromMillimeter(3);
            var messageLabel = labelRow.Cells[0].AddParagraph("備考欄");
            messageLabel.Format.Font.Size = 7;
            messageLabel.Format.Alignment = ParagraphAlignment.Left;
            messageLabel.Format.LeftIndent = Unit.FromMillimeter(0.5);

            Row messageRow = messageTable.AddRow();
            messageRow.Height = Unit.FromMillimeter(10);
            messageRow.Format.LeftIndent = Unit.FromMillimeter(2);
            var messageCell = messageRow.Cells[0].AddParagraph();
            messageCell.Format.Font.Size = 8;
            var font = messageCell.GetFont();
            foreach (char c in invoiceData.Message)
            {
                messageCell.AddFormattedText(c.ToString(), font);
            }
            messageCell.AddLineBreak();
            bottomRow.Cells[0].Borders.Color = Colors.Black;
            bottomRow.Cells[0].Borders.Width = 0.5;



            var bottomC2 = bottomRow.Cells[2].AddTextFrame();
            bottomC2.Height = Unit.FromMillimeter(20);
            Table totalsTable = bottomC2.AddTable();
            totalsTable.Format.Font.Size = 9;
            totalsTable.LeftPadding = 0;
            totalsTable.TopPadding = 0;
            totalsTable.RightPadding = 0;
            totalsTable.BottomPadding = 0;
            totalsTable.Borders.Width = 0.5;
            Column totalsTableCol0 = totalsTable.AddColumn(Unit.FromMillimeter(20));
            totalsTableCol0.Format.Shading.Color = Colors.LightGray;
            Column totalsTableCol1 = totalsTable.AddColumn(Unit.FromMillimeter(30));

            var subTotalRow = totalsTable.AddRow();
            subTotalRow.Cells[0].Shading.Color = Colors.LightGray;
            subTotalRow.VerticalAlignment = VerticalAlignment.Center;
            subTotalRow.Height = Unit.FromMillimeter(6);
            var totalC0 = subTotalRow.Cells[0].AddParagraph("小計");
            totalC0.Format.Shading.Color = Colors.LightGray;
            totalC0.Format.LeftIndent = Unit.FromMillimeter(3);
            var totalC1 = subTotalRow.Cells[1].AddParagraph($"{invoiceData.SubTotal.ToString("#,0")}");
            totalC1.Format.RightIndent = Unit.FromMillimeter(1);
            totalC1.Format.Alignment = ParagraphAlignment.Right;

            var gTotalRow = totalsTable.AddRow();
            gTotalRow.VerticalAlignment = VerticalAlignment.Center;
            gTotalRow.Height = Unit.FromMillimeter(6);
            gTotalRow.Cells[0].Shading.Color = Colors.LightGray;
            totalC0 = gTotalRow.Cells[0].AddParagraph("合計");
            totalC0.Format.LeftIndent = Unit.FromMillimeter(3);
            totalC1 = gTotalRow.Cells[1].AddParagraph($"{invoiceData.Total.ToString("#,0")}");
            totalC1.Format.Alignment = ParagraphAlignment.Right;
            totalC1.Format.RightIndent = Unit.FromMillimeter(1);

            var bankNumRow = bottomTable.AddRow();
            var bottomR1C0 = bankNumRow.Cells[0].AddParagraph("振込先：COMMON SENSE MATSUMOTO合同会社　松本信用金庫　南支店　普通　0293337");
            bottomR1C0.Format.Font.Size = 8;
            bottomR1C0.Format.Alignment = ParagraphAlignment.Right;
            bankNumRow.VerticalAlignment = VerticalAlignment.Bottom;

        }
        // ドキュメントのレンダリングと保存
        PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
        
        pdfRenderer.Document = document;
        pdfRenderer.RenderDocument();
        pdfRenderer.PdfDocument.Save(filePath);

        // PDFを自動的に開く（オプション）
        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });


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
        var sans = new FontType() { familyName = "Noto Sans JP"};
        var serif = new FontType() { familyName = "Noto Serif JP" };
        // 通常のスタイル
        Style style = document.Styles["Normal"];
        style.Font.Name = serif.Regular;  // 日本語フォントを指定
        style.Font.Size = 11;
        style.Font.Bold = false;
        // Light スタイルの定義

        Style compless = document.Styles.AddStyle("Compless","Normal");
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
