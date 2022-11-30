using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmQueryOptionsNs;

public class SmQueryOptionsUrl
{
    [DefaultValue(10)]
    public int? Top { get; set; }
    public int? Skip { get; set; }
    [DefaultValue("prod")]
    public string? Search { get; set; }
    [DefaultValue("Name startswith 'Product, with spec chars:('', &?) in it''s name, asdf.', Rating eq 2, Code eq 'c0000001', Price between 123.4 and 1234.5")]
    public string? Filter { get; set; }
    [DefaultValue("Price desc, Name")]
    public string? Orderby { get; set; }
    [DefaultValue("Id, Code, Name, Price, Stocks, Rating")]
    public string? Select { get; set; }

    /// <summary>
    /// : Name sw 'Product, with spec chars ('',&?) in it''s name, asdf.', Id eq 3, Price between 12.2 and 323.2
    /// -> 
    /// Name sw 'Product, with spec chars ('',&?) in it''s name, asdf.'
    /// Id eq 3
    /// Price between 12.2 and 323.2
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static List<string> SmSplit(string? data)
    {
        var res2 = new List<string>();
        if (data == null)
            return res2;

        var res1 = data.Split(',').ToList();

        var currentLine = "";
        foreach (var line in res1)
        {
            if (currentLine == "")
            {
                currentLine = line;
            }
            else
            {
                currentLine = currentLine + "," + line;
            }

            var count = currentLine.Count(c => c == '\'');
            if (count % 2 == 0)
            {
                res2.Add(currentLine.Trim().Replace("''", "'", StringComparison.InvariantCulture));
                currentLine = "";
            }
        }

        return res2.ToList();

    }
    private static RowFilter? ParseRowFilter(string data)
    {
        var res = new RowFilter();
        var pos = data.IndexOf(' ');
        if (pos == -1)
            return null;
        var fieldName = data.Substring(0, pos);
        var pos2 = data.IndexOf(' ', pos + 1);
        var filterType = data.Substring(pos + 1, pos2 - pos).Trim();
        var value = data.Substring(pos2 + 1, data.Length - (pos2 + 1)).Trim();

        res.FieldName = fieldName;
        
        switch (filterType.ToLower())
        {
            case "startswith":
            case "sw":
                res.FilterType = FilterType.StartsWithCaseInsensitive;
                break;
            case "equals":
            case "eq":
                res.FilterType = FilterType.Equals;
                break;
            case "between":
            case "bw":
                res.FilterType = FilterType.Between;
                break;
            default:
                return null;
        }
        if (res.FilterType == FilterType.StartsWithCaseInsensitive || res.FilterType == FilterType.Equals)
        {
            if (value == "null")
                res.FilterValue = null!;
            else if (value.IndexOf('\'') == 0)
            {
                value = value.Trim('\'');
                value = value.Replace("''", "'");
                res.FilterValue = value;
            } else 
                res.FilterValue = value;
        }
        if (res.FilterType == FilterType.Between)
        {
            var values = value.Split(" and ");
            if (values.Count() != 2)
                return null;
            res.FilterValue = values[0];
            res.FilterValue2 = values[1];
        }

        return res;
    }
    private static OrderField? ParseOrderField(string data)
    {
        var res = new OrderField();
        var parts = data.Trim().Split(' ');
        if (parts.Count() == 0)
            return null;
        if (parts.Count() == 1)
        {
            res.FieldName = parts[0];
            res.Descending = false;
        }
        if (parts.Count() == 2)
        {
            res.FieldName = parts[0];
            res.Descending = (parts[1].ToLower() == "desc") || (parts[1].ToLower() == "descending");
        }
        return res;

    }

    public static SmQueryOptions Parse(SmQueryOptionsUrl queryOptionsUrl)
    {
        var res = new SmQueryOptions();

        res.Top = queryOptionsUrl.Top;
        res.Skip = queryOptionsUrl.Skip;
        res.Search = queryOptionsUrl.Search;

        //queryOptionsUrl.Filter = "Name startswith 'Product, with spec chars ('',&?) in it''s name, asdf.', Id eq 3, Price between 12.2 and 323.2";
        var filters = SmSplit(queryOptionsUrl.Filter);
        foreach (var filterstr in filters)
        {
            var rf = ParseRowFilter(filterstr);
            if (rf != null)
                res.Filters.Add(rf);
        }


        //queryOptionsUrl.Orderby = "Rating desc, Name"
        var orders = SmSplit(queryOptionsUrl.Orderby);
        foreach (var orderbystr in orders)
        {
            var o = ParseOrderField(orderbystr);
            if (o != null)
                res.OrderFields.Add(o);
        }

        //queryOptionsUrl.Select = "Id, Name, Price, Rating"
        var selects = SmSplit(queryOptionsUrl.Select);
        foreach (var select in selects)
        {
            res.Select.Add(select.ToLowerInvariant().Trim());
        }

        return res;
    }

    public static SmQueryOptionsUrl Parse(SmQueryOptions queryOptions)
    {
        var res = new SmQueryOptionsUrl();

        res.Top = queryOptions.Top;
        res.Skip = queryOptions.Skip;
        res.Search = queryOptions.Search;

        var stringfilters = new List<string>();
        //queryOptionsUrl.Filter = "Name startswith 'Product, with spec chars ('',&?) in it''s name, asdf.', Id eq 3, Price between 12.2 and 323.2";
        if (queryOptions.Filters != null)
        {
            foreach (var filter in queryOptions.Filters)
            {
                switch (filter.FilterType)
                {
                    case FilterType.Between:
                        stringfilters.Add($"{filter.FieldName} between {filter.FilterValue} and {filter.FilterValue2}");
                        break;
                    case FilterType.Equals:
                        stringfilters.Add($"{filter.FieldName} equals '{filter.FilterValue}'");
                        break;
                    case FilterType.StartsWithCaseInsensitive:
                        stringfilters.Add($"{filter.FieldName} startswith '{filter.FilterValue}'");
                        break;
                    default:
                        break;
                }

            }
        }
        res.Filter = string.Join(", ", stringfilters);

        if (queryOptions.OrderFields != null)
        {
            res.Orderby = string.Join(", ", queryOptions.OrderFields.Select(x => x.FieldName + (x.Descending ? " desc" : "")));
        } else
            res.Orderby = null;

        res.Select = string.Join(", ", queryOptions.Select);
        return res;
    }

    public static string CalculateURL(SmQueryOptions qo)
    {
        var qou = Parse(qo);

        var urlParams = new Dictionary<string, string?>();



        urlParams["search"] = qou.Search;
        urlParams["top"] = qou.Top?.ToString();
        urlParams["skip"] = qou.Skip?.ToString();
        urlParams["select"] = qou.Select;
        urlParams["filter"] = qou.Filter;
        urlParams["orderby"] = qou.Orderby;
        

        var url = string.Join("&", urlParams.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => $"{x.Key}={x.Value}"));

        if (!string.IsNullOrEmpty(url))
            url = "?" + url;
        return url;
    }
}

