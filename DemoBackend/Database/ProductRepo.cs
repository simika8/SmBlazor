using Controllers;
using DemoModels;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using SmQueryOptionsNs;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static Controllers.AdminController;

namespace Database;

public class ProductRepo
{


    public static async Task<IEnumerable<DemoModels.ProductSearchResult>> Search(SmQueryOptions smQueryOptions, bool onlyStocks = false)
    {
        IEnumerable<DemoModels.ProductSearchResult> res;
        switch (CrudRepo<Product, Guid>.DbType)
        {
            case AdminController.DatabaseType.Dictionary:
                res = await ProductRepo.SearchDict(smQueryOptions, onlyStocks);
                break;
            case AdminController.DatabaseType.EfPg:
                res = await ProductRepo.SearchEf(smQueryOptions, onlyStocks);
                break;
            case AdminController.DatabaseType.Mongo:
                res = await ProductRepo.SearchMongo(smQueryOptions, onlyStocks);
                break;
            default:
                res = await ProductRepo.SearchDict(smQueryOptions, onlyStocks);
                break;
        }
        return res;
    }


    public static async Task<IEnumerable<DemoModels.ProductSearchResult>> SearchDict(SmQueryOptions smQueryOptions, bool onlyStocks = false)
    {
        var table = Database.DictionaryDatabase.Products;
        var query = GetBaseQueryDict(table, smQueryOptions, onlyStocks);

        query = query.ApplySkipTop(smQueryOptions);
        var queryResult = await query.RunQuery();
        var res = queryResult.Select(x => ProjectResultItem(x, smQueryOptions));

        return res;
    }

    public static async Task<IEnumerable<DemoModels.ProductSearchResult>> SearchEf(SmQueryOptions smQueryOptions, bool onlyStocks = false)
    {
        using var db = new Database.SmDemoProductContext();
        var table = db.Product;
        var query = GetBaseQueryEf(table, smQueryOptions, onlyStocks);

        query = query.ApplySkipTop(smQueryOptions);
        var queryResult = await query.RunQuery();
        var res = queryResult.Select(x => ProjectResultItem(x, smQueryOptions));

        return res;
    }

    public static async Task<IEnumerable<DemoModels.ProductSearchResult>> SearchMongo(SmQueryOptions smQueryOptions, bool onlyStocks = false)
    {
        var table = Database.SmDemoProductMongoDatabase.Product;
        var query = GetBaseQueryMongo(table, smQueryOptions, onlyStocks);

        query = query.ApplySkipTop(smQueryOptions);
        var queryResult = await query.RunQuery();
        var res = queryResult.Select(x => ProjectResultItem(x, smQueryOptions));

        return res;
    }


    public static DemoModels.ProductSearchResult ProjectResultItem(DemoModels.Product x, SmQueryOptions smQueryOptions)
    {
        var res = new DemoModels.ProductSearchResult();
        SmQueryOptionsNs.Mapper.CopyProperties(x, res, false, false, smQueryOptions.Select);
        if (smQueryOptions.Select?.Contains("StockSumQuantity".ToLower()) ?? true)
            res.StockSumQuantity = (x.Stocks?.Count() > 0) ? x.Stocks?.Sum(x => x.Quantity) : null;
        if (smQueryOptions.Select?.Contains("Description".ToLower()) ?? true)
            res.Description = x.Ext?.Description;
        return res;
    }

    public static IEnumerable<DemoModels.Product> GetBaseQueryDict(Dictionary<Guid, DemoModels.Product> table, SmQueryOptions smQueryOptions, bool OnlyStocks)
    {
        var baseQuery = table.Select(x => x.Value);

        #region apply OnlyStocks
        if (OnlyStocks)
            baseQuery = baseQuery.Where(x => x.Stocks != null/* && x.Stocks.Sum(x => x.Quantity) > 0*/);
        #endregion

        #region apply search
        if (smQueryOptions.Search == null)
            baseQuery = baseQuery.Where(x => true);
        else
            baseQuery = baseQuery.Where(x =>
                (x.Name != null && x.Name.ToLower().Contains(smQueryOptions.Search.ToLower()))
                ||
                (x.Code != null && x.Code.ToLower().StartsWith(smQueryOptions.Search.ToLower()))

            );
        #endregion
        var query = baseQuery;

        return query;

    }

    public static IQueryable<DemoModels.Product> GetBaseQueryEf(DbSet<DemoModels.Product> table, SmQueryOptions smQueryOptions, bool OnlyStocks)
    {
        var baseQuery = table
            /*.Include(x => x.Stocks)
            .Include(x => x.Ext)*/
            .OrderBy(x => x.Id)
            .AsQueryable();

        #region apply OnlyStocks
        if (OnlyStocks)
            baseQuery = baseQuery.Where(x => x.Stocks != null && x.Stocks.Sum(x => x.Quantity) > 0);
        #endregion

        #region apply search
        if (smQueryOptions.Search == null)
            baseQuery = baseQuery.Where(x => true);
        else
            baseQuery = baseQuery.Where(x =>
                (x.Name != null && x.Name.ToLower().Contains(smQueryOptions.Search.ToLower()))
                ||
                (x.Code != null && x.Code.ToLower().StartsWith(smQueryOptions.Search.ToLower()))

            );
        #endregion
        var query = baseQuery;
        return query;

    }

    public static IFindFluent<DemoModels.Product, DemoModels.Product> GetBaseQueryMongo(IMongoCollection<DemoModels.Product> table, SmQueryOptions smQueryOptions, bool OnlyStocks)
    {

        var filterbuilder = Builders<DemoModels.Product>.Filter;
        FilterDefinition<DemoModels.Product>? searchFilter = filterbuilder.Empty;
        var baseQuery = table;

        #region apply OnlyStocks
        if (OnlyStocks)
        {
            searchFilter &= filterbuilder.Gte("Stocks.Quantity", 0);
        }
        #endregion

        #region apply search
        if (smQueryOptions.Search != null)
        {
            searchFilter &= filterbuilder.Gte(x => x.Name, smQueryOptions.Search)
                & filterbuilder.Lte(x => x.Name, smQueryOptions.Search + "zzzz")
                ;
        }
        #endregion

        var fo = new FindOptions() { Collation = new Collation("hu", strength: CollationStrength.Primary) };
        var query = baseQuery.Find(searchFilter, fo);
        return query;
    }
}
