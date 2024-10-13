using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharkeyDB.Migrations
{
    /// <inheritdoc />
    public partial class ScanUpdatedInstances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                create trigger TRG_after_instance_update
                after update on "instance"
                for each row
                execute function add_instance_to_modshark_queue();
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop trigger TRG_after_instance_update on \"instance\";");
        }
    }
}
