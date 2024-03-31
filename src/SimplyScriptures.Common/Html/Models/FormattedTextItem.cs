using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplyScriptures.Common.Html.Models;

public class FormattedTextItem
{
    public string Text { get; set; } = "";
    public int Index { get; set; } = 0;
    public int IndentationLevel { get; set; } = 0;
    public bool IsItalic { get; set; }
    public bool IsBold { get; set; }
}
