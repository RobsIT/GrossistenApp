using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrossistenApp.Migrations
{
    /// <inheritdoc />
    public partial class FirstMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductsTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArticleNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvaibleToPurchase = table.Column<bool>(type: "bit", nullable: false),
                    InStock = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptsTable",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Incoming = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Outgoing = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IncomingReceipt = table.Column<bool>(type: "bit", nullable: false),
                    OutgoingReceipt = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptsTable", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductsTable");

            migrationBuilder.DropTable(
                name: "ReceiptsTable");
        }
    }
}
