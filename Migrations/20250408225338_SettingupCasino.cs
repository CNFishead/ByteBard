using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FallVerseBotV2.Migrations
{
    /// <inheritdoc />
    public partial class SettingupCasino : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CasinoCurrencyId",
                table: "ServerSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServerSettings_CasinoCurrencyId",
                table: "ServerSettings",
                column: "CasinoCurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServerSettings_CurrencyType_CasinoCurrencyId",
                table: "ServerSettings",
                column: "CasinoCurrencyId",
                principalTable: "CurrencyType",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServerSettings_CurrencyType_CasinoCurrencyId",
                table: "ServerSettings");

            migrationBuilder.DropIndex(
                name: "IX_ServerSettings_CasinoCurrencyId",
                table: "ServerSettings");

            migrationBuilder.DropColumn(
                name: "CasinoCurrencyId",
                table: "ServerSettings");
        }
    }
}
