using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LatestChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReferenceId",
                table: "Orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "OrderLoyaltyTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExternalTransactionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLoyaltyTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PaidDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TransactionReference = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderPayments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStockReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityReserved = table.Column<int>(type: "integer", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ExternalReservationId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStockReservations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ReferenceId",
                table: "Orders",
                column: "ReferenceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderLoyaltyTransactions_ExternalTransactionId",
                table: "OrderLoyaltyTransactions",
                column: "ExternalTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLoyaltyTransactions_OrderId",
                table: "OrderLoyaltyTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLoyaltyTransactions_TransactionDate",
                table: "OrderLoyaltyTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLoyaltyTransactions_TransactionType",
                table: "OrderLoyaltyTransactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPayments_OrderId",
                table: "OrderPayments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPayments_Status",
                table: "OrderPayments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPayments_TransactionReference",
                table: "OrderPayments",
                column: "TransactionReference");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStockReservations_ExpirationDate",
                table: "OrderStockReservations",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStockReservations_ExternalReservationId",
                table: "OrderStockReservations",
                column: "ExternalReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStockReservations_OrderId",
                table: "OrderStockReservations",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStockReservations_ProductId",
                table: "OrderStockReservations",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStockReservations_ReservationDate",
                table: "OrderStockReservations",
                column: "ReservationDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStockReservations_Status",
                table: "OrderStockReservations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderLoyaltyTransactions");

            migrationBuilder.DropTable(
                name: "OrderPayments");

            migrationBuilder.DropTable(
                name: "OrderStockReservations");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ReferenceId",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceId",
                table: "Orders",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);
        }
    }
}
