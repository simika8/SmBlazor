﻿using System;
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

        //queryOptionsUrl.Orderby = "Rating desc, Name"
        var orders = SmSplit(queryOptionsUrl.Orderby);
        foreach (var orderbystr in orders)
        {
            var o = ParseOrderField(orderbystr);
            if (o != null)
                res.OrderFields.Add(o);
        }

        //queryOptionsUrl.Select = "Id, Name, Price, Rating"
        if (queryOptionsUrl.Select != null)
        {
            res.Select = new();
            var selects = SmSplit(queryOptionsUrl.Select);
            foreach (var select in selects)
            {
                res.Select.Add(select.ToLowerInvariant().Trim());
            }
        }

        return res;
    }

    public static SmQueryOptionsUrl Parse(SmQueryOptions queryOptions)
    {
        var res = new SmQueryOptionsUrl();

        res.Top = queryOptions.Top;
        res.Skip = queryOptions.Skip;
        res.Search = queryOptions.Search;

        if (queryOptions.OrderFields != null)
        {
            res.Orderby = string.Join(", ", queryOptions.OrderFields.Select(x => x.FieldName + (x.Descending ? " desc" : "")));
        } else
            res.Orderby = null;
        if (queryOptions.Select != null)
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
        urlParams["orderby"] = qou.Orderby;
        

        var url = string.Join("&", urlParams.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => $"{x.Key}={x.Value}"));

        if (!string.IsNullOrEmpty(url))
            url = "?" + url;
        return url;
    }
}

