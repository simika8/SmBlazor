using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmProductController : ControllerBase
    {
        /*private static const SmData.SmQueryOptionsUrl ExampleSmQueryOptionsUrl = new()
        {
            Select = "Id, Name, Price, Stocks, Rating",
        };*/
        protected Dictionary<Guid, Models.Product> Table { get; set; } = new Dictionary<Guid, Models.Product> { };
        public SmProductController()
        {
            Models.DataTables.InitRandomData();
            Table = Models.DataTables.Products;
        }
        /// <summary>
        /// Example: 
        /// {
        ///   "top": 0,
        ///   "skip": 0,
        ///   "search": "string",
        ///   "filter": "string",
        ///   "orderby": "string",
        ///   "select": "string"
        /// }
        /// </summary>
        /// <param name="smQueryOptionsUrl"></param>
        /// <returns></returns>
        [HttpGet()]
        public async Task<ActionResult> Get(SmData.SmQueryOptionsUrl smQueryOptionsUrl)
        {
            var smQueryOptions = SmData.SmQueryOptionsUrl.Parse(smQueryOptionsUrl);
            var query = Table.Select(x => x.Value).AsQueryable();
            query = smQueryOptions.Apply(query);
            var res = query.ToList();
            ;
            await Task.Delay(0);
            return Ok(res);
        }
    }
}
