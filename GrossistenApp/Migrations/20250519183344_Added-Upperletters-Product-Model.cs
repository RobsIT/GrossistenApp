using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrossistenApp.Migrations
{
    /// <inheritdoc />
    public partial class AddedUpperlettersProductModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "showOnReceipt",
                table: "ProductsTable",
                newName: "ShowOnReceipt");

            migrationBuilder.RenameColumn(
                name: "showInStock",
                table: "ProductsTable",
                newName: "ShowInStock");

            migrationBuilder.RenameColumn(
                name: "showInAvailableToPurchase",
                table: "ProductsTable",
                newName: "ShowInAvailableToPurchase");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShowOnReceipt",
                table: "ProductsTable",
                newName: "showOnReceipt");

            migrationBuilder.RenameColumn(
                name: "ShowInStock",
                table: "ProductsTable",
                newName: "showInStock");

            migrationBuilder.RenameColumn(
                name: "ShowInAvailableToPurchase",
                table: "ProductsTable",
                newName: "showInAvailableToPurchase");
        }
    }
}
