using Carter;

using FluentValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using ZeroTrustOAuth.Data.Extensions;
using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddKeycloakJwtBearer(ServiceNames.Identity, "zerotrust-oauth", options =>
    {
        options.Audience = "inventory-service";

        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
        }
    });

builder.Services.AddOpenApi("v1");
builder.Services.AddOpenApi("internal", options =>
{
    options.ShouldInclude = ep => ep.GroupName == "internal";
});


builder.Services.AddAuthorization();

// Simplified OpenAPI registration (no transformer now)
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<InventoryDbContext>("inventorydb");
builder.AddMigration<InventoryDbContext>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddCarter();

WebApplication app = builder.Build();

app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();


await app.RunAsync();