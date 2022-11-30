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
    public List<OrderField> OrderFields { get; set; } = new List<OrderField>();
    public HashSet<string>? Select { get; set; }

    public IQueryable<T> Apply<T>(IQueryable<T> sourceQuery)
    {
        var res = sourceQuery;
        res = ApplyOrders(res);

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
            PropertyInfo pi = GetPropertyInfo<T>(prop)!;
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
                .Where(x => x.Name.ToLowerInvariant() == propertyName.ToLowerInvariant())
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

}
