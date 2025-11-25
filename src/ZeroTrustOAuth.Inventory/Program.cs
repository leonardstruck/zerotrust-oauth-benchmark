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

builder.AddNpgsqlDbContext<InventoryDbContext>(ServiceNames.InventoryDb, configureDbContextOptions: options =>
{
    options.UseAsyncSeeding(async (context, _, cancellationToken) =>
    {
        await context.Set<Product>().SeedProductsAsync();

        await context.SaveChangesAsync(cancellationToken);
    });
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


builder.Services.AddFastEndpoints()
    .SwaggerDocument(o =>
    {
        o.ShortSchemaNames = true;
        o.EndpointFilter = ep => ep.EndpointTags?.Contains("Internal") is null or false;
    })
    .SwaggerDocument(o =>
    {
        o.ShortSchemaNames = true;
        o.EndpointFilter = ep => ep.EndpointTags?.Contains("Internal") is true;
        o.AutoTagPathSegmentIndex = 2;
        o.DocumentSettings = s =>
        {
            s.DocumentName = "internal";
        };
    });


WebApplication app = builder.Build();
await app.EnsureCreated<InventoryDbContext>();

app
    .UseHttpsRedirection()
    .UseAuthentication().UseAuthorization()
    .UseFastEndpoints()
    .UseSwaggerGen(options =>
    {
        options.Path = "/openapi/{documentName}.json";
    });

app.MapDefaultEndpoints();


await app.RunAsync();