using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderJourneysAndOrderLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderJourneys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldState = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NewState = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransitionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InitiatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderJourneys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderJourneys_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PerformedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Source = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLogs_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderJourneys_InitiatedBy",
                table: "OrderJourneys",
                column: "InitiatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrderJourneys_NewState",
                table: "OrderJourneys",
                column: "NewState");

            migrationBuilder.CreateIndex(
                name: "IX_OrderJourneys_OldState",
                table: "OrderJourneys",
                column: "OldState");

            migrationBuilder.CreateIndex(
                name: "IX_OrderJourneys_OrderId",
                table: "OrderJourneys",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderJourneys_OrderId_NewState",
                table: "OrderJourneys",
                columns: new[] { "OrderId", "NewState" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderJourneys_OrderId_OldState",
                table: "OrderJourneys",
                columns: new[] { "OrderId", "OldState" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderJourneys_TransitionDate",
                table: "OrderJourneys",
                column: "TransitionDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLogs_ActionDate",
                table: "OrderLogs",
                column: "ActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLogs_ActionType",
                table: "OrderLogs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLogs_Errors_ActionDate",
                table: "OrderLogs",
                columns: new[] { "Level", "ActionDate" },
                filter: "\"Level\" = 'Error'");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLogs_Level",
                table: "OrderLogs",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLogs_OrderId",
                table: "OrderLogs",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLogs_OrderId_ActionDate",
                table: "OrderLogs",
                columns: new[] { "OrderId", "ActionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLogs_OrderId_ActionType",
                table: "OrderLogs",
                columns: new[] { "OrderId", "ActionType" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLogs_OrderId_Level",
                table: "OrderLogs",
                columns: new[] { "OrderId", "Level" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLogs_PerformedBy",
                table: "OrderLogs",
                column: "PerformedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderJourneys");

            migrationBuilder.DropTable(
                name: "OrderLogs");
        }
    }
}
