using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FallVerseBotV2.Migrations
{
    /// <inheritdoc />
    public partial class CombatTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CombatTrackers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GameId = table.Column<string>(type: "text", nullable: false),
                    CreatedByUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentTurnIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombatTrackers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Combatants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TrackerId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Initiative = table.Column<int>(type: "integer", nullable: false),
                    DiscordUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combatants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Combatants_CombatTrackers_TrackerId",
                        column: x => x.TrackerId,
                        principalTable: "CombatTrackers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Combatants_TrackerId",
                table: "Combatants",
                column: "TrackerId");

            migrationBuilder.CreateIndex(
                name: "IX_CombatTrackers_GuildId_ChannelId_GameId",
                table: "CombatTrackers",
                columns: new[] { "GuildId", "ChannelId", "GameId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Combatants");

            migrationBuilder.DropTable(
                name: "CombatTrackers");
        }
    }
}
