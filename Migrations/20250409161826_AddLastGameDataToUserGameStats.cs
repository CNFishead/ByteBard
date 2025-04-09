using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FallVerseBotV2.Migrations
{
    /// <inheritdoc />
    public partial class AddLastGameDataToUserGameStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Dictionary<string, object>>(
                name: "LastGameData",
                table: "UserGameStats",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastGameData",
                table: "UserGameStats");
        }
    }
}
