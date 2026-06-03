using Microsoft.EntityFrameworkCore;
using Npgsql;
using Recall.Api.Data;
using Recall.Api.Middleware;
using Recall.Api.Repositories;
using Recall.Api.Repositories.Interfaces;
using Recall.Api.Services;
using Recall.Api.Services.Interfaces;
using Recall.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
dataSourceBuilder.UseVector();
var dataSource = dataSourceBuilder.Build();
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dataSource, o => o.UseVector()));

var key = builder.Configuration["Gemini:ApiKey"];

Console.WriteLine(string.IsNullOrEmpty(key) ? "missing" : "ok");

builder.Services.Configure<OllamaConnectionSettings>(
    builder.Configuration.GetSection(OllamaConnectionSettings.SectionName));

// DI
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IITemService, ItemService>();
builder.Services.AddScoped<IExtractionService, ExtractionService>();
//builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IIngestionService, IngestionService>();
builder.Services.AddSingleton<IEmbeddingService, LocalEmbeddingService>();
builder.Services.AddHttpClient<IOllamaService, OllamaService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("DevCors", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    } 
    else
    {
        options.AddPolicy("DockerCors", policy =>
        {
            policy.WithOrigins("http://localhost:8080") // Angular container port
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    }

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevCors");
}
else
{
    app.UseCors("DockerCors");
}

app.UseMiddleware<GlobalExceptionHandler>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

    app.Run();
