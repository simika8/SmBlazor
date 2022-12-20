using Swashbuckle.AspNetCore.Filters;

namespace Controllers;

public class PatchProductExample : IExamplesProvider<Newtonsoft.Json.Linq.JObject>
{
    public Newtonsoft.Json.Linq.JObject GetExamples()
    {
        var example = new { 
            Name = "newName",
            Rating = 3 
        };

        var res = Newtonsoft.Json.Linq.JObject.FromObject(example);
        

        return res;
    }
}