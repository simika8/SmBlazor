using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    public List<string> Select { get; set; } = new List<string>();

    public IQueryable<T> Apply<T>(IQueryable<T> sourceQuery)
    {
        var res = sourceQuery;
        res = ApplyFilters(sourceQuery);
        res = ApplyOrders(res);
        res = ApplyPaging(res);
        res = ApplySelect(res);


        return res;
    }

    private IQueryable<T> ApplyFilters<T>(IQueryable<T> sourceQuery)
    {
        if (!Filters.Any())
            return sourceQuery;
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
            if (filter.FilterType == FilterType.Equals)
            {
                var expression = Equals<T>(parameterExpression, filter.FieldName, filter.FilterValue);
                expressions.Add(expression);
            }
            if (filter.FilterType == FilterType.Between)
            {
                var expression = Between<T>(parameterExpression, filter.FieldName, filter.FilterValue, filter.FilterValue2);
                expressions.Add(expression);
            }
        }

        Expression aggregatedExpression = expressions.Aggregate(
                    (prev, current) => Expression.And(prev, current)
                );

        Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(aggregatedExpression, parameterExpression);

        var res = sourceQuery.Where(lambda);
        return res;
    }

    private Expression? DefaultSearchExpression<T>(ParameterExpression parameterExpression, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return null;

        const string defaultSearchPropertyName = "Name";

        PropertyInfo? propertyInfo =
            typeof(T).GetProperties()
                .Where(x => x.PropertyType == typeof(string))
                .Where(x => x.Name == defaultSearchPropertyName)
                .FirstOrDefault();
        if (propertyInfo == null) 
            return null;

        var expression = StartsWithCaseInsensitiveExpression<T>(parameterExpression, defaultSearchPropertyName, search);

        return expression;
    }

    public IQueryable<T> ApplySearch<T>(ParameterExpression parameterExpression, IQueryable<T> sourceQuery, Expression searchExpression)
    {
        if (string.IsNullOrWhiteSpace(Search))
            return sourceQuery;

        Expression expression;
        if (searchExpression != null)
            expression = searchExpression;
        else
            expression = DefaultSearchExpression<T>(parameterExpression, Search);
        if (expression == null)
            return sourceQuery;

        //var expression = DefaultSearchExpression<T>(parameterExpression, Search);

        Expression <Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(expression, parameterExpression);

        var res = sourceQuery.Where(lambda);
        return res;
    }
    private IQueryable<T> ApplyOrders<T>(IQueryable<T> sourceQuery)
    {
        if (!OrderFields.Any())
            return sourceQuery;
        IOrderedQueryable<T> res;
        if (!OrderFields[0].Descending)
            res = ApplyOrder<T>(sourceQuery, OrderFields[0].FieldName, "OrderBy");
        else
            res = ApplyOrder<T>(sourceQuery, OrderFields[0].FieldName, "OrderByDescending");
        for (int i = 1; i < OrderFields.Count; i++)
        {
            var orderField = OrderFields[i];
            if (!orderField.Descending)
                res = ApplyOrder<T>(res, orderField.FieldName, "ThenBy");
            else
                res = ApplyOrder<T>(res, orderField.FieldName, "ThenByDescending");
        }
        return res;
    }
    static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
    {
        string[] props = property.Split('.');
        Type type = typeof(T);
        ParameterExpression arg = Expression.Parameter(type, "x");
        Expression expr = arg;
        foreach (string prop in props)
        {
            PropertyInfo pi = type.GetProperty(prop)!;
            expr = Expression.Property(expr, pi);
            type = pi.PropertyType;
        }
        Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
        LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

        object result = typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                        && method.IsGenericMethodDefinition
                        && method.GetGenericArguments().Length == 2
                        && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] { source, lambda })!;
        return (IOrderedQueryable<T>)result;
    }
    public static PropertyInfo? GetPropertyInfo<T>(string propertyName)
    {
        PropertyInfo? propertyInfo =
            typeof(T).GetProperties()
                .Where(x => x.Name == propertyName)
                .FirstOrDefault();
        return propertyInfo;
    }
    public static Expression? StartsWithCaseInsensitiveExpression<T>(ParameterExpression parameterExpression, string propertyName, string constant)
    {
        MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string), typeof(bool), typeof(CultureInfo) })!;

        PropertyInfo? propertyInfo = GetPropertyInfo<T>(propertyName);

        if (propertyInfo == null) 
            return null;
        if (propertyInfo.PropertyType != typeof(string)) 
            return null;

        var res = Expression.Call(
            Expression.Property(parameterExpression, propertyInfo)
            , startsWithMethod
            , Expression.Constant(constant)
            , Expression.Constant(true)
            , Expression.Constant(CultureInfo.InvariantCulture)

            );

        return res;
    }

    public static Expression? ContainsCaseInsensitiveExpression<T>(ParameterExpression parameterExpression, string propertyName, string constant)
    {
        //var a = "".Contains("sadf", StringComparison.InvariantCultureIgnoreCase);
        MethodInfo containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) })!;

        PropertyInfo? propertyInfo = GetPropertyInfo<T>(propertyName);

        if (propertyInfo == null)
            return null;
        if (propertyInfo.PropertyType != typeof(string))
            return null;

        var res = Expression.Call(
            Expression.Property(parameterExpression, propertyInfo)
            , containsMethod
            , Expression.Constant(constant)
            , Expression.Constant(StringComparison.InvariantCultureIgnoreCase)

            );

        return res;
    }

    public Expression? Equals<T>(ParameterExpression parameterExpression, string propertyName, string constant)
    {
        PropertyInfo? propertyInfo = GetPropertyInfo<T>(propertyName);
        if (propertyInfo == null) 
            return null;

        var ty = propertyInfo.PropertyType;



        if (ty == typeof(string))
        {
            var equalsMethod = typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(StringComparison) });
            if (equalsMethod == null)
                throw new NotSupportedException("Not supported data type");
            var res = Expression.Call(
                Expression.Property(parameterExpression, propertyInfo)
                , equalsMethod
                , Expression.Constant(constant)
                , Expression.Constant(StringComparison.InvariantCultureIgnoreCase)
                );
            return res;
        }
        else
        {
            var equalsMethod = ty.GetMethod("Equals", new[] { ty });
            var parseMethod = ty.GetMethod("Parse", new[] { typeof(string), typeof(CultureInfo) });
            if (parseMethod == null)
                throw new NotSupportedException("Not supported data type");
            dynamic? typedConsant = parseMethod.Invoke(ty, new object[] { constant, CultureInfo.InvariantCulture });
            var res = Expression.Call(
                Expression.Property(parameterExpression, propertyInfo)
                , equalsMethod
                , Expression.Constant(typedConsant)
                );
            return res;

        }
    }
    public Expression? Between<T>(ParameterExpression parameterExpression, string propertyName, string constant1, string? constant2)
    {
        PropertyInfo? propertyInfo = GetPropertyInfo<T>(propertyName);
        if (propertyInfo == null)
            return null;

        var ty = propertyInfo.PropertyType;



        if (ty == typeof(string))
            throw new NotSupportedException("Not supported data type");

        var parseMethod = ty.GetMethod("Parse", new[] { typeof(string), typeof(CultureInfo) });
        if (parseMethod == null)
            throw new NotSupportedException("Not supported data type");

        var cult = CultureInfo.InvariantCulture;
        dynamic? typedConsant1 = parseMethod.Invoke(ty, new object[] { constant1, CultureInfo.InvariantCulture });
        dynamic? typedConsant2 = parseMethod.Invoke(ty, new object[] { constant2??"", CultureInfo.InvariantCulture });

        var res = Expression.And(
            Expression.GreaterThanOrEqual(
                Expression.Property(parameterExpression, propertyInfo), 
                Expression.Constant(typedConsant1)
                ),
            Expression.LessThanOrEqual(
                Expression.Property(parameterExpression, propertyInfo), 
                Expression.Constant(typedConsant2)
                )
            );

        return res;
    }

    private IQueryable<T> ApplySelect<T>(IQueryable<T> sourceQuery)
    {
        var selectfields = string.Join(",", Select);
        var selectExpression = SelectExpression<T>(selectfields);
        var res = sourceQuery.Select(selectExpression);
        return res;
    }
    public static Func<T, T> DynamicSelectGeneratorCompiled<T>(string Fields = "")
    {
        var lambda = SelectExpression<T>(Fields);
        return lambda.Compile();
    }
    public static Expression<Func<T, T>> SelectExpression<T>(string Fields = "")
    {
        string[] EntityFields;
        if (Fields == "")
            // get Properties of the T
            EntityFields = typeof(T).GetProperties().Select(propertyInfo => propertyInfo.Name).ToArray();
        else
            EntityFields = Fields.Split(',');

        // input parameter "o"
        var xParameter = Expression.Parameter(typeof(T), "o");

        // new statement "new Data()"
        var xNew = Expression.New(typeof(T));

        // create initializers
        var bindings = EntityFields.Select(o => o.Trim())
            .Select(o =>
            {

                    // property "Field1"
                    var mi = typeof(T).GetProperty(o);

                    // original value "o.Field1"
                    var xOriginal = Expression.Property(xParameter, mi!);

                    // set value "Field1 = o.Field1"
                    return Expression.Bind(mi!, xOriginal);
            }
        );

        // initialization "new Data { Field1 = o.Field1, Field2 = o.Field2 }"
        var xInit = Expression.MemberInit(xNew, bindings);

        // expression "o => new Data { Field1 = o.Field1, Field2 = o.Field2 }"
        var lambda = Expression.Lambda<Func<T, T>>(xInit, xParameter);

        // compile to Func<Data, Data>
        return lambda;
    }

    private IQueryable<T> ApplyPaging<T>(IQueryable<T> sourceQuery)
    {
        var res = sourceQuery;
        if (Skip > 0)
            res = res.Skip(Skip??0);
        if (Top > 0)
            res = res.Take(Top??1);
                
        return res;
    }

}

[ModelBinder(BinderType = typeof(SmData.SmQueryOptionsUrlBinder))]
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
            res.Select.Add(select.Trim());
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
        res.Filter = string.Join(", ", stringfilters);

        res.Orderby = string.Join(", ", queryOptions.OrderFields.Select(x => x.FieldName + (x.Descending?" desc":"")));

        res.Select = string.Join(", ", queryOptions.Select);
        //queryOptionsUrl.Orderby = "Rating desc, Name"
        /*var orders = SmSplit(queryOptions.Orderby);
        foreach (var orderbystr in orders)
        {
            var o = ParseOrderField(orderbystr);
            if (o != null)
                res.OrderFields.Add(o);
        }

        //queryOptionsUrl.Select = "Id, Name, Price, Rating"
        var selects = SmSplit(queryOptions.Select);
        foreach (var select in selects)
        {
            res.Select.Add(select.Trim());
        }*/

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
