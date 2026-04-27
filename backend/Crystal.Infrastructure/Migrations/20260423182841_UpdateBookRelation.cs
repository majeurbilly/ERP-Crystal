using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crystal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Items_ProductId",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryLines_Items_ProductId",
                table: "InventoryLines");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Items_ProductId",
                table: "Receipts");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Receipts",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Receipts_ProductId",
                table: "Receipts",
                newName: "IX_Receipts_ItemId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "InventoryLines",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryLines_ProductId",
                table: "InventoryLines",
                newName: "IX_InventoryLines_ItemId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Books",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Books_ProductId",
                table: "Books",
                newName: "IX_Books_ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Items_ItemId",
                table: "Books",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryLines_Items_ItemId",
                table: "InventoryLines",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Items_ItemId",
                table: "Receipts",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Items_ItemId",
                table: "Books");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryLines_Items_ItemId",
                table: "InventoryLines");

            migrationBuilder.DropForeignKey(
                name: "FK_Receipts_Items_ItemId",
                table: "Receipts");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "Receipts",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Receipts_ItemId",
                table: "Receipts",
                newName: "IX_Receipts_ProductId");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "InventoryLines",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryLines_ItemId",
                table: "InventoryLines",
                newName: "IX_InventoryLines_ProductId");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "Books",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Books_ItemId",
                table: "Books",
                newName: "IX_Books_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Items_ProductId",
                table: "Books",
                column: "ProductId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryLines_Items_ProductId",
                table: "InventoryLines",
                column: "ProductId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Receipts_Items_ProductId",
                table: "Receipts",
                column: "ProductId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
