using FluentValidation;

using ZeroTrustOAuth.Data.Extensions;
using ZeroTrustOAuth.Auth;
using ZeroTrustOAuth.Inventory.Features.Categories;
using ZeroTrustOAuth.Inventory.Features.Products;
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
    .AddZeroTrustAuthentication(builder, ServiceNames.Identity, "zerotrust-oauth", "inventory-api")
    .AddZeroTrustAuthorization();
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenApi();
app.MapDefaultEndpoints();

RouteGroupBuilder inventoryApi = app.MapGroup("/api/inventory");

app.MapEndpoint<GetCategoriesEndpoint>(inventoryApi);
app.MapEndpoint<GetCategoryByIdEndpoint>(inventoryApi);
app.MapEndpoint<CreateCategoryEndpoint>(inventoryApi);
app.MapEndpoint<UpdateCategoryEndpoint>(inventoryApi);
app.MapEndpoint<ActivateCategoryEndpoint>(inventoryApi);
app.MapEndpoint<DeactivateCategoryEndpoint>(inventoryApi);

app.MapEndpoint<GetProductsEndpoint>(inventoryApi);
app.MapEndpoint<GetProductByIdEndpoint>(inventoryApi);
app.MapEndpoint<CreateProductEndpoint>(inventoryApi);
app.MapEndpoint<UpdateProductEndpoint>(inventoryApi);


await app.RunAsync();