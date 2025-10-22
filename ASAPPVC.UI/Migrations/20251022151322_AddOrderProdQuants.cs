using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASAPPVC.UI.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderProdQuants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "OrderProduct",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OrderProduct_OrderID",
                table: "OrderProduct",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProduct_ProductID",
                table: "OrderProduct",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_CustomerID",
                table: "Order",
                column: "CustomerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Customer_CustomerID",
                table: "Order",
                column: "CustomerID",
                principalTable: "Customer",
                principalColumn: "CustomerID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderProduct_Order_OrderID",
                table: "OrderProduct",
                column: "OrderID",
                principalTable: "Order",
                principalColumn: "OrderID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderProduct_Product_ProductID",
                table: "OrderProduct",
                column: "ProductID",
                principalTable: "Product",
                principalColumn: "ProductID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Customer_CustomerID",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderProduct_Order_OrderID",
                table: "OrderProduct");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderProduct_Product_ProductID",
                table: "OrderProduct");

            migrationBuilder.DropIndex(
                name: "IX_OrderProduct_OrderID",
                table: "OrderProduct");

            migrationBuilder.DropIndex(
                name: "IX_OrderProduct_ProductID",
                table: "OrderProduct");

            migrationBuilder.DropIndex(
                name: "IX_Order_CustomerID",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "OrderProduct");
        }
    }
}
