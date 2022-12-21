using System;
using DemoModels;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemoBackend.Migrations
{
    /// <inheritdoc />
    public partial class proba2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "productext");

            migrationBuilder.AddColumn<ProductExt>(
                name: "ext",
                table: "product",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ext",
                table: "product");

            migrationBuilder.CreateTable(
                name: "productext",
                columns: table => new
                {
                    productid = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    minimumstock = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_productext", x => x.productid);
                    table.ForeignKey(
                        name: "fk_productext_product_productid",
                        column: x => x.productid,
                        principalTable: "product",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
