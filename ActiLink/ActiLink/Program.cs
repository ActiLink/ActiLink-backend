using ActiLink.Repositories;
using ActiLink;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// Db Context
builder.Services.AddDbContext<WeatherContext>(options =>
    options.UseInMemoryDatabase("InMemoryDb"));


// Repositories and UoW
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Docker environment variables
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbPassword = Environment.GetEnvironmentVariable("DB_SA_PASSWORD");

// TODO: test if the connection string is correct
var connectionString = $"Server={dbHost},1433;Database={dbName};User Id=sa;Password={dbPassword};TrustServerCertificate=True;";


// Add ApiContext with SQL Server database
builder.Services.AddDbContext<ApiContext>(options =>
    options.UseSqlServer(connectionString));


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Inicjalizacja danych w bazie
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<WeatherContext>();

    if (!context.WeatherForecasts.Any())
    {
        context.WeatherForecasts.AddRange(new List<WeatherForecast>
        {
            new WeatherForecast
            {
                Id = 1,
                Date = DateOnly.FromDateTime(DateTime.Now),
                TemperatureC = 25,
                Summary = "Sunny"
            },
            new WeatherForecast
            {
                Id = 2,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                TemperatureC = 18,
                Summary = "Cloudy"
            },
            new WeatherForecast
            {
                Id = 3,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                TemperatureC = 15,
                Summary = "Rainy"
            }
        });
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    /*
     Does not work with Docker container
    */
    //app.UseSwaggerUI(options =>
    //{
    //    options.SwaggerEndpoint("/openapi/v1.json", "ActiLink API");
    //});

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();