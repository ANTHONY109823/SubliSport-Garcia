using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubliSport.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionOptionsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ClientOwnFabric",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ConfectionRosterDetails",
                table: "Orders",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludesIgv",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludesLaserCut",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientOwnFabric",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ConfectionRosterDetails",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IncludesIgv",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IncludesLaserCut",
                table: "Orders");
        }
    }
}
