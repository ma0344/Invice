// FontResolver.cs
using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class FontResolver : IFontResolver
{
    public static readonly FontResolver Instance = new FontResolver();
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The font family names that can be used in the constructor of XFont.
    /// Used in the first parameter of ResolveTypeface.
    /// Family names are given in lower case because the implementation of
    /// SegoeWpFontResolver ignores case.
    /// </summary>
    public static class FamilyNames
    {
        public const string NotoSerifJPBlack = "noto serif jp black";
        public const string NotoSerifJPBold = "noto serif jp bold";
        public const string NotoSerifJPExtraBold = "noto serif jp extrabold";
        public const string NotoSerifJPExtraLight = "noto serif jp extralight";
        public const string NotoSerifJPLight = "noto serif jp light";
        public const string NotoSerifJPMedium = "noto serif jp medium";
        public const string NotoSerifJPRegular = "noto serif jp regular";
        public const string NotoSerifJPSemiBold = "noto serif jp semibold";
        public const string NotoSansJPBlack = "noto sans jp black";
        public const string NotoSansJPBold = "noto sans jp bold";
        public const string NotoSansJPExtraBold = "noto sans jp extrabold";
        public const string NotoSansJPExtraLight = "noto sans jp extralight";
        public const string NotoSansJPLight = "noto sans jp light";
        public const string NotoSansJPMedium = "noto sans jp medium";
        public const string NotoSansJPRegular = "noto sans jp regular";
        public const string NotoSansJPSemiBold = "noto sans jp semibold";
        public const string NotoSansJPThin = "noto sans jp thin";
        public const string BodoniMTCondenced ="bodoni mt condenced";
        public const string BodoniMTPosterCompressed = "bodoni mt poster compressed";
        public const string GloucesterMTExtraCondensed = "gloucester mt extra condensed";
        public const string PalaceScriptMT = "palace script mt";

        // This implementation considers each font face as its own family.
        public const string SegoeWPLight = "segoe wp light";
        public const string SegoeWPSemilight = "segoe wp semilight";
        public const string SegoeWP = "segoe wp";
        public const string SegoeWPSemibold = "segoe wp semibold";
        public const string SegoeWPBold = "segoe wp bold";
        public const string SegoeWPBlack = "segoe wp black";
    }

    /// <summary>
    /// The internal names that uniquely identify the font faces.
    /// Used in the first parameter of the FontResolverInfo constructor.
    /// </summary>
    static class FaceNames
    {
        public const string NotoSerifJPBlack = "NotoSerifJPBlack";
        public const string NotoSerifJPBold = "NotoSerifJPBold";
        public const string NotoSerifJPExtraBold = "NotoSerifJPExtraBold";
        public const string NotoSerifJPExtraLight = "NotoSerifJPExtraLight";
        public const string NotoSerifJPLight = "NotoSerifJPLight";
        public const string NotoSerifJPMedium = "NotoSerifJPMedium";
        public const string NotoSerifJPRegular = "NotoSerifJPRegular";
        public const string NotoSerifJPSemiBold = "NotoSerifJPSemiBold";
        public const string NotoSansJPBlack = "NotoSansJPBlack";
        public const string NotoSansJPBold = "NotoSansJPBold";
        public const string NotoSansJPExtraBold = "NotoSansJPExtraBold";
        public const string NotoSansJPExtraLight = "NotoSansJPExtraLight";
        public const string NotoSansJPLight = "NotoSansJPLight";
        public const string NotoSansJPMedium = "NotoSansJPMedium";
        public const string NotoSansJPRegular = "NotoSansJPRegular";
        public const string NotoSansJPSemiBold = "NotoSansJPSemiBold";
        public const string NotoSansJPThin = "NotoSansJPThin";
        public const string BodoniMTCondenced = "BodoniMTCondenced";
        public const string BodoniMTPosterCompressed = "BodoniMTPosterCompressed";
        public const string GloucesterMTExtraCondensed = "GloucesterMTExtraCondensed";
        public const string PalaceScriptMT = "PalaceScriptMT";

        // Used in the first parameter of the FontResolverInfo constructor.
        public const string SegoeWPLight = "SegoeWPLight";
        public const string SegoeWPSemilight = "SegoeWPSemilight";
        public const string SegoeWP = "SegoeWP";
        public const string SegoeWPSemibold = "SegoeWPSemibold";
        public const string SegoeWPBold = "SegoeWPBold";
        public const string SegoeWPBlack = "SegoeWPBlack";
    }

    static class FileName
    {
        public const string NotoSerifJPBlack = "Invoice.Fonts.NotoSerifJP-Black.ttf";
        public const string NotoSerifJPBold = "Invoice.Fonts.NotoSerifJP-Bold.ttf";
        public const string NotoSerifJPExtraBold = "Invoice.Fonts.NotoSerifJP-ExtraBold.ttf";
        public const string NotoSerifJPExtraLight = "Invoice.Fonts.NotoSerifJP-ExtraLight.ttf";
        public const string NotoSerifJPLight = "Invoice.Fonts.NotoSerifJP-Light.ttf";
        public const string NotoSerifJPMedium = "Invoice.Fonts.NotoSerifJP-Medium.ttf";
        public const string NotoSerifJPRegular = "Invoice.Fonts.NotoSerifJP-Regular.ttf";
        public const string NotoSerifJPSemiBold = "Invoice.Fonts.NotoSerifJP-SemiBold.ttf";
        public const string NotoSansJPBlack = "Invoice.Fonts.NotoSansJP-Black.ttf";
        public const string NotoSansJPBold = "Invoice.Fonts.NotoSansJP-Bold.ttf";
        public const string NotoSansJPExtraBold = "Invoice.Fonts.NotoSansJP-ExtraBold.ttf";
        public const string NotoSansJPExtraLight = "Invoice.Fonts.NotoSansJP-ExtraLight.ttf";
        public const string NotoSansJPLight = "Invoice.Fonts.NotoSansJP-Light.ttf";
        public const string NotoSansJPMedium = "Invoice.Fonts.NotoSansJP-Medium.ttf";
        public const string NotoSansJPRegular = "Invoice.Fonts.NotoSansJP-Regular.ttf";
        public const string NotoSansJPSemiBold = "Invoice.Fonts.NotoSansJP-SemiBold.ttf";
        public const string NotoSansJPThin = "Invoice.Fonts.NotoSansJP-Thin.ttf";
        public const string BodoniMTCondenced = "Invoice.Fonts.BOD_CR.TTF";
        public const string BodoniMTPosterCompressed = "Invoice.Fonts.BOD_PSTC.TTF"; 
        public const string GloucesterMTExtraCondensed = "Invoice.Fonts.GLECB.TTF";
        public const string PalaceScriptMT = "Invoice.Fonts.PALSCRI.TTF";


    }

    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Selects a physical font face based on the specified information
    /// of a required typeface.
    /// </summary>
    /// <param name="familyName">Name of the font family.</param>
    /// <param name="isBold">Set to <c>true</c> when a bold font face
    ///  is required.</param>
    /// <param name="isItalic">Set to <c>true</c> when an italic font face 
    /// is required.</param>
    /// <returns>
    /// Information about the physical font, or null if the request cannot be satisfied.
    /// </returns>
    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // Note: PDFsharp calls ResolveTypeface only once for each unique combination
        // of familyName, isBold, and isItalic.

        string lowercaseFamilyName = familyName.ToLowerInvariant();

        switch (lowercaseFamilyName)
        {

        }

        if (lowercaseFamilyName.StartsWith("noto serif jp", StringComparison.Ordinal))
        {
            string faceName;
            switch (lowercaseFamilyName)
            {
                case FamilyNames.NotoSerifJPBlack:
                    faceName = FaceNames.NotoSerifJPBlack;
                    break;
                case FamilyNames.NotoSerifJPBold:
                    faceName = FaceNames.NotoSerifJPBold;
                    break;
                case FamilyNames.NotoSerifJPExtraBold:
                    faceName = FaceNames.NotoSerifJPExtraBold;
                    break;
                case FamilyNames.NotoSerifJPExtraLight:
                    faceName = FaceNames.NotoSerifJPExtraLight;
                    break;
                case FamilyNames.NotoSerifJPLight:
                    faceName = FaceNames.NotoSerifJPLight;
                    break;
                case FamilyNames.NotoSerifJPMedium:
                    faceName = FaceNames.NotoSerifJPMedium;
                    break;
                case FamilyNames.NotoSerifJPRegular:
                    faceName = FaceNames.NotoSerifJPRegular;
                    break;
                case FamilyNames.NotoSerifJPSemiBold:
                    faceName = FaceNames.NotoSerifJPSemiBold;
                    break;
                default:
                    return null;
            }
            // Tell PDFsharp the effective face name and whether italic should be
            // simulated. Since Noto Serif JP typefaces do not contain any italic font
            // always simulate italic if it is requested.
            return new FontResolverInfo(faceName, isBold, isItalic);
        }

        else if (lowercaseFamilyName.StartsWith("noto sans", StringComparison.Ordinal))
        {
            string faceName;
            switch (lowercaseFamilyName)
            {
                case FamilyNames.NotoSansJPBlack:
                    faceName = FaceNames.NotoSansJPBlack;
                    break;
                case FamilyNames.NotoSansJPBold:
                    faceName = FaceNames.NotoSansJPBold;
                    break;
                case FamilyNames.NotoSansJPExtraBold:
                    faceName = FaceNames.NotoSansJPExtraBold;
                    break;
                case FamilyNames.NotoSansJPExtraLight:
                    faceName = FaceNames.NotoSansJPExtraLight;
                    break;
                case FamilyNames.NotoSansJPLight:
                    faceName = FaceNames.NotoSansJPLight;
                    break;
                case FamilyNames.NotoSansJPMedium:
                    faceName = FaceNames.NotoSansJPMedium;
                    break;
                case FamilyNames.NotoSansJPRegular:
                    faceName = FaceNames.NotoSansJPRegular;
                    break;
                case FamilyNames.NotoSansJPSemiBold:
                    faceName = FaceNames.NotoSansJPSemiBold;
                    break;
                case FamilyNames.NotoSansJPThin:
                    faceName = FaceNames.NotoSansJPThin;
                    break;
                default:
                    return null;
            }
            // Tell PDFsharp the effective face name and whether italic should be
            // simulated. Since Noto Sans JP typefaces do not contain any italic font
            // always simulate italic if it is requested.
            return new FontResolverInfo(faceName, isBold, isItalic);
        }

        else if (lowercaseFamilyName.StartsWith("segoe wp", StringComparison.Ordinal))
        {
            // Bold simulation is not yet implemented in PDFsharp.
            const bool simulateBold = false;

            string faceName;

            // In this sample family names are case-sensitive. You can relax this
            // in your own implementation and make them case-insensitive.
            switch (lowercaseFamilyName)
            {
                case FamilyNames.SegoeWPLight:
                    // Just for demonstration use 'Semilight' if bold is requested.
                    if (isBold)
                        goto case FamilyNames.SegoeWPSemilight;
                    faceName = FaceNames.SegoeWPLight;
                    break;

                case FamilyNames.SegoeWPSemilight:
                    // Do not care about bold for semilight.
                    faceName = FaceNames.SegoeWPSemilight;
                    break;

                case FamilyNames.SegoeWP:
                    // Use font 'Bold' if bold is requested.
                    if (isBold)
                        goto UseSegoeWPBold;
                    faceName = FaceNames.SegoeWP;
                    break;

                case FamilyNames.SegoeWPSemibold:
                    // Do not care about bold for semibold.
                    faceName = FaceNames.SegoeWPSemibold;
                    break;

                case FamilyNames.SegoeWPBold:
                    // Just for demonstration use font 'Black' if bold is requested.
                    if (isBold)
                        goto case FamilyNames.SegoeWPBlack;
                    UseSegoeWPBold:
                    faceName = FaceNames.SegoeWPBold;
                    break;

                case FamilyNames.SegoeWPBlack:
                    // Do not care about bold for black.
                    faceName = FaceNames.SegoeWPBlack;
                    break;

                default:
                    return null;
            }

            // Tell PDFsharp the effective face name and whether italic should be
            // simulated. Since Segoe WP typefaces do not contain any italic font
            // always simulate italic if it is requested.
            return new FontResolverInfo(faceName, simulateBold, isItalic);
        }

        else
        {
            string faceName = "";
            switch (lowercaseFamilyName)
            {
                case FamilyNames.BodoniMTCondenced:
                    faceName = FaceNames.BodoniMTCondenced;
                    break;
                case FamilyNames.BodoniMTPosterCompressed:
                    faceName = FaceNames.BodoniMTPosterCompressed;
                    break;
                case FamilyNames.GloucesterMTExtraCondensed:
                    faceName = FaceNames.GloucesterMTExtraCondensed;
                    break;
                case FamilyNames.PalaceScriptMT:
                    faceName = FaceNames.PalaceScriptMT;
                    break;
                default:
                    faceName = "";;
                    break;
            }
            if (faceName != "") return new FontResolverInfo(faceName, isBold, isItalic);
        }
        // Return null means that the typeface cannot be resolved and PDFsharp forwards
        // the typeface request depending on PDFsharp build flavor and operating system.
        // Alternatively forward call to PlatformFontResolver.


        return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
    }



    /// <summary>
    /// Gets the bytes of a physical font face with specified face name.
    /// </summary>
    /// <param name="faceName">A face name previously retrieved by ResolveTypeface.</param>
    /// <returns>
    /// The bits of the font.
    /// </returns>
    public byte[]? GetFont(string faceName)
    {
        // Note: PDFsharp never calls GetFont twice with the same face name.
        // Note: If a typeface is resolved by the PlatformFontResolver.ResolveTypeface
        //       you never come here.

        // Return the bytes of a font.
        return faceName switch
        {


            FaceNames.NotoSerifJPBlack => LoadFontData(FileName.NotoSerifJPBlack),
            FaceNames.NotoSerifJPBold => LoadFontData(FileName.NotoSerifJPBold),
            FaceNames.NotoSerifJPExtraBold => LoadFontData(FileName.NotoSerifJPExtraBold),
            FaceNames.NotoSerifJPExtraLight => LoadFontData(FileName.NotoSerifJPExtraLight),
            FaceNames.NotoSerifJPLight => LoadFontData(FileName.NotoSerifJPLight),
            FaceNames.NotoSerifJPMedium => LoadFontData(FileName.NotoSerifJPMedium),
            FaceNames.NotoSerifJPRegular => LoadFontData(FileName.NotoSerifJPRegular),
            FaceNames.NotoSerifJPSemiBold => LoadFontData(FileName.NotoSerifJPSemiBold),
            FaceNames.NotoSansJPBlack => LoadFontData(FileName.NotoSansJPBlack),
            FaceNames.NotoSansJPBold => LoadFontData(FileName.NotoSansJPBold),
            FaceNames.NotoSansJPExtraBold => LoadFontData(FileName.NotoSansJPExtraBold),
            FaceNames.NotoSansJPExtraLight => LoadFontData(FileName.NotoSansJPExtraLight),
            FaceNames.NotoSansJPLight => LoadFontData(FileName.NotoSansJPLight),
            FaceNames.NotoSansJPMedium => LoadFontData(FileName.NotoSansJPMedium),
            FaceNames.NotoSansJPRegular => LoadFontData(FileName.NotoSansJPRegular),
            FaceNames.NotoSansJPSemiBold => LoadFontData(FileName.NotoSansJPSemiBold),
            FaceNames.NotoSansJPThin => LoadFontData(FileName.NotoSansJPThin),
            FaceNames.BodoniMTCondenced => LoadFontData(FileName.BodoniMTCondenced),
            FaceNames.BodoniMTPosterCompressed => LoadFontData(FileName.BodoniMTPosterCompressed),
            FaceNames.GloucesterMTExtraCondensed => LoadFontData(FileName.GloucesterMTExtraCondensed),
            FaceNames.PalaceScriptMT => LoadFontData(FileName.PalaceScriptMT),
            FaceNames.SegoeWPLight => PdfSharp.WPFonts.FontDataHelper.SegoeWPLight,
            FaceNames.SegoeWPSemilight => PdfSharp.WPFonts.FontDataHelper.SegoeWPSemilight,
            FaceNames.SegoeWP => PdfSharp.WPFonts.FontDataHelper.SegoeWP,
            FaceNames.SegoeWPSemibold => PdfSharp.WPFonts.FontDataHelper.SegoeWPSemibold,
            FaceNames.SegoeWPBold => PdfSharp.WPFonts.FontDataHelper.SegoeWPBold,
            FaceNames.SegoeWPBlack => PdfSharp.WPFonts.FontDataHelper.SegoeWPBlack,
            // PDFsharp never calls GetFont with a face name that was not returned
            // by ResolveTypeface.
            _ => null  // Comes here if faceName is from another font resolver.
        };
    }
    private byte[] LoadFontData(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
                throw new InvalidOperationException($"フォントリソース '{resourceName}' が見つかりません。");

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }

}
public class oldFontResolver : IFontResolver
{
    public static readonly oldFontResolver Instance = new oldFontResolver();

