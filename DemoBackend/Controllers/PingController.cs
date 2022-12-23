using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class PingController : ControllerBase
{

    [HttpGet()]
    public async Task<ActionResult> Ping()
    {
        Reporitory.RepositoryAdmin.InitRandomData();
        await Task.Delay(0);
        return Ok();
    }

}