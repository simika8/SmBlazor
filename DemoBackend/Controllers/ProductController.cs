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

            var query = Table.AsQueryable().Select(x=>x.Value).Where(x => true);

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

        protected override IEnumerable<DemoModels.Product> GetFilteredQuery(SmQueryOptions? smQueryOptions)
        {
            if (smQueryOptions.Search == null)
                return Table.Select(x => x.Value).Where(x => true);
            var res = Table.Select(x => x.Value).Where(x =>
                    x.Name.ToLowerInvariant().Contains(smQueryOptions.Search.ToLowerInvariant())
                    || x.Code.ToLowerInvariant().StartsWith(smQueryOptions.Search.ToLowerInvariant())
                );


            return res;
        }

    }
}
