using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimplyScriptures.Common.Enums;

namespace SimplyScriptures.Console.Models;

public class BookPositionInfo
{
    public ScriptureBook Book { get; set; } = ScriptureBook.None;
    public int Chapter { get; set; } = 0;
    public int Verse { get; set; } = 0;
}
