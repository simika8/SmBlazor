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
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace SmQueryOptionsNs;

public delegate TDto ProjectResultItemDelegate<T, TDto>(T x, SmQueryOptions? smQueryOptions);

public static class SmQueryOptionsHelper
{
    public static IEnumerable<T> ApplySkipTop<T>(this IEnumerable<T> query, SmQueryOptions smQueryOptions)
    {
        if (smQueryOptions.Skip > 0)
            query = query.Skip(smQueryOptions.Skip ?? 0);
        if (smQueryOptions.Top > 0)
            query = query.Take(smQueryOptions.Top ?? 1);
        return query;
    }

    public static IQueryable<T> ApplySkipTop<T>(this IQueryable<T> query, SmQueryOptions smQueryOptions)
    {
        if (smQueryOptions.Skip > 0)
            query = query.Skip(smQueryOptions.Skip ?? 0);
        if (smQueryOptions.Top > 0)
            query = query.Take(smQueryOptions.Top ?? 1);
        return query;
    }

    public static IFindFluent<T, T> ApplySkipTop<T>(this IFindFluent<T, T> query, SmQueryOptions smQueryOptions)
    {
        if (smQueryOptions.Skip > 0)
            query = query.Skip(smQueryOptions.Skip ?? 0);
        if (smQueryOptions.Top > 0)
            query = query.Limit(smQueryOptions.Top ?? 1);
        return query;
    }

    public static async Task<List<T>> RunQuery<T>(this IEnumerable<T> query)
    {
        var queryResult = query.ToList();
        return queryResult;
    }

    public static async Task<List<T>> RunQuery<T>(this IQueryable<T> query)
    {
        var queryResult = await query.ToListAsync();
        return queryResult;
    }

    public static async Task<List<T>> RunQuery<T>(this IFindFluent<T, T> query)
    {
        var queryResult = await query.ToListAsync();
        return queryResult;
    }




    public static async Task<IEnumerable<TDto>> ApplySkipTopDict<T, TDto>(this SmQueryOptions smQueryOptions, IEnumerable<T> query, ProjectResultItemDelegate<T, TDto> projectResultItemDelegate)
    {
        ;
        if (smQueryOptions.Skip > 0)
            query = query.Skip(smQueryOptions.Skip ?? 0);
        if (smQueryOptions.Top > 0)
            query = query.Take(smQueryOptions.Top ?? 1);
        var queryResult = query.ToList();
        var res = queryResult.Select(x => projectResultItemDelegate(x, smQueryOptions));
        return res;
    }

    public static async Task<IEnumerable<TDto>> ApplySkipTopEf<T, TDto>(this SmQueryOptions smQueryOptions, IQueryable<T> query, ProjectResultItemDelegate<T, TDto> projectResultItemDelegate)
    {
        ;
        if (smQueryOptions.Skip > 0)
            query = query.Skip(smQueryOptions.Skip ?? 0);
        if (smQueryOptions.Top > 0)
            query = query.Take(smQueryOptions.Top ?? 1);
        var queryResult = await query.ToListAsync();
        var res = queryResult.Select(x => projectResultItemDelegate(x, smQueryOptions));
        return res;
    }

    public static async Task<IEnumerable<TDto>> ApplySkipTopMongo<T, TDto>(this SmQueryOptions smQueryOptions, IFindFluent<T, T> query, ProjectResultItemDelegate<T, TDto> projectResultItemDelegate)
    {
        ;
        if (smQueryOptions.Skip > 0)
            query = query.Skip(smQueryOptions.Skip ?? 0);
        if (smQueryOptions.Top > 0)
            query = query.Limit(smQueryOptions.Top ?? 1);
        var queryResult = await query.ToListAsync();
        var res = queryResult.Select(x => projectResultItemDelegate(x, smQueryOptions));
        return res;
    }


}

