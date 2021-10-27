using Microsoft.AspNetCore.OData;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddOData(opt => opt.AddRouteComponents("odata",
                        Models.ProductODataEdmModel.GetEdmModel()).Filter().Expand().Select().Count().SkipToken().OrderBy().SetMaxTop(500))
                ;

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "DemoBackend", Version = "v1" });
});
builder.Services.AddCors();
    

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "DemoBackend v1"); c.RoutePrefix = string.Empty; });

app.UseHttpsRedirection();

app.UseCors(options => {
    options.AllowAnyOrigin();
    options.AllowAnyHeader();
    options.AllowAnyMethod();
});

app.UseAuthorization();

app.MapControllers();



app.Run();
