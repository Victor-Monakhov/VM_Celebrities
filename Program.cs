using VM_Celebrities_Back.Interfaces;
using VM_Celebrities_Back.Repositories;
using VM_Celebrities_Back.Services;
using ICelebrityScraperService = VM_Celebrities_Back.Services.ICelebrityScraperService;

var builder = WebApplication.CreateBuilder(args);

var celebrityOrigins = "celebrityOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: celebrityOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddScoped<ICelebrityRepository, CelebrityRepository>();
builder.Services.AddScoped<ICelebrityScraperService, CelebrityScraperService.CelebrityScraper>();

builder.Services.AddControllers();
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

app.UseHttpsRedirection();
app.MapControllers();
app.UseRouting();
app.UseCors(celebrityOrigins);

app.Run();