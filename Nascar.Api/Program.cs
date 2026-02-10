using Microsoft.EntityFrameworkCore;
using Nascar.Infrastructure.Data;
using Nascar.Infrastructure.Repositories;
using Nascar.Api.Clients;
using Microsoft.ML;
using Nascar.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// DbContext
builder.Services.AddDbContext<NascarDbContext>(opt =>
    opt.UseSqlite("Data Source=nascar.db"));

// Repository
builder.Services.AddScoped<INascarRepository, NascarRepository>();

// Http client for real NASCAR JSON[web:31]
builder.Services.AddHttpClient<NascarLiveFeedClient>(client =>
{
    client.BaseAddress = new Uri("https://cf.nascar.com/");
});

// ML.NET
builder.Services.AddSingleton<MLContext>();
builder.Services.AddScoped<PredictionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();