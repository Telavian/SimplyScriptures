using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplyScriptures.TemplateSelectors;

public class HighlightColorTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ColorTemplate { get; set; }
    public DataTemplate? TransparentTemplate { get; set; }

    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
    {
        return item switch
        {
            Color c when c.Equals(Colors.Transparent) => TransparentTemplate,
            _ => ColorTemplate,
        };
    }
}
