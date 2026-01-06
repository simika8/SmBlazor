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
        SetGridHeight = 1,
        SetColumnWidth = 2,
        Extra = 8,
        ScrollBar8 = 16
    }
    public static class StyleSettingsHelper
    {
        public static StyleSettings GetDefaultStyleSettings()
        {
            return
            StyleSettings.None
            | StyleSettings.Extra
            | StyleSettings.SetGridHeight
            | StyleSettings.SetColumnWidth
            //| StyleSettings.ScrollBar8
            ;
        }

        public static string GetStyleClasses(this StyleSettings styleSettings)
        {
            var styleClassList = new List<string>();
            if (styleSettings.HasFlag(StyleSettings.Extra))
            {
                styleClassList.Add("ES");
            }
            if (styleSettings.HasFlag(StyleSettings.ScrollBar8))
            {
                styleClassList.Add("SB8"); //8 px width scrollbar
            } else
            {
                styleClassList.Add("SBT"); //thin scrollbar
            }
            var res = string.Join(" ", styleClassList);
            return res;
        }

    }
}
