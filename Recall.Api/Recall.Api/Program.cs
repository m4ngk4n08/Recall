using Microsoft.EntityFrameworkCore;
using Npgsql;
using Recall.Api.Data;
using Recall.Api.Middleware;
using Recall.Api.Repositories;
using Recall.Api.Repositories.Interfaces;
using Recall.Api.Services;
using Recall.Api.Services.Interfaces;

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

// DI
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IITemService, ItemService>();
builder.Services.AddScoped<IExtractionService, ExtractionService>();
//builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IIngestionService, IngestionService>();
builder.Services.AddScoped<IEmbeddingService, LocalEmbeddingService>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("DevCors");

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
