using FastEndpoints;
using FastEndpoints.Swagger;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using ZeroTrustOAuth.Data.Extensions;
using ZeroTrustOAuth.Inventory.Data;
using ZeroTrustOAuth.Inventory.Data.Seeding;
using ZeroTrustOAuth.Inventory.Domain.Products;
using ZeroTrustOAuth.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.SwaggerDocument(o =>
{
    o.EndpointFilter = ep => ep.EndpointTags?.Contains("internal") is null or false;
});

builder.Services.SwaggerDocument(o =>
{
    o.EndpointFilter = ep => ep.EndpointTags?.Contains("internal") is true;
    o.DocumentSettings = s =>
    {
        s.DocumentName = "internal";
    };
});

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

builder.Services.AddAuthorization();

builder.AddNpgsqlDbContext<InventoryDbContext>(ServiceNames.InventoryDb, configureDbContextOptions: options =>
{
    options.UseAsyncSeeding(async (context, _, cancellationToken) =>
    {
        await context.Set<Product>().SeedProductsAsync();

        await context.SaveChangesAsync(cancellationToken);
    });
});

builder.Services.AddFastEndpoints();

WebApplication app = builder.Build();
await app.EnsureCreated<InventoryDbContext>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseFastEndpoints();

app.UseSwaggerGen(options =>
{
    options.Path = "/openapi/{documentName}.json";
});

app.MapDefaultEndpoints();


await app.RunAsync();