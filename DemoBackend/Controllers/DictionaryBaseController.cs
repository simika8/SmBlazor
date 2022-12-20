using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common;
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
    public abstract class DictionaryBaseController<T> : ControllerBase where T : class
    {
        protected Dictionary<Guid, T> Table { get; set; } = new Dictionary<Guid, T> { };

        [HttpPost]
        public IActionResult Post([FromBody] T entity)
        {
            var id = Others.GetGuidKey(entity);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            /*if (Table.TryGetValue(key, out var dbentity))
            {
                Table[key] = entity;
                //entity = modifiedEntity;
            }
            else*/
            Table[id] = entity;

            return CreatedAtAction(nameof(Post), id, entity);

        }


        [HttpPut("{id:Guid}")]
        public IActionResult Put(Guid id, [FromBody] T modifiedEntity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (Table.TryGetValue(id, out var entity))
            {
                Table[id] = modifiedEntity;
                //entity = modifiedEntity;
            } else
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:Guid}")]
        public async Task<ActionResult<T>> Delete(Guid id)
        {
            if (Table.ContainsKey(id))
            {
                Table.Remove(id);
            }
            else
                return NotFound();

            return NoContent();
        }

        [HttpGet("{id:Guid}")]
        public async Task<ActionResult<T>> Get(Guid id)
        {
            if (Table.TryGetValue(id, out var entity))
            {
                return entity;
            }
            else
                return NotFound();
        }
        

        /// <summary>
        /// json merge patch
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patch"></param>
        /// <returns></returns>
        [HttpPatch("{id:Guid}")]
        public virtual IActionResult Patch(Guid id, [FromBody] Newtonsoft.Json.Linq.JObject patch)
        {
            if (Table.TryGetValue(id, out var entity))
            {
                var sourceObject = Newtonsoft.Json.Linq.JObject.FromObject(entity);
                sourceObject.Merge(patch, new Newtonsoft.Json.Linq.JsonMergeSettings() { MergeArrayHandling = Newtonsoft.Json.Linq.MergeArrayHandling.Union });
                entity = sourceObject.ToObject<T>();
                if (entity == null)
                    return NotFound("entity == null");

                var isValid = TryValidateModel(entity);
                if (!isValid)
                {
                    return BadRequest(ModelState);
                }

                Table[id] = entity;
            }
            else
                return NotFound();

            return NoContent();


        }



        protected void AddExpressionIfNotNull(List<Expression> list, Expression? item)
        {
            if (item != null)
                list.Add(item);
        } 
    }
}
