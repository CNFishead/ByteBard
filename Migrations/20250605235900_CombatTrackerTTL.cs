using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FallVerseBotV2.Migrations
{
    /// <inheritdoc />
    public partial class CombatTrackerTTL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "CombatTrackers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateIndex(
                name: "IX_CombatTrackers_LastUpdatedAt",
                table: "CombatTrackers",
                column: "LastUpdatedAt");

            migrationBuilder.Sql(
                "CREATE EXTENSION IF NOT EXISTS pg_ttl; SELECT ttl_create_policy('CombatTrackers', 'LastUpdatedAt', INTERVAL '7 days');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "SELECT ttl_drop_policy('CombatTrackers', 'LastUpdatedAt');");

            migrationBuilder.DropIndex(
                name: "IX_CombatTrackers_LastUpdatedAt",
                table: "CombatTrackers");

            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "CombatTrackers");
        }
    }
}
