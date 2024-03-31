using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplyScriptures.Common.Enums;

namespace SimplyScriptures.Common.Models;

public class ShareItem
{
    public string? ImageLink { get; set; } = null;
    public ScriptureBook Book { get; set; } = ScriptureBook.None;
    public string Verse { get; set; } = "";
}
