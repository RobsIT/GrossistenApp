using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrossistenApp.Migrations
{
    /// <inheritdoc />
    public partial class ProductReceiptmodelschanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Incoming",
                table: "ReceiptsTable");

            migrationBuilder.DropColumn(
                name: "ReceiptProductId",
                table: "ReceiptsTable");

            migrationBuilder.RenameColumn(
                name: "PersonName",
                table: "ReceiptsTable",
                newName: "WorkerName");

            migrationBuilder.RenameColumn(
                name: "OutgoingReceipt",
                table: "ReceiptsTable",
                newName: "showAsOutgoingReceipt");

            migrationBuilder.RenameColumn(
                name: "Outgoing",
                table: "ReceiptsTable",
                newName: "DateAndTimeCreated");

            migrationBuilder.RenameColumn(
                name: "IncomingReceipt",
                table: "ReceiptsTable",
                newName: "showAsIncomingReceipt");

            migrationBuilder.RenameColumn(
                name: "OnReceipt",
                table: "ProductsTable",
                newName: "showOnReceipt");

            migrationBuilder.RenameColumn(
                name: "InStock",
                table: "ProductsTable",
                newName: "showInStock");

            migrationBuilder.RenameColumn(
                name: "AvaibleToPurchase",
                table: "ProductsTable",
                newName: "showInAvailableToPurchase");

            migrationBuilder.AddColumn<int>(
                name: "ReceiptId",
                table: "ProductsTable",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiptId",
                table: "ProductsTable");

            migrationBuilder.RenameColumn(
                name: "showAsOutgoingReceipt",
                table: "ReceiptsTable",
                newName: "OutgoingReceipt");

            migrationBuilder.RenameColumn(
                name: "showAsIncomingReceipt",
                table: "ReceiptsTable",
                newName: "IncomingReceipt");

            migrationBuilder.RenameColumn(
                name: "WorkerName",
                table: "ReceiptsTable",
                newName: "PersonName");

            migrationBuilder.RenameColumn(
                name: "DateAndTimeCreated",
                table: "ReceiptsTable",
                newName: "Outgoing");

            migrationBuilder.RenameColumn(
                name: "showOnReceipt",
                table: "ProductsTable",
                newName: "OnReceipt");

            migrationBuilder.RenameColumn(
                name: "showInStock",
                table: "ProductsTable",
                newName: "InStock");

            migrationBuilder.RenameColumn(
                name: "showInAvailableToPurchase",
                table: "ProductsTable",
                newName: "AvaibleToPurchase");

            migrationBuilder.AddColumn<DateTime>(
                name: "Incoming",
                table: "ReceiptsTable",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceiptProductId",
                table: "ReceiptsTable",
                type: "int",
                nullable: true);
        }
    }
}
