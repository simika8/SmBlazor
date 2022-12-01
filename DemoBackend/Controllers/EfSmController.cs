using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common;
using Database;
using DemoModels;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmQueryOptionsNs;
using Swashbuckle.AspNetCore.SwaggerGen;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class EfSmController<T, TDto> : ControllerBase where T : class
    {

        protected virtual IQueryable<T>? GetFilteredQuery(SmDemoProductContext db, SmQueryOptions? smQueryOptions)
        {

            var res = db.Set<T>().Where(x => true);
            return res;
        }
        protected abstract TDto ProjectResultItem(T x, SmQueryOptions? smQueryOptions);

        [HttpGet()]
        public virtual async Task<ActionResult> Get([ModelBinder(BinderType = typeof(SmQueryOptionsUrlBinder.SmQueryOptionsUrlBinder))] SmQueryOptionsUrl smQueryOptionsUrl)
        {
            using var db = new SmDemoProductContext();
            #region delay
            var sw = System.Diagnostics.Stopwatch.StartNew();
            #endregion

            SmQueryOptions? smQueryOptions = SmQueryOptionsUrl.Parse(smQueryOptionsUrl);

            var query = GetFilteredQuery(db, smQueryOptions);
            if (smQueryOptions.Skip > 0)
                query = query.Skip(smQueryOptions.Skip ?? 0);
            if (smQueryOptions.Top > 0)
                query = query.Take(smQueryOptions.Top ?? 1);
            var queryResult = await query.ToListAsync();
            var res = queryResult.Select(x => ProjectResultItem(x, smQueryOptions));

            ;
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
