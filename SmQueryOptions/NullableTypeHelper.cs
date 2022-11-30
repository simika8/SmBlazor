using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmQueryOptionsNs;

public static class NullableTypeHelper
{
    public static T? ParseNullable<T>(string? value)
    {
        if (value == null || value.Trim() == string.Empty || value.Trim() == "null")
        {
            return default(T);
        }
        else
        {
            var ty = typeof(T);
            var parseMethod = ty.GetMethod("Parse", new[] { typeof(string), typeof(CultureInfo) });
            if (parseMethod == null)
            {
                var uty = Nullable.GetUnderlyingType(ty);
                if (uty != null)
                    parseMethod = uty.GetMethod("Parse", new[] { typeof(string), typeof(CultureInfo) });
            }
            try
            {
                dynamic? typedConsant = parseMethod.Invoke(ty, new object[] { value, CultureInfo.InvariantCulture });
                return typedConsant;
            }
            catch
            {
                return default(T);
            }
        }
    }

    public static Expression<Func<T, bool>> BuildWhereExpressionorig<T>(string nameValueQuery)// where T : class
    {
        Expression<Func<T, bool>> predicate = null;
        PropertyInfo prop = null;
        var fieldName = nameValueQuery.Split("=")[0];
        var fieldValue = nameValueQuery.Split("=")[1];
        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            if (property.Name.ToLower() == fieldName.ToLower())
            {
                prop = property;
            }
        }
        if (prop != null)
        {
            var isNullable = prop.PropertyType.IsNullableType();
            var parameter = Expression.Parameter(typeof(T), "x");
            var member = Expression.Property(parameter, fieldName);

            if (isNullable)
            {
                var filter1 =
                    Expression.Constant(
                        Convert.ChangeType(fieldValue, member.Type.GetGenericArguments()[0]));
                Expression typeFilter = Expression.Convert(filter1, member.Type);
                dynamic? body = Expression.Equal(member, typeFilter);
                return body;
                predicate = Expression.Lambda<Func<T, bool>>(body, parameter);
            }
            else
            {
                if (prop.PropertyType == typeof(string))
                {
                    var parameterExp = Expression.Parameter(typeof(T), "type");
                    var propertyExp = Expression.Property(parameterExp, prop);
                    MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    var someValue = Expression.Constant(fieldValue, typeof(string));
                    var containsMethodExp = Expression.Call(propertyExp, method, someValue);
                    predicate = Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
                }
                else
                {
                    var constant = Expression.Constant(Convert.ChangeType(fieldValue, prop.PropertyType));
                    var body = Expression.Equal(member, constant);
                    predicate = Expression.Lambda<Func<T, bool>>(body, parameter);
                }
            }
        }
        return predicate;
    }
    public static Expression? BuildWhereExpression<T>(string nameValueQuery)// where T : class
    {
        Expression<Func<T, bool>> predicate = null;
        PropertyInfo prop = null;
        var fieldName = nameValueQuery.Split("=")[0];
        var fieldValue = nameValueQuery.Split("=")[1];
        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            if (property.Name.ToLower() == fieldName.ToLower())
            {
                prop = property;
            }
        }
        if (prop != null)
        {
            var isNullable = prop.PropertyType.IsNullableType();
            var parameter = Expression.Parameter(typeof(T), "x");
            var member = Expression.Property(parameter, fieldName);

            if (isNullable)
            {
                var filter1 =
                    Expression.Constant(
                        Convert.ChangeType(fieldValue, member.Type.GetGenericArguments()[0]));
                Expression typeFilter = Expression.Convert(filter1, member.Type);
                var body = Expression.Equal(member, typeFilter);
                return body;
                predicate = Expression.Lambda<Func<T, bool>>(body, parameter);
            }
            else
            {
                if (prop.PropertyType == typeof(string))
                {
                    var parameterExp = Expression.Parameter(typeof(T), "type");
                    var propertyExp = Expression.Property(parameterExp, prop);
                    MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    var someValue = Expression.Constant(fieldValue, typeof(string));
                    var containsMethodExp = Expression.Call(propertyExp, method, someValue);
                    predicate = Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
                }
                else
                {
                    var constant = Expression.Constant(Convert.ChangeType(fieldValue, prop.PropertyType));
                    var body = Expression.Equal(member, constant);
                    predicate = Expression.Lambda<Func<T, bool>>(body, parameter);
                }
            }
        }
        return predicate;
    }
    public static bool IsNullableType(this Type type)
    {
        return type.IsGenericType && (type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
    }

    public static dynamic? GetExpressionConstant(Type ty, string constant)
    {

        dynamic? typedConsant;
        if (constant == null)
            typedConsant = null;
        else
        {
            var parseMethod = ty.GetMethod("Parse", new[] { typeof(string), typeof(CultureInfo) });
            if (parseMethod == null)
            {
                var uty = Nullable.GetUnderlyingType(ty);
                if (uty != null)
                    parseMethod = uty.GetMethod("Parse", new[] { typeof(string), typeof(CultureInfo) });
            }
            if (parseMethod == null)
                throw new NotSupportedException("Not supported data type");
            typedConsant = parseMethod.Invoke(ty, new object[] { constant, CultureInfo.InvariantCulture });
        }

        var res = Expression.Constant(typedConsant, ty);
        return res;
    }
    public static bool StringIsNull(string? s)
    {
        return s == null;
    }
    public static bool StringEquals(string? s1, string? s2)
    {
        if (s1 == null && s2 == null)
            return true;
        else if (s1 == null && s2 != null)
            return false;
        else if (s1 != null && s2 == null)
            return false;
        else 
            return s1 == s2;
    }
}
