﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common;
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
    public class DictionarySmController<T> : ControllerBase where T : class
    {
        protected Dictionary<Guid, T> Table { get; set; } = new Dictionary<Guid, T> { };

        //[ModelBinder(BinderType = typeof(SmQueryOptionsUrlBinder))]
        [HttpGet()]
        public async Task<ActionResult> Get([ModelBinder(BinderType = typeof(SmQueryOptionsUrlBinder.SmQueryOptionsUrlBinder))]SmQueryOptionsUrl smQueryOptionsUrl)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var smQueryOptions = SmQueryOptionsUrl.Parse(smQueryOptionsUrl);
            var query = Table.Select(x => x.Value).AsQueryable();
            Type elementType = typeof(DemoModels.Product);
            ParameterExpression parameterExpression = Expression.Parameter(elementType);

            query = smQueryOptions.ApplySearch(parameterExpression, query, SearchExpression<T>(parameterExpression, smQueryOptions.Search));
            //query = smQueryOptions.ApplySearch(parameterExpression, query, null);

            query = smQueryOptions.Apply(query);
            var res = query.ToList();
            ;

            sw.Stop();



            var smQueryOptionsUrlJson = System.Text.Json.JsonSerializer.Serialize(smQueryOptionsUrl);
            var rndTime = new Random(smQueryOptionsUrlJson.GetHashCode());
            var timeMs = (int)(Math.Pow(rndTime.NextDouble(), 4) * (AdminController.MaxQueryMilliseconds - AdminController.MinQueryMilliseconds) + AdminController.MinQueryMilliseconds);

            if (timeMs - (int)sw.ElapsedMilliseconds > 0)
                await Task.Delay(timeMs - (int)sw.ElapsedMilliseconds);



            return Ok(res);


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

        [HttpPut]
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

        [HttpPatch]
        public IActionResult Patch(Guid id, [FromBody] JsonPatchDocument<T> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (Table.TryGetValue(id, out var entity))
            {
                patch.ApplyTo(entity);

                var isValid = TryValidateModel(entity);
                if (!isValid)
                {
                    return BadRequest(ModelState);
                }

                //Table[key] = entity;
                //entity = modifiedEntity;
            }
            else
                return NotFound();

            return NoContent();
        }

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

        [HttpDelete]
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


        protected virtual Expression? SearchExpression<T>(ParameterExpression parameterExpression, string? search)
        {
            const string defaultSearchPropertyName = "Name";

            PropertyInfo? propertyInfo = typeof(T).GetProperties()
                .Where(x => x.PropertyType == typeof(string))
                .Where(x => x.Name == defaultSearchPropertyName)
                .FirstOrDefault();
            if (propertyInfo == null)
                return null;

            if (string.IsNullOrWhiteSpace(search))
                return null;

            var expressions = new List<Expression>();

            AddExpressionIfNotNull(expressions, SmQueryOptionsNs.SmQueryOptions.StartsWithCaseInsensitiveExpression<T>(parameterExpression, defaultSearchPropertyName, search));

            Expression aggregatedExpression = expressions.Aggregate((prev, current) => Expression.Or(prev, current));

            return aggregatedExpression;
        }

        protected void AddExpressionIfNotNull(List<Expression> list, Expression? item)
        {
            if (item != null)
                list.Add(item);
        } 
    }
}
