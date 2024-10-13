using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharkeyDB.Migrations
{
    /// <inheritdoc />
    public partial class ScanUpdatedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                create trigger TRG_after_user_update
                after update on "user"
                for each row
                execute function add_user_to_modshark_queue();
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop trigger TRG_after_user_update on \"user\";");
        }
    }
}
