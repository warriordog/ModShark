using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SharkeyDB.Migrations
{
    /// <inheritdoc />
    public partial class CreateMSQueuedUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ms_queued_user",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ms_queued_user", x => x.id);
                    table.ForeignKey(
                        name: "FK_ms_queued_user_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ms_queued_user_user_id",
                table: "ms_queued_user",
                column: "user_id",
                unique: true);
            
            // Create trigger for new data
            migrationBuilder.Sql(
                """
                create or replace function add_user_to_modshark_queue()
                returns trigger as $$
                begin
                    insert into ms_queued_user (user_id)
                    values (new.id);
                    return new;
                end;
                $$ language plpgsql;
                """
            );
            migrationBuilder.Sql(
                """
                create trigger TRG_after_user_insert
                after insert on "user"
                for each row
                execute function add_user_to_modshark_queue();
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove trigger
            migrationBuilder.Sql("drop trigger TRG_after_user_insert on \"user\";");
            migrationBuilder.Sql("drop function add_user_to_modshark_queue;");
            
            migrationBuilder.DropTable(
                name: "ms_queued_user");
        }
    }
}
