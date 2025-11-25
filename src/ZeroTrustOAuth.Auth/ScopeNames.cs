namespace ZeroTrustOAuth.Auth;

public static class ScopeNames
{
    public static class Inventory
    {
        public const string ProductRead = "inventory.product.read";
        public const string CatalogSearch = "inventory.catalog.search";
        public const string StockRead = "inventory.stock.read";
        public const string StockAdjust = "inventory.stock.adjust";
        public const string StockOverride = "inventory.stock.override";
        public const string ProductManage = "inventory.product.manage";
    }
}
