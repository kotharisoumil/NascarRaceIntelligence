using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Nascar.Infrastructure.Data;
using Nascar.Infrastructure.Repositories;
using Nascar.Api.Services;
using Nascar.Api.Clients;

var builder = WebApplication.CreateBuilder(args);

// Controllers (no Swagger needed)
builder.Services.AddControllers();

// DbContext
builder.Services.AddDbContext<NascarDbContext>(options =>
    options.UseSqlite("Data Source=nascar.db"));

// Repository
builder.Services.AddScoped<INascarRepository, NascarRepository>();

// ML.NET
builder.Services.AddSingleton<MLContext>();

// Services
builder.Services.AddScoped<PredictionService>();
builder.Services.AddScoped<LiveRaceService>();

// HttpClient for NASCAR feeds
builder.Services.AddHttpClient<NascarLiveFeedClient>(client =>
{
    client.BaseAddress = new Uri("https://cf.nascar.com/");
});

// CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.MapControllers();

Console.WriteLine("🚀 NASCAR API running!");
Console.WriteLine("📡 Test endpoint: https://localhost:5001/api/live/1/5273");
Console.WriteLine("Press Ctrl+C to stop.");
app.Run();
