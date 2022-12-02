using SmQueryOptionsNs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmBlazor
{
    public static class SmQueryOptionsHelper
    {
        public static SmQueryOptions CreateSmQueryOptions(SmGridSettings settings, int? top, int? skip)
        {
            var res = new SmQueryOptions();
            res.Top = top > 0 ? top : null;
            res.Skip = skip > 0 ? skip : null;
            res.Search = settings.Search;

            res.Select = new ();
            foreach (var column in settings.Columns)
            {
                res.Select.Add(column.FieldName);
            }



            return res;

        }

    }
}
