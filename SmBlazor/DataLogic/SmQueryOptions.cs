using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmBlazor
{
    public class SmQueryOptions
    {
        public int? Top { get; set; }
        public int? Skip { get; set; }
        public string? Search { get; set; }
        public List<RowFilter>? Filters { get; set; }
        public List<OrderField>? OrderFields { get; set; }
        public List<string>? Select { get; set; }

    }
    public enum FilterType
    {
        Equals = 0,
        StartsWithCaseInsensitive = 1,
    }
    
    public class RowFilter
    {
        public string FieldName { get; set; } = null!;
        public string FilterValue { get; set; } = null!;
        public FilterType? FilterType { get; set; }
    }

    public class OrderField
    {
        public string FieldName { get; set; } = null!;
        public bool Descending { get; set; }
    }
    
    public static class SmQueryOptionsHelper
    {
        public static SmQueryOptions CreateSmQueryOptions(Settings settings, int? top, int? skip)
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

            res.Select = new List<string>();
            foreach (var column in settings.Columns.ColumnList)
            {
                res.Select.Add(column.FieldName);
            }



            return res;

        }

    }
}