    private readonly Dictionary<string, string> fontFiles = new();

    public void AddFont(string familyName, string weight, bool isItalic, string dirName)
    {
        string filename = $"{dirName}.{familyName.Replace(" ", "")}-{weight}.ttf";
        string faceName = CreateFaceName(familyName, weight, isItalic);
        fontFiles[faceName] = filename;
    }

    public void AddFont(string familyName,string face, bool isBold, bool isItalic, string fileName)
    {
        string filename = $"Fonts.{fileName}";
        string faceName = $"{familyName} {face}";
        if (string.IsNullOrWhiteSpace(face)) faceName = familyName;
        fontFiles[faceName] = filename;
    }


    public byte[] GetFont(string faceName)
    {
        if (fontFiles.TryGetValue(faceName, out string resourceName))
        {
            return LoadFontData(resourceName);
        }
        throw new InvalidOperationException($"フォント '{faceName}' が見つかりません。");
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // ウェイトを文字列で扱う
        string weight = isBold ? "Bold" : "Regular";
        string faceName = CreateFaceName(familyName, weight, isItalic);

        if (fontFiles.ContainsKey(faceName))
        {
            return new FontResolverInfo(faceName);
        }
        // ウェイトが見つからない場合、他のウェイトで試行
        string[] weights = new[] { "Thin", "ExtraLight", "Light", "Regular", "Medium", "SemiBold", "Bold", "ExtraBold", "Black" };
        foreach (var w in weights)
        {
            faceName = CreateFaceName(familyName, w, isItalic);
            if (fontFiles.ContainsKey(faceName))
            {
                return new FontResolverInfo(faceName);
            }
        }

        // デフォルトのフォントにフォールバック
        return PlatformFontResolver.ResolveTypeface("Arial", isBold, isItalic);
    }

    private string CreateFaceName(string familyName, string weight, bool isItalic)
    {
        string style = isItalic ? "Italic" : "Normal";
        return $"{familyName}-{weight}-{style}";
    }

    private byte[] LoadFontData(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
                throw new InvalidOperationException($"フォントリソース '{resourceName}' が見つかりません。");

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
