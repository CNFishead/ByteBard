using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FallVerseBotV2.Migrations
{
    /// <inheritdoc />
    public partial class rollingback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Dictionary<string, JsonElement>>(
                name: "LastGameData",
                table: "UserGameStats",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb",
                oldClrType: typeof(string),
                oldType: "jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LastGameData",
                table: "UserGameStats",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(Dictionary<string, JsonElement>),
                oldType: "jsonb",
                oldDefaultValueSql: "'{}'::jsonb");

            migrationBuilder.AddColumn<Dictionary<string, JsonElement>>(
                name: "LastGameData1",
                table: "UserGameStats",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");
        }
    }
}
