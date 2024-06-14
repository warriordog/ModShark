using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharkeyDB.Migrations
{
    /// <inheritdoc />
    public partial class CorrectColumnCasing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MSUsers",
                table: "MSUsers");

            migrationBuilder.RenameTable(
                name: "MSUsers",
                newName: "ms_user");

            migrationBuilder.RenameColumn(
                name: "LastFlagged",
                table: "ms_user",
                newName: "last_flagged");

            migrationBuilder.RenameColumn(
                name: "LastChecked",
                table: "ms_user",
                newName: "last_checked");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ms_user",
                newName: "user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ms_user",
                table: "ms_user",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ms_user",
                table: "ms_user");

            migrationBuilder.RenameTable(
                name: "ms_user",
                newName: "MSUsers");

            migrationBuilder.RenameColumn(
                name: "last_flagged",
                table: "MSUsers",
                newName: "LastFlagged");

            migrationBuilder.RenameColumn(
                name: "last_checked",
                table: "MSUsers",
                newName: "LastChecked");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "MSUsers",
                newName: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MSUsers",
                table: "MSUsers",
                column: "UserId");
        }
    }
}
