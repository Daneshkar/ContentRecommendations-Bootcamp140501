using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmotionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommendationQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MediaItems_ItemTypeId",
                table: "MediaItems");

            migrationBuilder.DropIndex(
                name: "IX_MediaItemGenres_GenreId",
                table: "MediaItemGenres");

            migrationBuilder.DropIndex(
                name: "IX_ItemThemeWeights_ThemeId",
                table: "ItemThemeWeights");

            migrationBuilder.DropIndex(
                name: "IX_ItemMoodWeights_MoodId",
                table: "ItemMoodWeights");

            migrationBuilder.CreateIndex(
                name: "IX_MediaItems_ItemTypeId_Status_Id",
                table: "MediaItems",
                columns: new[] { "ItemTypeId", "Status", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaItemGenres_GenreId_MediaItemId",
                table: "MediaItemGenres",
                columns: new[] { "GenreId", "MediaItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemThemeWeights_ThemeId_MediaItemId",
                table: "ItemThemeWeights",
                columns: new[] { "ThemeId", "MediaItemId" })
                .Annotation("SqlServer:Include", new[] { "WeightValue", "ExperienceCount" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemMoodWeights_MoodId_MediaItemId",
                table: "ItemMoodWeights",
                columns: new[] { "MoodId", "MediaItemId" })
                .Annotation("SqlServer:Include", new[] { "WeightValue", "ExperienceCount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MediaItems_ItemTypeId_Status_Id",
                table: "MediaItems");

            migrationBuilder.DropIndex(
                name: "IX_MediaItemGenres_GenreId_MediaItemId",
                table: "MediaItemGenres");

            migrationBuilder.DropIndex(
                name: "IX_ItemThemeWeights_ThemeId_MediaItemId",
                table: "ItemThemeWeights");

            migrationBuilder.DropIndex(
                name: "IX_ItemMoodWeights_MoodId_MediaItemId",
                table: "ItemMoodWeights");

            migrationBuilder.CreateIndex(
                name: "IX_MediaItems_ItemTypeId",
                table: "MediaItems",
                column: "ItemTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaItemGenres_GenreId",
                table: "MediaItemGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemThemeWeights_ThemeId",
                table: "ItemThemeWeights",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemMoodWeights_MoodId",
                table: "ItemMoodWeights",
                column: "MoodId");
        }
    }
}
