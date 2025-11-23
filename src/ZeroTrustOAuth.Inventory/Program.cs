using Carter;
using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();

// Add database context with Aspire PostgreSQL integration
builder.AddNpgsqlDbContext<InventoryDbContext>("inventorydb");

// Add Carter for automatic feature discovery
builder.Services.AddCarter();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapDefaultEndpoints();

// Carter automatically discovers and maps all ICarterModule implementations
var inventory = app.MapGroup("/api/inventory");
inventory.MapCarter();

await app.RunAsync();
