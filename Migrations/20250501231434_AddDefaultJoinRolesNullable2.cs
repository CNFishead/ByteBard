using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FallVerseBotV2.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultJoinRolesNullable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
         name: "WelcomeMessage",
         table: "ServerSettings",
         type: "text",
         nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WelcomeChannelId",
                table: "ServerSettings",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<List<ulong>>(
                name: "DefaultJoinRoleIds",
                table: "ServerSettings",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
          name: "WelcomeMessage",
          table: "ServerSettings");

            migrationBuilder.DropColumn(
                name: "WelcomeChannelId",
                table: "ServerSettings");

            migrationBuilder.DropColumn(
                name: "DefaultJoinRoleIds",
                table: "ServerSettings");
        }
    }
}
