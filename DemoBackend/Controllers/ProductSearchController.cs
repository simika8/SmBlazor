using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Database;
using DemoModels;
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
    protected ProductRepo SearchRepo { get; set; }
    protected CrudRepo<Product, Guid> CrudRepo { get; set; }
    public ProductSearchController()
    {
        SearchRepo = new ProductRepo();

        CrudRepo = new CrudRepo<DemoModels.Product, Guid>(Database.DictionaryDatabase.Products);
        CrudRepo.InitRandomData();
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
        var res = await ProductRepo.Search(smQueryOptions, onlyStocks);
        
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



}
