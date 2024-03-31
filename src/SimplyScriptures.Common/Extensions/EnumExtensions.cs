using System.Text;
using SimplyScriptures.Common.Enums;

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
        var alphaNumeric = str[(num + 1)..].SeparateAlphaNumeric().TrimToAlphaNumeric();
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

        return book.IsBookOfMormon() ? "BM" : book.IsDoctrineAndCovenants() ? "DC" : throw new Exception($"Invalid book: {book}");
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

        return book.IsBookOfMormon()
            ? ScriptureBook.BM
            : book.IsDoctrineAndCovenants() ? ScriptureBook.DC : throw new Exception($"Invalid book: {book}");
    }

    public static ScriptureBook ToSpecificBook(this ScriptureBook book)
    {
        return book switch
        {
            ScriptureBook.OT1 => ScriptureBook.OT1_About,
            ScriptureBook.OT2 => ScriptureBook.OT2_About,
            ScriptureBook.OT3 => ScriptureBook.OT3_About,
            ScriptureBook.NT => ScriptureBook.NT_About,
            ScriptureBook.BM => ScriptureBook.BM_About,
            ScriptureBook.DC => ScriptureBook.DC_About,
            _ => book,
        };
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

        return book.IsBookOfMormon()
            ? "./Scriptures/PDF/BM.pdf"
            : book.IsDoctrineAndCovenants() ? "./Scriptures/PDF/DC.pdf" : throw new Exception($"Invalid book: {book}");
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

        return book.IsBookOfMormon()
            ? "./Scriptures/BM/BM.json"
            : book.IsDoctrineAndCovenants() ? "./Scriptures/DC/DC.json" : throw new Exception($"Invalid book: {book}");
    }

    public static string ToHtmlPath(this ScriptureBook book, bool isUnknownBookError = true)
    {
        var text = book.ToString();

        if (text.StartsWith("DC_Lecture"))
        {
            return "./Scriptures/DC/DC_Lectures.html";
        }

        if (text.StartsWith("DC_Section"))
        {
            return "./Scriptures/DC/DC_Sections.html";
        }

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

        //if (book == ScriptureBook.NT) return "./Scriptures/NT/NT.html";
        return book switch
        {
            ScriptureBook.NT_1Corinthians => "./Scriptures/NT/NT_1Corinthians.html",
            ScriptureBook.NT_1John => "./Scriptures/NT/NT_1John.html",
            ScriptureBook.NT_1Peter => "./Scriptures/NT/NT_1Peter.html",
            ScriptureBook.NT_1Thessalonians => "./Scriptures/NT/NT_1Thessalonians.html",
            ScriptureBook.NT_1Timothy => "./Scriptures/NT/NT_1Timothy.html",
            ScriptureBook.NT_2Corinthians => "./Scriptures/NT/NT_2Corinthians.html",
            ScriptureBook.NT_2John => "./Scriptures/NT/NT_2John.html",
            ScriptureBook.NT_2Peter => "./Scriptures/NT/NT_2Peter.html",
            ScriptureBook.NT_2Thessalonians => "./Scriptures/NT/NT_2Thessalonians.html",
            ScriptureBook.NT_2Timothy => "./Scriptures/NT/NT_2Timothy.html",
            ScriptureBook.NT_3John => "./Scriptures/NT/NT_3John.html",
            ScriptureBook.NT_About => "./Scriptures/NT/NT_About.html",
            ScriptureBook.NT_Acts => "./Scriptures/NT/NT_Acts.html",
            ScriptureBook.NT_Colossians => "./Scriptures/NT/NT_Colossians.html",
            ScriptureBook.NT_Ephesians => "./Scriptures/NT/NT_Ephesians.html",
            ScriptureBook.NT_Galatians => "./Scriptures/NT/NT_Galatians.html",
            ScriptureBook.NT_Hebrews => "./Scriptures/NT/NT_Hebrews.html",
            ScriptureBook.NT_James => "./Scriptures/NT/NT_James.html",
            ScriptureBook.NT_John => "./Scriptures/NT/NT_John.html",
            ScriptureBook.NT_Jude => "./Scriptures/NT/NT_Jude.html",
            ScriptureBook.NT_Luke => "./Scriptures/NT/NT_Luke.html",
            ScriptureBook.NT_Mark => "./Scriptures/NT/NT_Mark.html",
            ScriptureBook.NT_Matthew => "./Scriptures/NT/NT_Matthew.html",
            ScriptureBook.NT_Philemon => "./Scriptures/NT/NT_Philemon.html",
            ScriptureBook.NT_Philippians => "./Scriptures/NT/NT_Philippians.html",
            ScriptureBook.NT_Revelation => "./Scriptures/NT/NT_Revelation.html",
            ScriptureBook.NT_Romans => "./Scriptures/NT/NT_Romans.html",
            ScriptureBook.NT_Titus => "./Scriptures/NT/NT_Titus.html",
            ScriptureBook.OT1_About => "./Scriptures/OT1/OT1_About.html",
            ScriptureBook.OT1_DescendantsTerah => "./Scriptures/OT1/OT1_DescendantsTerah.html",
            ScriptureBook.OT1_Deuteronomy => "./Scriptures/OT1/OT1_Deuteronomy.html",
            ScriptureBook.OT1_Exodus => "./Scriptures/OT1/OT1_Exodus.html",
            ScriptureBook.OT1_Genesis => "./Scriptures/OT1/OT1_Genesis.html",
            ScriptureBook.OT1_Joshua => "./Scriptures/OT1/OT1_Joshua.html",
            ScriptureBook.OT1_Judges => "./Scriptures/OT1/OT1_Judges.html",
            ScriptureBook.OT1_Leviticus => "./Scriptures/OT1/OT1_Leviticus.html",
            ScriptureBook.OT1_MasoreticTimeline => "./Scriptures/OT1/OT1_MasoreticTimeline.html",
            ScriptureBook.OT1_Numbers => "./Scriptures/OT1/OT1_Numbers.html",
            ScriptureBook.OT1_Ruth => "./Scriptures/OT1/OT1_Ruth.html",
            ScriptureBook.OT1_SeptuagintTimeline => "./Scriptures/OT1/OT1_SeptuagintTimeline.html",
            ScriptureBook.OT2_1Chronicles => "./Scriptures/OT2/OT2_1Chronicles.html",
            ScriptureBook.OT2_1Kings => "./Scriptures/OT2/OT2_1Kings.html",
            ScriptureBook.OT2_1Samuel => "./Scriptures/OT2/OT2_1Samuel.html",
            ScriptureBook.OT2_2Chronicles => "./Scriptures/OT2/OT2_2Chronicles.html",
            ScriptureBook.OT2_2Kings => "./Scriptures/OT2/OT2_2Kings.html",
            ScriptureBook.OT2_2Samuel => "./Scriptures/OT2/OT2_2Samuel.html",
            ScriptureBook.OT2_About => "./Scriptures/OT2/OT2_About.html",
            ScriptureBook.OT2_ChronologyKings => "./Scriptures/OT2/OT2_ChronologyKings.html",
            ScriptureBook.OT2_Ecclesiastes => "./Scriptures/OT2/OT2_Ecclesiastes.html",
            ScriptureBook.OT2_Esther => "./Scriptures/OT2/OT2_Esther.html",
            ScriptureBook.OT2_Ezra => "./Scriptures/OT2/OT2_Ezra.html",
            ScriptureBook.OT2_Job => "./Scriptures/OT2/OT2_Job.html",
            ScriptureBook.OT2_Nehemiah => "./Scriptures/OT2/OT2_Nehemiah.html",
            ScriptureBook.OT2_Proverbs => "./Scriptures/OT2/OT2_Proverbs.html",
            ScriptureBook.OT2_Psalms => "./Scriptures/OT2/OT2_Psalms.html",
            ScriptureBook.OT3_About => "./Scriptures/OT3/OT3_About.html",
            ScriptureBook.OT3_Amos => "./Scriptures/OT3/OT3_Amos.html",
            ScriptureBook.OT3_ChronologyKings => "./Scriptures/OT3/OT3_ChronologyKings.html",
            ScriptureBook.OT3_Daniel => "./Scriptures/OT3/OT3_Daniel.html",
            ScriptureBook.OT3_Ezekiel => "./Scriptures/OT3/OT3_Ezekiel.html",
            ScriptureBook.OT3_Habakkuk => "./Scriptures/OT3/OT3_Habakkuk.html",
            ScriptureBook.OT3_Haggai => "./Scriptures/OT3/OT3_Haggai.html",
            ScriptureBook.OT3_Hosea => "./Scriptures/OT3/OT3_Hosea.html",
            ScriptureBook.OT3_Isaiah => "./Scriptures/OT3/OT3_Isaiah.html",
            ScriptureBook.OT3_Jeremiah => "./Scriptures/OT3/OT3_Jeremiah.html",
            ScriptureBook.OT3_Joel => "./Scriptures/OT3/OT3_Joel.html",
            ScriptureBook.OT3_Jonah => "./Scriptures/OT3/OT3_Jonah.html",
            ScriptureBook.OT3_Lamentations => "./Scriptures/OT3/OT3_Lamentations.html",
            ScriptureBook.OT3_Malachi => "./Scriptures/OT3/OT3_Malachi.html",
            ScriptureBook.OT3_Micah => "./Scriptures/OT3/OT3_Micah.html",
            ScriptureBook.OT3_Nahum => "./Scriptures/OT3/OT3_Nahum.html",
            ScriptureBook.OT3_Obadiah => "./Scriptures/OT3/OT3_Obadiah.html",
            ScriptureBook.OT3_Zechariah => "./Scriptures/OT3/OT3_Zechariah.html",
            ScriptureBook.OT3_Zephaniah => "./Scriptures/OT3/OT3_Zephaniah.html",
            _ => isUnknownBookError ? throw new Exception($"Invalid book: {book}") : "",
        };
    }
}
