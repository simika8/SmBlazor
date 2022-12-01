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
    public class ProductController : DictionarySmController<DemoModels.Product, DemoModels.ProductDto>
    {
        public ProductController()
        {
            Database.DictionaryDatabase.InitRandomData();
            Table = Database.DictionaryDatabase.Products;

            var query = Table.AsQueryable().Select(x=>x.Value).Where(x => true);

        }

        /*protected override Expression? SearchExpression<T>(ParameterExpression parameterExpression, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return null;

            var expressions = new List<Expression>();

            AddExpressionIfNotNull(expressions, SmQueryOptionsNs.SmQueryOptions.ContainsCaseInsensitiveExpression<T>(parameterExpression, "Name", search));
            AddExpressionIfNotNull(expressions, SmQueryOptionsNs.SmQueryOptions.StartsWithCaseInsensitiveExpression<T>(parameterExpression, "Code", search));

            Expression aggregatedExpression = expressions.Aggregate((prev, current) => Expression.Or(prev, current));


            return aggregatedExpression;
        }*/

        protected override IEnumerable<DemoModels.Product> GetFilteredQuery(SmQueryOptions? smQueryOptions)
        {
            if (smQueryOptions.Search == null)
                return Table.Select(x => x.Value).Where(x => true);
            var query = Table.Select(x => x.Value).Where(x =>
                    (x.Name != null && x.Name.ToLowerInvariant().Contains(smQueryOptions.Search.ToLowerInvariant()))
                    ||
                    (x.Code != null && x.Code.ToLowerInvariant().StartsWith(smQueryOptions.Search.ToLowerInvariant()))

                );

            var query2 = query.Select(x => new DemoModels.Product() { Name = x.Name });

            return query;
        }

        protected override DemoModels.ProductDto ProjectResultItem(DemoModels.Product x, SmQueryOptions smQueryOptions)
        {
            var res = new DemoModels.ProductDto();
            SmQueryOptionsNs.Mapper.CopyProperties(x, res, false, false, smQueryOptions.Select);
            if (smQueryOptions.Select?.Contains("StockSumQuantity".ToLower()) ?? true)
                res.StockSumQuantity = x.Stocks?.Sum(x => x.Quantity);
            if (smQueryOptions.Select?.Contains("Description".ToLower()) ?? true)
                res.Description = x.Ext?.Description;
            return res;
        }


    }
}
