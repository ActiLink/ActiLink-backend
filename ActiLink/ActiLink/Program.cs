using ActiLink.Repositories;
using ActiLink;
using Microsoft.EntityFrameworkCore;
using System;
using ActiLink.Model;

var builder = WebApplication.CreateBuilder(args);



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