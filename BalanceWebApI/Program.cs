using AccountServiceN;
using QueueService;
using Microsoft.OpenApi.Models; // Add this using directive
using Swashbuckle.AspNetCore.SwaggerGen; // Optional, but sometimes needed
using Swashbuckle.AspNetCore.SwaggerUI; // Add this using directive
// ^^^ Add this line to fix CS1061

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure services
builder.Services.AddSingleton<BankQueueService>(provider =>
    new BankQueueService(builder.Configuration.GetConnectionString("AzureStorage")));

builder.Services.AddScoped<AccountService>(provider =>
    new AccountService(
        builder.Configuration.GetConnectionString("BankDatabase"),
        builder.Configuration.GetConnectionString("AzureStorage")
    ));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
