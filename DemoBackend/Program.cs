using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Common;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

/*builder.Services.AddControllers(options =>
    {
        options.InputFormatters.Insert(0, MyJPIF.GetJsonPatchInputFormatter());
    })
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    })
    .AddOData(opt => opt.AddRouteComponents("odata",
                        DemoModels.ProductODataEdmModel.GetEdmModel()).Filter().Expand().Select().Count().SkipToken().OrderBy().SetMaxTop(500))
                ;*/

builder.Services.AddControllers(options =>
    {
        options.InputFormatters.Insert(0, MyJpif.GetJsonPatchInputFormatter());
    }).AddNewtonsoftJson(opt =>
    {
        opt.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        opt.UseMemberCasing();
        opt.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
    });


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "DemoBackend", Version = "v1" });
    c.ExampleFilters();
    var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    string[] methodsOrder = new string[] {"post", "get", "put", "patch", "delete", };
    //c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}");
    //c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{Array.IndexOf(methodsOrder, apiDesc.HttpMethod?.ToLower())}");
    //c.OrderActionsBy(apiDesc => $"{Array.IndexOf(methodsOrder, apiDesc.HttpMethod?.ToLower())}");
    c.OrderActionsBy(apiDesc => $"{apiDesc.RelativePath}_{Array.IndexOf(methodsOrder, apiDesc.HttpMethod?.ToLower())}");
    //c.OrderActionsBy(apiDesc => $"{(new Random()).Next()}");

});
builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

builder.Services.AddCors();

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseSwagger();

app.UseSwaggerUI(
    c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "DemoBackend v1"); 
        c.RoutePrefix = string.Empty;
    });

app.UseHttpsRedirection();

app.UseCors(options => {
    options.AllowAnyOrigin();
    options.AllowAnyHeader();
    options.AllowAnyMethod();
});


app.UseAuthorization();

app.MapControllers();



app.Run();
