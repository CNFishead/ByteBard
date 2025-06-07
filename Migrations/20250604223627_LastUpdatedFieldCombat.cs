using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FallVerseBotV2.Migrations
{
    /// <inheritdoc />
    public partial class LastUpdatedFieldCombat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "CombatTrackers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_CombatTrackers_LastUpdatedAt",
                table: "CombatTrackers",
                column: "LastUpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CombatTrackers_LastUpdatedAt",
                table: "CombatTrackers");

            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "CombatTrackers");
        }
    }
}
