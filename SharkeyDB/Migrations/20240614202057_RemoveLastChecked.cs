using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharkeyDB.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLastChecked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "last_checked",
                table: "ms_user",
                newName: "checked_at");
            
            migrationBuilder.DropColumn(
                name: "last_flagged",
                table: "ms_user");

            migrationBuilder.AddColumn<bool>(
                name: "is_flagged",
                table: "ms_user",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_flagged",
                table: "ms_user");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_flagged",
                table: "ms_user",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "checked_at",
                table: "ms_user",
                newName: "last_checked");
        }
    }
}
