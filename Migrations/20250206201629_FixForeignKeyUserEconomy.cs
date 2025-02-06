using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FallVerseBotV2.Migrations
{
    /// <inheritdoc />
    public partial class FixForeignKeyUserEconomy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserEconomy_UserRecord_Id",
                table: "UserEconomy");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserEconomy",
                newName: "UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_UserEconomy_Id",
                table: "UserEconomy",
                newName: "IX_UserEconomy_UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserEconomy_UserRecord_UserId1",
                table: "UserEconomy",
                column: "UserId1",
                principalTable: "UserRecord",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserEconomy_UserRecord_UserId1",
                table: "UserEconomy");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                table: "UserEconomy",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_UserEconomy_UserId1",
                table: "UserEconomy",
                newName: "IX_UserEconomy_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserEconomy_UserRecord_Id",
                table: "UserEconomy",
                column: "Id",
                principalTable: "UserRecord",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
