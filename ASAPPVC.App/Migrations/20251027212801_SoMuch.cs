using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASAPPVC.UI.Migrations
{
    /// <inheritdoc />
    public partial class SoMuch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductComponents_ProductId",
                table: "ProductComponents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CodeCounters",
                table: "CodeCounters");

            migrationBuilder.DropIndex(
                name: "IX_CodeCounters_CodeType_PeriodKey",
                table: "CodeCounters");

            migrationBuilder.DropColumn(
                name: "ImageBytes",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CodeCounters");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "ProductComponents",
                newName: "Unit");

            migrationBuilder.RenameColumn(
                name: "ImageContentType",
                table: "Components",
                newName: "Image_UploadedUtc");

            migrationBuilder.RenameColumn(
                name: "ImageBytes",
                table: "Components",
                newName: "Image_Thumb");

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Colour",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Image_ContentType",
                table: "Products",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Image_Data",
                table: "Products",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Image_Height",
                table: "Products",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Image_Length",
                table: "Products",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image_Sha256",
                table: "Products",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Image_Thumb",
                table: "Products",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Image_UploadedUtc",
                table: "Products",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Image_Width",
                table: "Products",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Material",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ComponentId1",
                table: "ProductComponents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityRequired",
                table: "ProductComponents",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Image_ContentType",
                table: "Components",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Image_Data",
                table: "Components",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Image_Height",
                table: "Components",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Image_Length",
                table: "Components",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Image_Sha256",
                table: "Components",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Image_Width",
                table: "Components",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Unit",
                table: "Components",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "PeriodKey",
                table: "CodeCounters",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CodeCounters",
                table: "CodeCounters",
                columns: new[] { "CodeType", "PeriodKey" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponents_ComponentId1",
                table: "ProductComponents",
                column: "ComponentId1");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponents_ProductId_ComponentId",
                table: "ProductComponents",
                columns: new[] { "ProductId", "ComponentId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductComponents_Components_ComponentId1",
                table: "ProductComponents",
                column: "ComponentId1",
                principalTable: "Components",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductComponents_Components_ComponentId1",
                table: "ProductComponents");

            migrationBuilder.DropIndex(
                name: "IX_ProductComponents_ComponentId1",
                table: "ProductComponents");

            migrationBuilder.DropIndex(
                name: "IX_ProductComponents_ProductId_ComponentId",
                table: "ProductComponents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CodeCounters",
                table: "CodeCounters");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Colour",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Image_ContentType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Image_Data",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Image_Height",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Image_Length",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Image_Sha256",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Image_Thumb",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Image_UploadedUtc",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Image_Width",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ComponentId1",
                table: "ProductComponents");

            migrationBuilder.DropColumn(
                name: "QuantityRequired",
                table: "ProductComponents");

            migrationBuilder.DropColumn(
                name: "Image_ContentType",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "Image_Data",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "Image_Height",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "Image_Length",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "Image_Sha256",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "Image_Width",
                table: "Components");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Components");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "ProductComponents",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "Image_UploadedUtc",
                table: "Components",
                newName: "ImageContentType");

            migrationBuilder.RenameColumn(
                name: "Image_Thumb",
                table: "Components",
                newName: "ImageBytes");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageBytes",
                table: "Products",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "Products",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "PeriodKey",
                table: "CodeCounters",
                type: "TEXT",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "CodeCounters",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CodeCounters",
                table: "CodeCounters",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponents_ProductId",
                table: "ProductComponents",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeCounters_CodeType_PeriodKey",
                table: "CodeCounters",
                columns: new[] { "CodeType", "PeriodKey" });
        }
    }
}
