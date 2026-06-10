using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubliSport.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class IncreasePricingNotesForLandingProforma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PricingNotes",
                table: "Orders",
                type: "character varying(6000)",
                maxLength: 6000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PricingNotes",
                table: "Orders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(6000)",
                oldMaxLength: 6000,
                oldNullable: true);
        }
    }
}
