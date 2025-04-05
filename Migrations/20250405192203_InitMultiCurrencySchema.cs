using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FallVerseBotV2.Migrations
{
    /// <inheritdoc />
    public partial class InitMultiCurrencySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrencyType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRecord",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerSettings",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DailyCurrencyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerSettings", x => x.GuildId);
                    table.ForeignKey(
                        name: "FK_ServerSettings_CurrencyType_DailyCurrencyId",
                        column: x => x.DailyCurrencyId,
                        principalTable: "CurrencyType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCurrencyBalance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyTypeId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCurrencyBalance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCurrencyBalance_CurrencyType_CurrencyTypeId",
                        column: x => x.CurrencyTypeId,
                        principalTable: "CurrencyType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCurrencyBalance_UserRecord_UserId",
                        column: x => x.UserId,
                        principalTable: "UserRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserEconomy",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CurrencyAmount = table.Column<int>(type: "integer", nullable: false),
                    LastClaimed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StreakCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEconomy", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserEconomy_UserRecord_UserId",
                        column: x => x.UserId,
                        principalTable: "UserRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServerSettings_DailyCurrencyId",
                table: "ServerSettings",
                column: "DailyCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCurrencyBalance_CurrencyTypeId",
                table: "UserCurrencyBalance",
                column: "CurrencyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCurrencyBalance_UserId_CurrencyTypeId",
                table: "UserCurrencyBalance",
                columns: new[] { "UserId", "CurrencyTypeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerSettings");

            migrationBuilder.DropTable(
                name: "UserCurrencyBalance");

            migrationBuilder.DropTable(
                name: "UserEconomy");

            migrationBuilder.DropTable(
                name: "CurrencyType");

            migrationBuilder.DropTable(
                name: "UserRecord");
        }
    }
}
