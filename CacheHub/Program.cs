using CacheHub.Configuration;
using CacheHub.EndPoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add custom application services
builder.Services.AddServices();

// Register application-specific endpoints
builder.Services.AddApplicationEndPoints();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map application-specific endpoints
app.MapApplicationEndpoints();

// Map endpoints from registered IEndPoint implementations
app.MapEndPoints();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
