using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubliSport.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionAcceptedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ProductionAcceptedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductionAcceptedByUserId",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Orders" o
                SET "ProductionAcceptedAt" = h."ChangedAt",
                    "ProductionAcceptedByUserId" = h."ChangedByUserId"
                FROM (
                    SELECT DISTINCT ON ("OrderId") "OrderId", "ChangedAt", "ChangedByUserId"
                    FROM "OrderStatusHistories"
                    WHERE "Comment" ILIKE '%aceptado por producción%'
                       OR "Comment" ILIKE '%aceptado por impresión%'
                    ORDER BY "OrderId", "ChangedAt" DESC
                ) h
                WHERE o."Id" = h."OrderId" AND o."ProductionAcceptedAt" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductionAcceptedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductionAcceptedByUserId",
                table: "Orders");
        }
    }
}
