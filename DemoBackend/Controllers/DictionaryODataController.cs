using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Controllers
{

    public abstract class DictionaryODataController<T> : ODataController where T : class
    {
        protected Dictionary<Guid, T> Table { get; set; } = new Dictionary<Guid, T> { };

        /// <summary>
        /// Apply Server Side Filter (must be well indexed, can affect multiple columns, can have unique logic or text search)
        /// </summary>
        protected abstract IQueryable<T> ApplySearchFilter(IQueryable<T> q1, string keywords);

        [HttpGet]
        public async Task<IEnumerable<T>> Get(ODataQueryOptions<T> queryOptions, CancellationToken cancellationToken, string? search = "")
        {
            try
            {
                var query = Table.Select(x => x.Value).AsQueryable();

                //apply model specific server side $search filter
                var queryString = HttpContext.Request.QueryString.ToString();
                var keywords = Others.GetSearchField(queryString);
                if (!string.IsNullOrEmpty(keywords))
                    query = ApplySearchFilter(query, keywords);


                //apply Odata query options
                var dinQuery = (IQueryable<dynamic>)queryOptions.ApplyTo(query);
                ;
                //run Query
                var queryResult = await Task.Run(() =>
                {
                    var queryResult = dinQuery.ToArray();
                    return queryResult;
                });

                //Extracts Data from MS.odada shits. If we return untyped queryResult, then api result will not contain "@odata.context and "value" parts
                var typedQueryResult = queryResult.ToTypedCollection<T>();
                return typedQueryResult;

            }
            catch (System.Threading.Tasks.TaskCanceledException e)
            {
                Console.WriteLine(e.ToString());
                return new List<T>();
            }



        }
        [HttpPut]
        public IActionResult Put(Guid key, [FromBody] Microsoft.AspNetCore.OData.Deltas.Delta<T> delta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = Table.GetValueOrDefault(key);
            if (entity == null)
            {
                return NotFound();
            }

            delta.Put(entity);

            var res = Updated(entity);
            return res;
        }


        [HttpPatch]
        public IActionResult Patch([Microsoft.AspNetCore.OData.Formatter.FromODataUri] Guid key, Microsoft.AspNetCore.OData.Deltas.Delta<T> delta)
        {


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = Table.GetValueOrDefault(key);
            if (entity == null)
            {
                return NotFound();
            }

            delta.Patch(entity);


            var res = Updated(entity);
            return res;
        }

        [HttpPost]
        public IActionResult Post([FromBody] T entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var key = Others.GetGuidKey(entity);
            Table.Add(key, entity);
            return Created(entity);
        }

        [HttpDelete]
        public IActionResult Delete(Guid key)
        {
            var entity = Table.GetValueOrDefault(key);
            if (entity == null)
            {
                return NotFound();
            }

            Table.Remove(key);
            return Ok();
        }

    }
}
