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
    public int? Top { get; set; }
    public int? Skip { get; set; }
    public string? Search { get; set; }
    public string? Select { get; set; }

    
    public static SmQueryOptions Parse(int? top, int? skip, string? search, string? select)
    {
        var qou = new SmQueryOptions();

        qou.Top = top;
        qou.Skip = skip;
        qou.Search = search;


        //select example value: "Id, Name, Price, Rating"
        if (select != null)
        {
            qou.Select = new();
            var selectFields = select.Split(',', StringSplitOptions.TrimEntries);
            foreach (var selectField in selectFields)
            {
                qou.Select.Add(selectField.ToLowerInvariant().Trim());
            }
        }

        return qou;
    }
    private static Dictionary<string, string?> GetUrlParams(SmQueryOptions queryOptions)
    {
        var urlParams = new Dictionary<string, string?>();

        urlParams["top"] = queryOptions.Top?.ToString();
        urlParams["skip"] = queryOptions.Skip?.ToString();
        urlParams["search"] = queryOptions.Search;
        urlParams["select"] = (queryOptions.Select == null) ? null : string.Join(", ", queryOptions.Select); ;
        
        return urlParams;

    }
    public static string CalculateURL(SmQueryOptions queryOptions, Dictionary<string, string>? extraParams)
    {
        var urlParams = GetUrlParams(queryOptions);

        if (extraParams != null)
        {
            foreach (var extraParam in extraParams)
            {
                urlParams[extraParam.Key] = extraParam.Value;
            }
        }

        var url = string.Join("&", urlParams.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => $"{x.Key}={x.Value}"));

        if (!string.IsNullOrEmpty(url))
            url = "?" + url;
        return url;
    }
}

