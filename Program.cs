using VM_Celebrities_Back.Repositories;
using VM_Celebrities_Back.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ICelebrityRepository, CelebrityRepository>();
builder.Services.AddScoped<ICelebrityScraperService, CelebrityScraperService.CelebrityScraper>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("swagger-api-celebrities", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CELEBRITIES",
        Version = "1.0"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger-api-celebrities/swagger.json", "CELEBRITIES");
        c.RoutePrefix = string.Empty;
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseRouting();

app.Run();