using SimplyScriptures.Common.Enums;
using System.Text;

namespace SimplyScriptures.Common.Extensions;

public static class EnumExtensions
{
    public static string ToDisplayString(this ScriptureBook book)
    {
        switch (book)
        {
            case ScriptureBook.OT1:
                return "Old Testament (Genesis - Ruth)";
            case ScriptureBook.OT2:
                return "Old Testament (1 Samuel - Ecclesiastes)";
            case ScriptureBook.OT3:
                return "Old Testament (Isaiah - Malachi)";
            case ScriptureBook.NT:
                return "New Testament";
            case ScriptureBook.BM:
                return "Book of Mormon";
            case ScriptureBook.DC:
                return "Doctrine & Covenants";
        }

        var str = book.ToString();
        var num = str.IndexOf('_');
        var alphaNumeric = str.Substring(num + 1).SeparateAlphaNumeric().TrimToAlphaNumeric();
        var stringBuilder = new StringBuilder();
        var str1 = alphaNumeric;

        foreach (var chr in str1)
        {
            if (char.IsUpper(chr))
            {
                stringBuilder.Append(' ');
            }

            stringBuilder.Append(chr);
        }

        return stringBuilder.ToString().Trim().Replace("  ", " ").Replace("  ", " ");
    }

    public static string ToAbbreviatedDisplayString(this ScriptureBook book)
    {
        if (book.IsOldTestament())
        {
            return "OT";
        }

        if (book.IsNewTestament())
        {
            return "NT";
        }

        if (book.IsBookOfMormon())
        {
            return "BM";
        }

        if (book.IsDoctrineAndCovenants())
        {
            return "DC";
        }

        throw new Exception($"Invalid book: {book}");
    }

    public static string ToHtmlDisplayString(this ScriptureBook book)
    {
        return book.ToDisplayString()
            .Replace(" & ", "&amp;");
    }

    public static bool IsOldTestament(this ScriptureBook book)
    {
        return book.ToString().StartsWith("OT");
    }

    public static bool IsOldTestament1(this ScriptureBook book)
    {
        return book.ToString().StartsWith("OT1");
    }

    public static bool IsOldTestament2(this ScriptureBook book)
    {
        return book.ToString().StartsWith("OT2");
    }

    public static bool IsOldTestament3(this ScriptureBook book)
    {
        return book.ToString().StartsWith("OT3");
    }

    public static bool IsNewTestament(this ScriptureBook book)
    {
        return book.ToString().StartsWith("NT");
    }

    public static bool IsBookOfMormon(this ScriptureBook book)
    {
        return book.ToString().StartsWith("BM");
    }

    public static bool IsDoctrineAndCovenants(this ScriptureBook book)
    {
        return book.ToString().StartsWith("DC");
    }

    public static ScriptureBook ToRootBook(this ScriptureBook book)
    {
        if (book.IsOldTestament1())
        {
            return ScriptureBook.OT1;
        }

        if (book.IsOldTestament2())
        {
            return ScriptureBook.OT2;
        }

        if (book.IsOldTestament3())
        {
            return ScriptureBook.OT3;
        }

        if (book.IsNewTestament())
        {
            return ScriptureBook.NT;
        }

        if (book.IsBookOfMormon())
        {
            return ScriptureBook.BM;
        }

        if (book.IsDoctrineAndCovenants())
        {
            return ScriptureBook.DC;
        }

        throw new Exception($"Invalid book: {book}");
    }

    public static ScriptureBook ToSpecificBook(this ScriptureBook book)
    {
        switch (book)
        {
            case ScriptureBook.OT1:
                return ScriptureBook.OT1_About;
            case ScriptureBook.OT2:
                return ScriptureBook.OT2_About;
            case ScriptureBook.OT3:
                return ScriptureBook.OT3_About;
            case ScriptureBook.NT:
                return ScriptureBook.NT_About;
            case ScriptureBook.BM:
                return ScriptureBook.BM_About;
            case ScriptureBook.DC:
                return ScriptureBook.DC_About;
            default:
                return book;
        }
    }

    public static string ToPDFPath(this ScriptureBook book)
    {
        if (book.IsOldTestament1())
        {
            return "./Scriptures/PDF/OT1.pdf";
        }

        if (book.IsOldTestament2())
        {
            return "./Scriptures/PDF/OT2.pdf";
        }

        if (book.IsOldTestament3())
        {
            return "./Scriptures/PDF/OT3.pdf";
        }

        if (book.IsNewTestament())
        {
            return "./Scriptures/PDF/NT.pdf";
        }

        if (book.IsBookOfMormon())
        {
            return "./Scriptures/PDF/BM.pdf";
        }

        if (book.IsDoctrineAndCovenants())
        {
            return "./Scriptures/PDF/DC.pdf";
        }

        throw new Exception($"Invalid book: {book}");
    }

