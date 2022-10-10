using Microsoft.AspNetCore.OData;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    })
    .AddOData(opt => opt.AddRouteComponents("odata",
                        DemoModels.ProductODataEdmModel.GetEdmModel()).Filter().Expand().Select().Count().SkipToken().OrderBy().SetMaxTop(500))
                ;

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "DemoBackend", Version = "v1" });
    //var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    //c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

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
