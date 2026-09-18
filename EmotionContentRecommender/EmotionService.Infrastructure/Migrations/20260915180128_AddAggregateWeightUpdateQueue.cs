using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmotionService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAggregateWeightUpdateQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingMediaItemWeightUpdates",
                columns: table => new
                {
                    MediaItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingMediaItemWeightUpdates", x => x.MediaItemId);
                    table.CheckConstraint("CK_PendingMediaItemWeightUpdates_Version", "[Version] > 0");
                    table.ForeignKey(
                        name: "FK_PendingMediaItemWeightUpdates_MediaItems_MediaItemId",
                        column: x => x.MediaItemId,
                        principalTable: "MediaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingMediaItemWeightUpdates_RequestedAt",
                table: "PendingMediaItemWeightUpdates",
                column: "RequestedAt");

            migrationBuilder.Sql(
                """
                INSERT INTO [PendingMediaItemWeightUpdates]
                    ([MediaItemId], [RequestedAt], [Version])
                SELECT
                    candidates.[MediaItemId], SYSUTCDATETIME(), 1
                FROM
                (
                    SELECT [MediaItemId] FROM [Experiences]
                    UNION
                    SELECT [MediaItemId] FROM [ItemMoodWeights]
                    UNION
                    SELECT [MediaItemId] FROM [ItemThemeWeights]
                ) AS candidates;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingMediaItemWeightUpdates");
        }
    }
}