    public static string ToMenuContentPath(this ScriptureBook book)
    {
        if (book.IsOldTestament1())
        {
            return "./Scriptures/OT1/OT1.json";
        }

        if (book.IsOldTestament2())
        {
            return "./Scriptures/OT2/OT2.json";
        }

        if (book.IsOldTestament3())
        {
            return "./Scriptures/OT3/OT3.json";
        }

        if (book.IsNewTestament())
        {
            return "./Scriptures/NT/NT.json";
        }

        if (book.IsBookOfMormon())
        {
            return "./Scriptures/BM/BM.json";
        }

        if (book.IsDoctrineAndCovenants())
        {
            return "./Scriptures/DC/DC.json";
        }

        throw new Exception($"Invalid book: {book}");
    }

    public static string ToHtmlPath(this ScriptureBook book, bool isUnknownBookError = true)
    {
        var text = book.ToString();
        //if (book == ScriptureBook.BM) return "./Scriptures/BM/BM.html";
        switch (book)
        {
            case ScriptureBook.BM_1Nephi:
                return "./Scriptures/BM/BM_1Nephi.html";
            case ScriptureBook.BM_2Nephi:
                return "./Scriptures/BM/BM_2Nephi.html";
            case ScriptureBook.BM_3Nephi:
                return "./Scriptures/BM/BM_3Nephi.html";
            case ScriptureBook.BM_3Witness:
                return "./Scriptures/BM/BM_3Witness.html";
            case ScriptureBook.BM_4Nephi:
                return "./Scriptures/BM/BM_4Nephi.html";
            case ScriptureBook.BM_8Witness:
                return "./Scriptures/BM/BM_8Witness.html";
            case ScriptureBook.BM_About:
                return "./Scriptures/BM/BM_About.html";
            case ScriptureBook.BM_Alma:
                return "./Scriptures/BM/BM_Alma.html";
            case ScriptureBook.BM_Enos:
                return "./Scriptures/BM/BM_Enos.html";
            case ScriptureBook.BM_Ether:
                return "./Scriptures/BM/BM_Ether.html";
            case ScriptureBook.BM_Helaman:
                return "./Scriptures/BM/BM_Helaman.html";
            case ScriptureBook.BM_Jacob:
                return "./Scriptures/BM/BM_Jacob.html";
            case ScriptureBook.BM_Jarom:
                return "./Scriptures/BM/BM_Jarom.html";
            case ScriptureBook.BM_Mormon:
                return "./Scriptures/BM/BM_Mormon.html";
            case ScriptureBook.BM_Moroni:
                return "./Scriptures/BM/BM_Moroni.html";
            case ScriptureBook.BM_Mosiah:
                return "./Scriptures/BM/BM_Mosiah.html";
            case ScriptureBook.BM_Omni:
                return "./Scriptures/BM/BM_Omni.html";
            case ScriptureBook.BM_TitlePage:
                return "./Scriptures/BM/BM_TitlePage.html";
            case ScriptureBook.BM_Words:
                return "./Scriptures/BM/BM_Words.html";
            case ScriptureBook.DC_1832Account:
                return "./Scriptures/DC/DC_1832Account.html";
            case ScriptureBook.DC_1835Account:
                return "./Scriptures/DC/DC_1835Account.html";
            case ScriptureBook.DC_1838Account:
                return "./Scriptures/DC/DC_1838Account.html";
            case ScriptureBook.DC_1842Account:
                return "./Scriptures/DC/DC_1842Account.html";
            case ScriptureBook.DC_About:
                return "./Scriptures/DC/DC_About.html";
            case ScriptureBook.DC_Abraham:
                return "./Scriptures/DC/DC_Abraham.html";
            case ScriptureBook.DC_Articles:
                return "./Scriptures/DC/DC_Articles.html";
            case ScriptureBook.DC_Lectures:
                return "./Scriptures/DC/DC_Lectures.html";
            case ScriptureBook.DC_LetterLibertyJail:
                return "./Scriptures/DC/DC_LetterLibertyJail.html";
            case ScriptureBook.DC_Matthew25:
                return "./Scriptures/DC/DC_Matthew25.html";
            case ScriptureBook.DC_MoroniVisit:
                return "./Scriptures/DC/DC_MoroniVisit.html";
            case ScriptureBook.DC_Moses:
                return "./Scriptures/DC/DC_Moses.html";
            case ScriptureBook.DC_Order:
                return "./Scriptures/DC/DC_Order.html";
            case ScriptureBook.DC_Preface:
                return "./Scriptures/DC/DC_Preface.html";
            case ScriptureBook.DC_Reception:
                return "./Scriptures/DC/DC_Reception.html";
            case ScriptureBook.DC_Sections:
                return "./Scriptures/DC/DC_Sections.html";
        }

        if (text.StartsWith("DC_Lecture"))
        {
            return "./Scriptures/DC/DC_Sections.html";
        }

        if (text.StartsWith("DC_Section"))
        {
            return "./Scriptures/DC/DC_Sections.html";
        }

        //if (book == ScriptureBook.NT) return "./Scriptures/NT/NT.html";
        switch (book)
        {
            case ScriptureBook.NT_1Corinthians:
                return "./Scriptures/NT/NT_1Corinthians.html";
            case ScriptureBook.NT_1John:
                return "./Scriptures/NT/NT_1John.html";
            case ScriptureBook.NT_1Peter:
                return "./Scriptures/NT/NT_1Peter.html";
            case ScriptureBook.NT_1Thessalonians:
                return "./Scriptures/NT/NT_1Thessalonians.html";
            case ScriptureBook.NT_1Timothy:
                return "./Scriptures/NT/NT_1Timothy.html";
            case ScriptureBook.NT_2Corinthians:
                return "./Scriptures/NT/NT_2Corinthians.html";
            case ScriptureBook.NT_2John:
                return "./Scriptures/NT/NT_2John.html";
            case ScriptureBook.NT_2Peter:
                return "./Scriptures/NT/NT_2Peter.html";
            case ScriptureBook.NT_2Thessalonians:
                return "./Scriptures/NT/NT_2Thessalonians.html";
            case ScriptureBook.NT_2Timothy:
                return "./Scriptures/NT/NT_2Timothy.html";
            case ScriptureBook.NT_3John:
                return "./Scriptures/NT/NT_3John.html";
            case ScriptureBook.NT_About:
                return "./Scriptures/NT/NT_About.html";
            case ScriptureBook.NT_Acts:
                return "./Scriptures/NT/NT_Acts.html";
            case ScriptureBook.NT_Colossians:
                return "./Scriptures/NT/NT_Colossians.html";
            case ScriptureBook.NT_Ephesians:
                return "./Scriptures/NT/NT_Ephesians.html";
            case ScriptureBook.NT_Galatians:
                return "./Scriptures/NT/NT_Galatians.html";
            case ScriptureBook.NT_Hebrews:
                return "./Scriptures/NT/NT_Hebrews.html";
            case ScriptureBook.NT_James:
                return "./Scriptures/NT/NT_James.html";
            case ScriptureBook.NT_John:
                return "./Scriptures/NT/NT_John.html";
            case ScriptureBook.NT_Jude:
                return "./Scriptures/NT/NT_Jude.html";
            case ScriptureBook.NT_Luke:
                return "./Scriptures/NT/NT_Luke.html";
            case ScriptureBook.NT_Mark:
                return "./Scriptures/NT/NT_Mark.html";
            case ScriptureBook.NT_Matthew:
                return "./Scriptures/NT/NT_Matthew.html";
            case ScriptureBook.NT_Philemon:
                return "./Scriptures/NT/NT_Philemon.html";
            case ScriptureBook.NT_Philippians:
                return "./Scriptures/NT/NT_Philippians.html";
            case ScriptureBook.NT_Revelation:
                return "./Scriptures/NT/NT_Revelation.html";
            case ScriptureBook.NT_Romans:
                return "./Scriptures/NT/NT_Romans.html";
            case ScriptureBook.NT_Titus:
                return "./Scriptures/NT/NT_Titus.html";
            case ScriptureBook.OT1_About:
                return "./Scriptures/OT1/OT1_About.html";
            case ScriptureBook.OT1_DescendantsTerah:
                return "./Scriptures/OT1/OT1_DescendantsTerah.html";
            case ScriptureBook.OT1_Deuteronomy:
                return "./Scriptures/OT1/OT1_Deuteronomy.html";
            case ScriptureBook.OT1_Exodus:
                return "./Scriptures/OT1/OT1_Exodus.html";
            case ScriptureBook.OT1_Genesis:
                return "./Scriptures/OT1/OT1_Genesis.html";
            case ScriptureBook.OT1_Joshua:
                return "./Scriptures/OT1/OT1_Joshua.html";
            case ScriptureBook.OT1_Judges:
                return "./Scriptures/OT1/OT1_Judges.html";
            case ScriptureBook.OT1_Leviticus:
                return "./Scriptures/OT1/OT1_Leviticus.html";
            case ScriptureBook.OT1_MasoreticTimeline:
                return "./Scriptures/OT1/OT1_MasoreticTimeline.html";
            case ScriptureBook.OT1_Numbers:
                return "./Scriptures/OT1/OT1_Numbers.html";
            case ScriptureBook.OT1_Ruth:
                return "./Scriptures/OT1/OT1_Ruth.html";
            case ScriptureBook.OT1_SeptuagintTimeline:
                return "./Scriptures/OT1/OT1_SeptuagintTimeline.html";
            case ScriptureBook.OT2_1Chronicles:
                return "./Scriptures/OT2/OT2_1Chronicles.html";
            case ScriptureBook.OT2_1Kings:
                return "./Scriptures/OT2/OT2_1Kings.html";
            case ScriptureBook.OT2_1Samuel:
                return "./Scriptures/OT2/OT2_1Samuel.html";
            case ScriptureBook.OT2_2Chronicles:
                return "./Scriptures/OT2/OT2_2Chronicles.html";
            case ScriptureBook.OT2_2Kings:
                return "./Scriptures/OT2/OT2_2Kings.html";
            case ScriptureBook.OT2_2Samuel:
                return "./Scriptures/OT2/OT2_2Samuel.html";
            case ScriptureBook.OT2_About:
                return "./Scriptures/OT2/OT2_About.html";
            case ScriptureBook.OT2_ChronologyKings:
                return "./Scriptures/OT2/OT2_ChronologyKings.html";
            case ScriptureBook.OT2_Ecclesiastes:
                return "./Scriptures/OT2/OT2_Ecclesiastes.html";
            case ScriptureBook.OT2_Esther:
                return "./Scriptures/OT2/OT2_Esther.html";
            case ScriptureBook.OT2_Ezra:
                return "./Scriptures/OT2/OT2_Ezra.html";
            case ScriptureBook.OT2_Job:
                return "./Scriptures/OT2/OT2_Job.html";
            case ScriptureBook.OT2_Nehemiah:
                return "./Scriptures/OT2/OT2_Nehemiah.html";
            case ScriptureBook.OT2_Proverbs:
                return "./Scriptures/OT2/OT2_Proverbs.html";
            case ScriptureBook.OT2_Psalms:
                return "./Scriptures/OT2/OT2_Psalms.html";
            case ScriptureBook.OT3_About:
                return "./Scriptures/OT3/OT3_About.html";
            case ScriptureBook.OT3_Amos:
                return "./Scriptures/OT3/OT3_Amos.html";
            case ScriptureBook.OT3_ChronologyKings:
                return "./Scriptures/OT3/OT3_ChronologyKings.html";
            case ScriptureBook.OT3_Daniel:
                return "./Scriptures/OT3/OT3_Daniel.html";
            case ScriptureBook.OT3_Ezekiel:
                return "./Scriptures/OT3/OT3_Ezekiel.html";
            case ScriptureBook.OT3_Habakkuk:
                return "./Scriptures/OT3/OT3_Habakkuk.html";
            case ScriptureBook.OT3_Haggai:
                return "./Scriptures/OT3/OT3_Haggai.html";
            case ScriptureBook.OT3_Hosea:
                return "./Scriptures/OT3/OT3_Hosea.html";
            case ScriptureBook.OT3_Isaiah:
                return "./Scriptures/OT3/OT3_Isaiah.html";
            case ScriptureBook.OT3_Jeremiah:
                return "./Scriptures/OT3/OT3_Jeremiah.html";
            case ScriptureBook.OT3_Joel:
                return "./Scriptures/OT3/OT3_Joel.html";
            case ScriptureBook.OT3_Jonah:
                return "./Scriptures/OT3/OT3_Jonah.html";
            case ScriptureBook.OT3_Lamentations:
                return "./Scriptures/OT3/OT3_Lamentations.html";
            case ScriptureBook.OT3_Malachi:
                return "./Scriptures/OT3/OT3_Malachi.html";
            case ScriptureBook.OT3_Micah:
                return "./Scriptures/OT3/OT3_Micah.html";
            case ScriptureBook.OT3_Nahum:
                return "./Scriptures/OT3/OT3_Nahum.html";
            case ScriptureBook.OT3_Obadiah:
                return "./Scriptures/OT3/OT3_Obadiah.html";
            case ScriptureBook.OT3_Zechariah:
                return "./Scriptures/OT3/OT3_Zechariah.html";
            case ScriptureBook.OT3_Zephaniah:
                return "./Scriptures/OT3/OT3_Zephaniah.html";
        }

        if (isUnknownBookError)
        {
            throw new Exception($"Invalid book: {book}");
        }

        return "";
    }
}
