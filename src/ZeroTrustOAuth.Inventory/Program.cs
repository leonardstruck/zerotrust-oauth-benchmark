using Carter;

using FluentValidation;

using ZeroTrustOAuth.Data.Extensions;
using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.AddNpgsqlDbContext<InventoryDbContext>("inventorydb");
builder.AddMigration<InventoryDbContext>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddCarter();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapDefaultEndpoints();

var inventory = app.MapGroup("/api/inventory");
inventory.MapCarter();

await app.RunAsync();
