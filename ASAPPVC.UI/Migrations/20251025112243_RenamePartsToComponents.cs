using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASAPPVC.UI.Migrations
{
    /// <inheritdoc />
    public partial class RenamePartsToComponents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPart_Part_PartId",
                table: "ProductPart");

            migrationBuilder.RenameColumn(
                name: "PartId",
                table: "ProductPart",
                newName: "ComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductPart_PartId",
                table: "ProductPart",
                newName: "IX_ProductPart_ComponentId");

            migrationBuilder.RenameColumn(
                name: "PartCode",
                table: "Part",
                newName: "ComponentCode");

            migrationBuilder.RenameIndex(
                name: "IX_Part_PartCode",
                table: "Part",
                newName: "IX_Part_ComponentCode");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPart_Part_ComponentId",
                table: "ProductPart",
                column: "ComponentId",
                principalTable: "Part",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPart_Part_ComponentId",
                table: "ProductPart");

            migrationBuilder.RenameColumn(
                name: "ComponentId",
                table: "ProductPart",
                newName: "PartId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductPart_ComponentId",
                table: "ProductPart",
                newName: "IX_ProductPart_PartId");

            migrationBuilder.RenameColumn(
                name: "ComponentCode",
                table: "Part",
                newName: "PartCode");

            migrationBuilder.RenameIndex(
                name: "IX_Part_ComponentCode",
                table: "Part",
                newName: "IX_Part_PartCode");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPart_Part_PartId",
                table: "ProductPart",
                column: "PartId",
                principalTable: "Part",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
