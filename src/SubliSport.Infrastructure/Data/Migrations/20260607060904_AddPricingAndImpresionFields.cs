using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SubliSport.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingAndImpresionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedConfectionCost",
                table: "Orders",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedExtraCost",
                table: "Orders",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedFabricCost",
                table: "Orders",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedFabricRipCost",
                table: "Orders",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedLaserCost",
                table: "Orders",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedPrintPressCost",
                table: "Orders",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedTotal",
                table: "Orders",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ChargeAmount",
                table: "Orders",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FabricMeters",
                table: "Orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FabricMetersRip",
                table: "Orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FabricTypeId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FabricTypeName",
                table: "Orders",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FabricTypeRipId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FabricTypeRipName",
                table: "Orders",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludesConfection",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PricingNotes",
                table: "Orders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PricingUpdatedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ServiceOnlyPrintPress",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PricingConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JsonData = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingConfigurations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingConfigurations");

            migrationBuilder.DropColumn(
                name: "CalculatedConfectionCost",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CalculatedExtraCost",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CalculatedFabricCost",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CalculatedFabricRipCost",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CalculatedLaserCost",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CalculatedPrintPressCost",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CalculatedTotal",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ChargeAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FabricMeters",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FabricMetersRip",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FabricTypeId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FabricTypeName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FabricTypeRipId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FabricTypeRipName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IncludesConfection",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PricingNotes",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PricingUpdatedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ServiceOnlyPrintPress",
                table: "Orders");
        }
    }
}
