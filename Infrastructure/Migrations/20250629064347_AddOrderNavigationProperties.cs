using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderPayments_OrderId",
                table: "OrderPayments");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPayments_OrderId",
                table: "OrderPayments",
                column: "OrderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLoyaltyTransactions_Orders_OrderId",
                table: "OrderLoyaltyTransactions",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderPayments_Orders_OrderId",
                table: "OrderPayments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStockReservations_Orders_OrderId",
                table: "OrderStockReservations",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLoyaltyTransactions_Orders_OrderId",
                table: "OrderLoyaltyTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderPayments_Orders_OrderId",
                table: "OrderPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderStockReservations_Orders_OrderId",
                table: "OrderStockReservations");

            migrationBuilder.DropIndex(
                name: "IX_OrderPayments_OrderId",
                table: "OrderPayments");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPayments_OrderId",
                table: "OrderPayments",
                column: "OrderId");
        }
    }
}
