using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharkeyDB.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceMSUserWithMSFlaggedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ms_user");

            migrationBuilder.CreateTable(
                name: "ms_flagged_user",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    flagged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ms_flagged_user", x => x.user_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ms_flagged_user");

            migrationBuilder.CreateTable(
                name: "ms_user",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_flagged = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ms_user", x => x.user_id);
                });
        }
    }
}
