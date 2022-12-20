using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using SmQueryOptionsNs;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductMongoController : ControllerBase
    {
        public ProductMongoController()
        {
            Database.SmDemoProductMongoDatabase.InitRandomData();
        }

        [HttpGet(nameof(Search))]
        public async Task<ActionResult> Search(int? top, int? skip, string? search, string? select, bool OnlyStocks = false)
        {
            #region delay
            var sw = System.Diagnostics.Stopwatch.StartNew();
            #endregion

            var smQueryOptions = SmQueryOptionsUrl.Parse(top, skip, search, select);



            var table = Database.SmDemoProductMongoDatabase.Product;
            var query = GetBaseQuery(table, smQueryOptions, OnlyStocks);

            query = query.ApplySkipTop(smQueryOptions);
            var queryResult = await query.RunQuery();
            var res = queryResult.Select(x => ProjectResultItem(x, smQueryOptions));

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

        private DemoModels.ProductDto ProjectResultItem(DemoModels.Product x, SmQueryOptions smQueryOptions)
        {
            var res = new DemoModels.ProductDto();
            SmQueryOptionsNs.Mapper.CopyProperties(x, res, false, false, smQueryOptions.Select);
            if (smQueryOptions.Select?.Contains("StockSumQuantity".ToLower()) ?? true)
                res.StockSumQuantity = (x.Stocks?.Count() > 0) ? x.Stocks?.Sum(x => x.Quantity) : null;
            if (smQueryOptions.Select?.Contains("Description".ToLower()) ?? true)
                res.Description = x.Ext?.Description;
            return res;
        }

        private IFindFluent<DemoModels.Product, DemoModels.Product> GetBaseQuery(IMongoCollection<DemoModels.Product> table, SmQueryOptions smQueryOptions, bool OnlyStocks)
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
}
