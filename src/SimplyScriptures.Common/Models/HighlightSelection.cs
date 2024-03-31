using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplyScriptures.Common.Enums;

namespace SimplyScriptures.Common.Models;

public class HighlightSelection
{
    public ScriptureBook Book { get; set; } = ScriptureBook.None;
    public string Color { get; set; } = "";
    public string[] XPath { get; set; } = [];
}
