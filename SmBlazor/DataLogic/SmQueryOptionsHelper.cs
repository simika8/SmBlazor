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

            if (settings.FilterValues != null)
            {
                res.Filters = new List<RowFilter>();
                foreach (var filtervalue in settings.FilterValues)
                {
                    var filterValueString = filtervalue.Value;
                    var column = settings.Columns.GetColumn(filtervalue.Key);
                    if (!string.IsNullOrEmpty(filterValueString) && column != null)
                    {
                        var rowfilter = new RowFilter()
                        {
                            FieldName = filtervalue.Key,
                            FilterValue = filterValueString,
                            FilterType = column.FilterType,
                        };
                        res.Filters.Add(rowfilter);
                    }
                }
                if (res.Filters.Count == 0)
                    res.Filters = null;
            }

            res.OrderFields = settings.Order.OrderFields;

            res.Select = new ();
            foreach (var column in settings.Columns)
            {
                res.Select.Add(column.FieldName);
            }



            return res;

        }

    }
}
