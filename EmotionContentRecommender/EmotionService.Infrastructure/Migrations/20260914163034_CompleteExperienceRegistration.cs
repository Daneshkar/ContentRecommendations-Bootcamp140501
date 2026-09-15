using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmotionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompleteExperienceRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ExperienceThemes_UserWeight",
                table: "ExperienceThemes");

            migrationBuilder.DropIndex(
                name: "IX_Experiences_UserId",
                table: "Experiences");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ExperienceMoods_UserWeight",
                table: "ExperienceMoods");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ExperienceThemes_UserWeight",
                table: "ExperienceThemes",
                sql: "[UserWeight] >= 0.00 AND [UserWeight] <= 0.10");

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_UserId_MediaItemId",
                table: "Experiences",
                columns: new[] { "UserId", "MediaItemId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ExperienceMoods_UserWeight",
                table: "ExperienceMoods",
                sql: "[UserWeight] >= 0.00 AND [UserWeight] <= 0.10");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ExperienceThemes_UserWeight",
                table: "ExperienceThemes");

            migrationBuilder.DropIndex(
                name: "IX_Experiences_UserId_MediaItemId",
                table: "Experiences");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ExperienceMoods_UserWeight",
                table: "ExperienceMoods");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ExperienceThemes_UserWeight",
                table: "ExperienceThemes",
                sql: "[UserWeight] >= 1.00 AND [UserWeight] <= 5.00");

            migrationBuilder.CreateIndex(
                name: "IX_Experiences_UserId",
                table: "Experiences",
                column: "UserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ExperienceMoods_UserWeight",
                table: "ExperienceMoods",
                sql: "[UserWeight] >= 1.00 AND [UserWeight] <= 5.00");
        }
    }
}
