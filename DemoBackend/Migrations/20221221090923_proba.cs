using System;
using System.Collections.Generic;
using DemoModels;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoBackend.Migrations
{
    /// <inheritdoc />
    public partial class proba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventorystock");

            migrationBuilder.AddColumn<List<InventoryStock>>(
                name: "stocks",
                table: "product",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "stocks",
                table: "product");

            migrationBuilder.CreateTable(
                name: "inventorystock",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    productid = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<double>(type: "double precision", nullable: false),
                    storeid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inventorystock", x => x.id);
                    table.ForeignKey(
                        name: "fk_inventorystock_product_productid",
                        column: x => x.productid,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_inventorystock_productid",
                table: "inventorystock",
                column: "productid");
        }
    }
}
