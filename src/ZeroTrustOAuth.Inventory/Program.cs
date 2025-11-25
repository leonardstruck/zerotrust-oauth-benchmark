using FluentValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using ZeroTrustOAuth.Data.Extensions;
using ZeroTrustOAuth.Inventory.Features.Categories;
using ZeroTrustOAuth.Inventory.Infrastructure;
using ZeroTrustOAuth.ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.ExecuteWhenNotGenerating(_ =>
{
    builder.AddServiceDefaults();

    builder.AddNpgsqlDbContext<InventoryDbContext>(ServiceNames.InventoryDb, configureDbContextOptions: options =>
    {
        options.UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            if (context is not InventoryDbContext dbContext)
            {
                return;
            }

            await dbContext.Seed(cancellationToken);
        });
    });

    builder.AddDatabaseMigration<InventoryDbContext>();

    builder.Services.AddValidatorsFromAssemblyContaining<Program>();
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
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

app
    .UseHttpsRedirection()
    .UseAuthentication().UseAuthorization();

app.MapOpenApi();
app.MapDefaultEndpoints();

app.MapEndpoint<GetCategoriesEndpoint>();


await app.RunAsync();