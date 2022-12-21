using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using SmQueryOptionsNs;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Controllers;

/// <summary>
/// ### Search in products
/// Results projected to "ProductSearchResult" format.
/// 
/// To use MongoDB, EF core version set up database connection strings like this:
/// 
/// ```
/// shell setx SmBlazorMongoConnectionString "mongodb://localhost:27017"
/// setx SmBlazorPgConnectionString "Host=localhost;Username=somebody;Password=somepwd;Database=smDemoDb"
/// ```
/// 
/// </summary>

[Route("api/[controller]")]
[ApiController]
public class ProductSearchController : ControllerBase
{
    public ProductSearchController()
    {
        Database.DictionaryDatabase.InitRandomData();

        Database.SmDemoProductContext.InitRandomData();

        Database.SmDemoProductMongoDatabase.InitRandomData();

    }

    /// <summary>
    /// Search in in-memory Products database
    /// </summary>
    /// <param name="top" example="10"></param>
    /// <param name="skip"></param>
    /// <param name="search" example="prod"></param>
    /// <param name="select" example="Id, Code, Name, Price, StockSumQuantity">Select fields in response</param>
    /// <param name="onlyStocks">Search fory only item with stocks</param>
    /// <returns>List of products</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DemoModels.ProductSearchResult>>> Search(int? top, int? skip, string? search, string? select, bool onlyStocks = false)
    {
        #region delay
        var sw = System.Diagnostics.Stopwatch.StartNew();
        #endregion

        var smQueryOptions = SmQueryOptionsUrlHelper.Parse(top, skip, search, select);

        IEnumerable<DemoModels.ProductSearchResult> res;
        switch (AdminController.DbType)
        {
            case AdminController.DatabaseType.Dictionary:
                res = await SearchDict(smQueryOptions, onlyStocks);
                break;
            case AdminController.DatabaseType.EfPg:
                res = await SearchEf(smQueryOptions, onlyStocks);
                break;
            case AdminController.DatabaseType.Mongo:
                res = await SearchMongo(smQueryOptions, onlyStocks);
                break;
            default:
                res = await SearchDict(smQueryOptions, onlyStocks);
                break;
        }
        #region delay
        sw.Stop();
        var smQueryOptionsUrlJson = System.Text.Json.JsonSerializer.Serialize(smQueryOptions);
        var rndTime = new Random(smQueryOptionsUrlJson.GetHashCode());
        var timeMs = (int)(Math.Pow(rndTime.NextDouble(), 4) * (AdminController.MaxQueryMilliseconds - AdminController.MinQueryMilliseconds) + AdminController.MinQueryMilliseconds);

        if (timeMs - (int)sw.ElapsedMilliseconds > 0)
            await Task.Delay(timeMs - (int)sw.ElapsedMilliseconds);
        #endregion
        return Ok(res);
    }

    private async Task<IEnumerable<DemoModels.ProductSearchResult>> SearchDict(SmQueryOptions smQueryOptions, bool onlyStocks = false)
    {
        var table = Database.DictionaryDatabase.Products;
        var query = GetBaseQuery(table, smQueryOptions, onlyStocks);

        query = query.ApplySkipTop(smQueryOptions);
        var queryResult = await query.RunQuery();
        var res = queryResult.Select(x => ProjectResultItem(x, smQueryOptions));

        return res;
    }

    private async Task<IEnumerable<DemoModels.ProductSearchResult>> SearchEf(SmQueryOptions smQueryOptions, bool onlyStocks = false)
    {
        using var db = new Database.SmDemoProductContext();
        var table = db.Product;
        var query = GetBaseQueryEf(table, smQueryOptions, onlyStocks);

        query = query.ApplySkipTop(smQueryOptions);
        var queryResult = await query.RunQuery();
        var res = queryResult.Select(x => ProjectResultItem(x, smQueryOptions));

        return res;
    }

    private async Task<IEnumerable<DemoModels.ProductSearchResult>> SearchMongo(SmQueryOptions smQueryOptions, bool onlyStocks = false)
    {
        var table = Database.SmDemoProductMongoDatabase.Product;
        var query = GetBaseQueryMongo(table, smQueryOptions, onlyStocks);

        query = query.ApplySkipTop(smQueryOptions);
        var queryResult = await query.RunQuery();
        var res = queryResult.Select(x => ProjectResultItem(x, smQueryOptions));

        return res;
    }


    private DemoModels.ProductSearchResult ProjectResultItem(DemoModels.Product x, SmQueryOptions smQueryOptions)
    {
        var res = new DemoModels.ProductSearchResult();
        SmQueryOptionsNs.Mapper.CopyProperties(x, res, false, false, smQueryOptions.Select);
        if (smQueryOptions.Select?.Contains("StockSumQuantity".ToLower()) ?? true)
            res.StockSumQuantity = (x.Stocks?.Count() > 0) ? x.Stocks?.Sum(x => x.Quantity) : null;
        if (smQueryOptions.Select?.Contains("Description".ToLower()) ?? true)
            res.Description = x.Ext?.Description;
        return res;
    }

    private IEnumerable<DemoModels.Product> GetBaseQuery(Dictionary<Guid, DemoModels.Product> table, SmQueryOptions smQueryOptions, bool OnlyStocks)
    {
        var baseQuery = table.Select(x => x.Value);

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

    private IQueryable<DemoModels.Product> GetBaseQueryEf(DbSet<DemoModels.Product> table, SmQueryOptions smQueryOptions, bool OnlyStocks)
    {
        var baseQuery = table
            .Include(x => x.Stocks)
            .Include(x => x.Ext)
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

    private IFindFluent<DemoModels.Product, DemoModels.Product> GetBaseQueryMongo(IMongoCollection<DemoModels.Product> table, SmQueryOptions smQueryOptions, bool OnlyStocks)
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
