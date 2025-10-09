using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASAPPVC.UI.Migrations
{
    /// <inheritdoc />
    public partial class changedPkForPartProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OrderProductID",
                table: "ProductPart",
                newName: "ProductPartID");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "ProductPart",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ImageContentType",
                table: "Part",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<byte[]>(
                name: "ImageBytes",
                table: "Part",
                type: "BLOB",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "BLOB");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPart_PartID",
                table: "ProductPart",
                column: "PartID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPart_ProductID",
                table: "ProductPart",
                column: "ProductID");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPart_Part_PartID",
                table: "ProductPart",
                column: "PartID",
                principalTable: "Part",
                principalColumn: "PartID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPart_Product_ProductID",
                table: "ProductPart",
                column: "ProductID",
                principalTable: "Product",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPart_Part_PartID",
                table: "ProductPart");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPart_Product_ProductID",
                table: "ProductPart");

            migrationBuilder.DropIndex(
                name: "IX_ProductPart_PartID",
                table: "ProductPart");

            migrationBuilder.DropIndex(
                name: "IX_ProductPart_ProductID",
                table: "ProductPart");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ProductPart");

            migrationBuilder.RenameColumn(
                name: "ProductPartID",
                table: "ProductPart",
                newName: "OrderProductID");

            migrationBuilder.AlterColumn<string>(
                name: "ImageContentType",
                table: "Part",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "ImageBytes",
                table: "Part",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0],
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldNullable: true);
        }
    }
}
