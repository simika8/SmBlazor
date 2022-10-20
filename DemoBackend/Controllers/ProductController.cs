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
using SmQueryOptionsNs;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : DictionarySmController<DemoModels.Product>
    {
        public ProductController()
        {
            DemoModels.DataTables.InitRandomData();
            Table = DemoModels.DataTables.Products;
        }

        protected override Expression? SearchExpression<T>(ParameterExpression parameterExpression, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return null;

            var expressions = new List<Expression>();

            AddExpressionIfNotNull(expressions, SmQueryOptionsNs.SmQueryOptions.ContainsCaseInsensitiveExpression<T>(parameterExpression, "Name", search));
            AddExpressionIfNotNull(expressions, SmQueryOptionsNs.SmQueryOptions.StartsWithCaseInsensitiveExpression<T>(parameterExpression, "Code", search));

            Expression aggregatedExpression = expressions.Aggregate((prev, current) => Expression.Or(prev, current));


            return aggregatedExpression;
        }

        /*[HttpGet("test")]
        public async Task<ActionResult> test(SmQueryOptionsNs.SmQueryOptionsUrl smQueryOptionsUrl)
        {
            var smQueryOptions = SmQueryOptionsUrl.Parse(smQueryOptionsUrl);
            var res = SmQueryOptionsUrl.Parse(smQueryOptions);

            return Ok(res);
        }*/
    }
}
