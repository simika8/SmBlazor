var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.InputFormatters.Insert(0, Common.MyJpif.GetJsonPatchInputFormatter());
}).AddNewtonsoftJson(opt =>
{
    opt.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    opt.UseMemberCasing();
    opt.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "DemoBackendMongo", Version = "v1" });
    //var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    //c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddCors();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(
    c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DemoBackend v1");
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
