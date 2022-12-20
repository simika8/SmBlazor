using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using SmQueryOptionsNs;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductEfController : ControllerBase
    {
        public ProductEfController()
        {
            Database.SmDemoProductContext.InitRandomData();
            //using var db = new Database.SmDemoProductContext();
        }

        [HttpGet(nameof(Search))]
        public async Task<ActionResult> Search([FromQuery] SmQueryOptionsUrl smQueryOptionsUrl, bool OnlyStocks = false)
        {
            ;
            #region delay
            var sw = System.Diagnostics.Stopwatch.StartNew();
            #endregion

            var smQueryOptions = SmQueryOptionsUrl.Parse(smQueryOptionsUrl);

            using var db = new Database.SmDemoProductContext();
            var table = db.Product;
            var query = GetBaseQuery(table, smQueryOptions, OnlyStocks);

            query = query.ApplySkipTop(smQueryOptions);
            var queryResult = await query.RunQuery();
            var res = queryResult.Select(x => ProjectResultItem(x, smQueryOptions));

            #region delay
            sw.Stop();
            var smQueryOptionsUrlJson = System.Text.Json.JsonSerializer.Serialize(smQueryOptionsUrl);
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
        private IQueryable<DemoModels.Product> GetBaseQuery(DbSet<DemoModels.Product> table, SmQueryOptions smQueryOptions, bool OnlyStocks)
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
    }
}