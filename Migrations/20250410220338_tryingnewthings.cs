using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FallVerseBotV2.Migrations
{
    /// <inheritdoc />
    public partial class tryingnewthings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastGameDataRaw",
                table: "UserGameStats",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastGameDataRaw",
                table: "UserGameStats");
        }
    }
}
