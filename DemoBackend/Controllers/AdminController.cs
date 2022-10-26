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
public class AdminController : ControllerBase
{
    public static int MinQueryMilliseconds { get; set; } = 100;
    public static int MaxQueryMilliseconds { get; set; } = 3000;

    [HttpPut(nameof(SetQueryRunTime))]
    public async Task<ActionResult> SetQueryRunTime(int minQueryMilliseconds, int maxQueryMilliseconds)
    {
        MinQueryMilliseconds = minQueryMilliseconds;
        MaxQueryMilliseconds = maxQueryMilliseconds;

        return Ok();
    }
}

