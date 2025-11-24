using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ZeroTrustOAuth.Inventory.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    QuantityInStock = table.Column<int>(type: "integer", nullable: false),
                    ReorderLevel = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SupplierId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "Name", "QuantityInStock", "ReorderLevel", "Sku", "SupplierId", "UpdatedAt" },
                values: new object[,]
                {
                    { "1", "Electronics", new DateTime(2024, 10, 25, 0, 0, 0, 0, DateTimeKind.Utc), "High-performance laptop for business", "Laptop", 25, 10, "LAP-001", "SUP-001", new DateTime(2024, 10, 25, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "2", "Electronics", new DateTime(2024, 11, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Ergonomic wireless mouse", "Wireless Mouse", 150, 50, "MOU-001", "SUP-002", new DateTime(2024, 11, 4, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "3", "Furniture", new DateTime(2024, 11, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Comfortable ergonomic office chair", "Office Chair", 8, 10, "CHR-001", "SUP-003", new DateTime(2024, 11, 9, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "4", "Electronics", new DateTime(2024, 10, 30, 0, 0, 0, 0, DateTimeKind.Utc), "4K Ultra HD monitor", "Monitor 27\"", 40, 15, "MON-001", "SUP-001", new DateTime(2024, 10, 30, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "5", "Office Supplies", new DateTime(2024, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc), "LED desk lamp with adjustable brightness", "Desk Lamp", 5, 20, "LMP-001", "SUP-002", new DateTime(2024, 11, 14, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                table: "Products",
                column: "Sku",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
