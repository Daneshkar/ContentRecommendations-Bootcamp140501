using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmotionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignExperienceUserIdAndAddScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Experiences",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Experiences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Experiences_Score",
                table: "Experiences",
                sql: "[Score] BETWEEN 1 AND 5");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Experiences_Score",
                table: "Experiences");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "Experiences");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Experiences",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
