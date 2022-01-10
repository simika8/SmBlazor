using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmProductController : ControllerBase
    {
        protected Dictionary<Guid, Models.Product> Table { get; set; } = new Dictionary<Guid, Models.Product> { };
        public SmProductController()
        { 
            Models.DataTables.InitRandomData();
            Table = Models.DataTables.Products;
        }
        [HttpGet()]
        public async Task<ActionResult> Get(SmData.SmQueryOptionsUrl smQueryOptionsUrl)
        {
            var smQueryOptions = SmData.SmQueryOptionsUrl.Convert2SmQueryOptions(smQueryOptionsUrl);
            var query = Table.Select(x => x.Value).AsQueryable();
            query = smQueryOptions.Apply<Models.Product>(query);
            var res = query.ToList();
            ;
            await Task.Delay(0);
            return Ok(res);
        }

    }
}
