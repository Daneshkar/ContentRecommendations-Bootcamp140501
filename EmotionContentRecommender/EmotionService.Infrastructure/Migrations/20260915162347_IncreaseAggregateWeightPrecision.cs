using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmotionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseAggregateWeightPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "WeightValue",
                table: "ItemThemeWeights",
                type: "decimal(6,5)",
                precision: 6,
                scale: 5,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightValue",
                table: "ItemMoodWeights",
                type: "decimal(6,5)",
                precision: 6,
                scale: 5,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "WeightValue",
                table: "ItemThemeWeights",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,5)",
                oldPrecision: 6,
                oldScale: 5);

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightValue",
                table: "ItemMoodWeights",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,5)",
                oldPrecision: 6,
                oldScale: 5);
        }
    }
}
