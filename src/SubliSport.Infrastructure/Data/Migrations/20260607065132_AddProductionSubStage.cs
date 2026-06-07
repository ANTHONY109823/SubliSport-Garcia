using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SubliSport.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionSubStage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductionSubStage",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "Orders" SET "ProductionSubStage" = 1 WHERE "Status" = 5 AND "ProductionSubStage" = 0;
                UPDATE "Orders" SET "ProductionSubStage" = 3 WHERE "Status" = 6 AND "ProductionSubStage" = 0;
                UPDATE "Orders" SET "ProductionSubStage" = 5 WHERE "Status" = 7 AND "ProductionSubStage" = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductionSubStage",
                table: "Orders");
        }
    }
}
