using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASAPPVC.UI.Migrations
{
    /// <inheritdoc />
    public partial class BigChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductComponents",
                table: "ProductComponents");

            migrationBuilder.DropIndex(
                name: "IX_ProductComponents_ProductId_ComponentId",
                table: "ProductComponents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderProducts",
                table: "OrderProducts");

            migrationBuilder.DropIndex(
                name: "IX_OrderProducts_OrderId",
                table: "OrderProducts");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ProductComponents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "OrderProducts");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentAmount",
                table: "Components",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductComponents",
                table: "ProductComponents",
                columns: new[] { "ProductId", "ComponentId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderProducts",
                table: "OrderProducts",
                columns: new[] { "OrderId", "ProductId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductComponents",
                table: "ProductComponents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderProducts",
                table: "OrderProducts");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ProductComponents",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "OrderProducts",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "CurrentAmount",
                table: "Components",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductComponents",
                table: "ProductComponents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderProducts",
                table: "OrderProducts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponents_ProductId_ComponentId",
                table: "ProductComponents",
                columns: new[] { "ProductId", "ComponentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderProducts_OrderId",
                table: "OrderProducts",
                column: "OrderId");
        }
    }
}
