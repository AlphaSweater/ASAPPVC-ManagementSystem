using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASAPPVC.UI.Migrations
{
    /// <inheritdoc />
    public partial class SoMuch2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductComponents_Components_ComponentId1",
                table: "ProductComponents");

            migrationBuilder.DropIndex(
                name: "IX_ProductComponents_ComponentId1",
                table: "ProductComponents");

            migrationBuilder.DropColumn(
                name: "ComponentId1",
                table: "ProductComponents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ComponentId1",
                table: "ProductComponents",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductComponents_ComponentId1",
                table: "ProductComponents",
                column: "ComponentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductComponents_Components_ComponentId1",
                table: "ProductComponents",
                column: "ComponentId1",
                principalTable: "Components",
                principalColumn: "Id");
        }
    }
}
