using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmBlazor
{
    [Flags] public enum StyleSettings
    {
        None = 0,
        GridHeight = 1,
        ColumnWidth = 2,
        ColumnAlign = 4,
        Basic = 8,
        Extra = 16,
        BasicScrollBar = 32,
        ExtraScrollBar = 64,
    }
    public static class StyleSettingsHelper
    {
        public static StyleSettings GetDefaultStyleSettings()
        {
            return 
                StyleSettings.GridHeight
            | StyleSettings.ColumnWidth
            | StyleSettings.ColumnAlign
            | StyleSettings.Basic
            | StyleSettings.Extra
            | StyleSettings.ExtraScrollBar
            ;
        }

        public static string GetStyleClasses(this StyleSettings styleSettings)
        {
            var styleClassList = new List<string>();
            if (styleSettings.HasFlag(StyleSettings.Basic))
            {
                styleClassList.Add("BS");
            }
            if (styleSettings.HasFlag(StyleSettings.Extra))
            {
                styleClassList.Add("ES");
            }
            if (styleSettings.HasFlag(StyleSettings.BasicScrollBar))
            {
                styleClassList.Add("BSB");
            }
            if (styleSettings.HasFlag(StyleSettings.ExtraScrollBar))
            {
                styleClassList.Add("ESB");
            }
            var res = string.Join(" ", styleClassList);
            return res;
        }

    }
}
