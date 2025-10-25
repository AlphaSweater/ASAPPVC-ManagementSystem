using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASAPPVC.UI.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeCounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "Part",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CodeCounters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodeType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PeriodKey = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    LastNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeCounters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CodeCounters_CodeType_PeriodKey",
                table: "CodeCounters",
                columns: new[] { "CodeType", "PeriodKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeCounters");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "Part");
        }
    }
}
