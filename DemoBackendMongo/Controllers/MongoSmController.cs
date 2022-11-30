using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common;
using DemoModels;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using SmQueryOptionsNs;
using Swashbuckle.AspNetCore.SwaggerGen;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class MongoSmController<T> : ControllerBase where T : class
    {
        protected IMongoCollection<T> Table { get; set; }

        protected virtual IFindFluent<T, T>? GetFilteredQuery(SmQueryOptions? smQueryOptions)
        {
            var res = Table.Find(x => true);
            return res;
        }

        [HttpGet()]
        public virtual async Task<ActionResult> Get([ModelBinder(BinderType = typeof(SmQueryOptionsUrlBinder.SmQueryOptionsUrlBinder))] SmQueryOptionsUrl smQueryOptionsUrl)
        {
            #region delay
            var sw = System.Diagnostics.Stopwatch.StartNew();
            #endregion

            SmQueryOptions? smQueryOptions = SmQueryOptionsUrl.Parse(smQueryOptionsUrl);
            
            var filteredQuery = GetFilteredQuery(smQueryOptions);
            var query = filteredQuery.Limit(10);
            var res = await query.ToListAsync();

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
   }
}
