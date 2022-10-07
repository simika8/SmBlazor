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

namespace SmQueryOptions;

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
