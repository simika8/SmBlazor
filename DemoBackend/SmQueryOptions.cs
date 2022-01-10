using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmData;

public enum FilterType
{
    Equals = 0,
    StartsWithCaseInsensitive = 1,
    Between = 2,
}

public class RowFilter
{
    public string FieldName { get; set; } = null!;
    public string FilterValue { get; set; } = null!;
    public string? FilterValue2 { get; set; }
    public FilterType? FilterType { get; set; }
}

public class OrderField
{
    public string FieldName { get; set; } = null!;
    public bool Descending { get; set; }
}
public class SmQueryOptions
{
    public int? Top { get; set; }
    public int? Skip { get; set; }
    public string? Search { get; set; }
    public List<RowFilter> Filters { get; set; } = new List<RowFilter>();
    public List<OrderField> OrderFields { get; set; } = new List<OrderField>();
    public List<string>? Select { get; set; } = new List<string>();

    public IQueryable<T> Apply<T>(IQueryable<T> sourceQuery)
    {
        var res = ApplyFilters(sourceQuery);
        return res;
    }

    private IQueryable<T> ApplyFilters<T>(IQueryable<T> sourceQuery)
    {

        // Combine all the resultant expression nodes using ||
        Type elementType = typeof(T);
        ParameterExpression parameterExpression = Expression.Parameter(elementType);
        //parameterExpression
        List<Expression> expressions = new List<Expression>();
        foreach (var filter in Filters)
        {
            if (filter.FilterType == FilterType.StartsWithCaseInsensitive)
            {
                var expression = StartsWithCaseInsensitiveExpression<T>(parameterExpression, filter.FieldName, filter.FilterValue);
                expressions.Add(expression);
            }
        }

        Expression aggregatedExpression = expressions.Aggregate(
                    (prev, current) => Expression.And(prev, current)
                );

        //Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(aggregatedExpression, parameterExpression);
        Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(expressions.First(), parameterExpression);

        var res = sourceQuery.Where(lambda);
        return res;
    }

    public Expression? StartsWithCaseInsensitiveExpression<T>(ParameterExpression parameterExpression, string propertyName, string constant)
    {
        MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string), typeof(bool), typeof(CultureInfo) })!;

        PropertyInfo? propertyInfo =
            typeof(T).GetProperties()
                .Where(x => x.PropertyType == typeof(string))
                .Where(x => x.Name == propertyName)
                .FirstOrDefault();
        if (propertyInfo == null) return null;

        var res = Expression.Call(
            Expression.Property(parameterExpression, propertyInfo)
            , startsWithMethod
            , Expression.Constant(constant)
            , Expression.Constant(true)
            , Expression.Constant(CultureInfo.InvariantCulture)

            );

        return res;
    }


}

[ModelBinder(BinderType = typeof(SmData.SmQueryOptionsUrlBinder))]
public class SmQueryOptionsUrl
{
    public int? Top { get; set; } = 1;
    public int? Skip { get; set; } = 0;
    public string? Search { get; set; }
    public string? Filter { get; set; }
    public string? Orderby { get; set; } = "Price desc, Name";
    public string? Select { get; set; } = "Id, Name, Price";

    /// <summary>
    /// : "Name sw 'Product, with spec chars ('',&?) in it''s name, asdf.', Id eq 3, Price between 12.2 and 323.2"
    /// -> 
    /// "Name sw 'Product, with spec chars ('',&?) in it''s name, asdf.'"
    /// " Id eq 3"
    /// " Price between 12.2 and 323.2"
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static List<string> SmSplit(string? data)
    {
        var res1 = data.Split(',').ToList();
        var res2 = new List<string>();
        if (data == null)
            return res2;

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
            if (count % 2 != 0)
            {
                //összevonni
                
            } else
            {
                res2.Add(currentLine.Trim());
                currentLine = "";
            }
        }

        return res2.ToList();

    }
    private static RowFilter? Parse(string data)
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
            if (value.IndexOf('\'') == 0)
            {
                value = value.Trim('\'');
                value = value.Replace("''", "'");
            }
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
    public static SmQueryOptions Convert2SmQueryOptions(SmQueryOptionsUrl queryOptionsUrl)
    {
        var res = new SmQueryOptions();

        res.Top = queryOptionsUrl.Top;
        res.Skip = queryOptionsUrl.Skip;
        res.Search = queryOptionsUrl.Search;

        //queryOptionsUrlstring data = "Name startswith 'Product, with spec chars ('',&?) in it''s name, asdf.', Id eq 3, Price between 12.2 and 323.2";
        var filters = SmSplit(queryOptionsUrl.Filter);
        foreach (var filterstr in filters)
        {
            var rf = Parse(filterstr);
            if (rf != null)
                res.Filters.Add(rf);
        }
        ;


        return res;
    }


}


public class SmQueryOptionsUrlBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        SmQueryOptionsUrl queryOptionsUrl = new SmQueryOptionsUrl();
        queryOptionsUrl.Top = Convert.ToInt32(bindingContext.ValueProvider.GetValue("Top").FirstOrDefault());
        queryOptionsUrl.Skip = Convert.ToInt32(bindingContext.ValueProvider.GetValue("Skip").FirstOrDefault());
        queryOptionsUrl.Search = bindingContext.ValueProvider.GetValue("Search").FirstOrDefault();
        queryOptionsUrl.Filter = bindingContext.ValueProvider.GetValue("Filter").FirstOrDefault();
        queryOptionsUrl.Orderby = bindingContext.ValueProvider.GetValue("Orderby").FirstOrDefault();
        queryOptionsUrl.Select = bindingContext.ValueProvider.GetValue("Select").FirstOrDefault();

        bindingContext.Result = ModelBindingResult.Success(queryOptionsUrl);
        return Task.FromResult(result: true);
    }
}
