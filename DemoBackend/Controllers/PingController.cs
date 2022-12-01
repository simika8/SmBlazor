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
        Database.DictionaryDatabase.InitRandomData();
        await Task.Delay(0);

        var col1 = new SmColumn2() { FieldName = "a" };
        var col2 = new SmColumn2() { FieldName = "a" };
        var col3 = new SmColumn2() { FieldName = "b" };
        var collist1 = new List<SmColumn2> { col1, col2 }.ToImmutableArray();
        var collist2 = new List<SmColumn2> { col1, col2 }.ToImmutableArray();
        var b1 = collist1.Equals(collist2);

        ;

        var settings1 = new Settings2 { Name = "asdf"};
        settings1.Height = 200;

        var settings2 = new Settings2 { Name = "asdf" };
        settings2.Height = 200;

        var b = settings1.Equals(settings2);
        ;

        return Ok();
    }

}


public struct Settings2
{
    public string Name { get; set; }
    public int FirstTopCount { get; set; } = 20;
    public int Height { get; set; } = 200;
    public string Search { get; set; } = "";
    public int Cursor { get; set; } = 0;
    //public SmColumn2[] Columns { get; set; }

}
public struct SmColumn2
{
    private string fieldName;
    private string[] splittedFieldName;
    private string? propertyTypeName;
    private bool? rightAligned;
    private Func<object?, object?>? cellFormatter;

    internal string[] SplittedFieldName { get => splittedFieldName; }
    public string FieldName
    {
        get => fieldName;
        set
        {
            fieldName = value;
            splittedFieldName = fieldName.Split(".");
        }
    }
}
