using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
    public class SmDictionaryController<T> : ControllerBase
    {
        protected Dictionary<Guid, T> Table { get; set; } = new Dictionary<Guid, T> { };

        //[ModelBinder(BinderType = typeof(SmQueryOptionsUrlBinder))]
        [HttpGet()]
        public async Task<ActionResult> Get([ModelBinder(BinderType = typeof(SmQueryOptionsUrlBinder.SmQueryOptionsUrlBinder))]SmQueryOptionsUrl smQueryOptionsUrl)
        {
            var smQueryOptions = SmQueryOptionsUrl.Parse(smQueryOptionsUrl);
            var query = Table.Select(x => x.Value).AsQueryable();
            Type elementType = typeof(DemoModels.Product);
            ParameterExpression parameterExpression = Expression.Parameter(elementType);

            query = smQueryOptions.ApplySearch(parameterExpression, query, SearchExpression<T>(parameterExpression, smQueryOptions.Search));
            //query = smQueryOptions.ApplySearch(parameterExpression, query, null);

            query = smQueryOptions.Apply(query);
            var res = query.ToList();
            ;
            await Task.Delay(0);
            return Ok(res);
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
