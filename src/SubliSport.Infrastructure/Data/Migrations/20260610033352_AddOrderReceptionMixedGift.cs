using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubliSport.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderReceptionMixedGift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GiftOption",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MixedGarmentDetails",
                table: "Orders",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ReceivedAt",
                table: "Orders",
                column: "ReceivedAt");

            migrationBuilder.Sql("""
                UPDATE "Orders"
                SET "ReceivedAt" = "CreatedAt"
                WHERE "ReceivedAt" = '-infinity'::timestamptz OR "ReceivedAt" < '2000-01-01'::timestamptz;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_ReceivedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GiftOption",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MixedGarmentDetails",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "Orders");
        }
    }
}
