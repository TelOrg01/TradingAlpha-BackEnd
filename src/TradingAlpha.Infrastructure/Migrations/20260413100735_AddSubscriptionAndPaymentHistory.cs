using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingAlpha.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionAndPaymentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PlanName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "INR"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PaymentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OrderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "INR"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GatewayOrderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GatewayPaymentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GatewaySignature = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RawResponse = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentHistory_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PaymentHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistory_GatewayOrderId",
                table: "PaymentHistory",
                column: "GatewayOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistory_SubscriptionId",
                table: "PaymentHistory",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentHistory_UserId",
                table: "PaymentHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_OrderId",
                table: "Subscriptions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentHistory");

            migrationBuilder.DropTable(
                name: "Subscriptions");
        }
    }
}
