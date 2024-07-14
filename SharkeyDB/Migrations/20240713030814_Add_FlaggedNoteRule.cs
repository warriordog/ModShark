using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SharkeyDB.Migrations
{
    /// <inheritdoc />
    public partial class Add_FlaggedNoteRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ms_flagged_note",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    note_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    flagged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ms_flagged_note", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ms_queued_note",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    note_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ms_queued_note", x => x.id);
                    table.ForeignKey(
                        name: "FK_ms_queued_note_note_note_id",
                        column: x => x.note_id,
                        principalTable: "note",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
            
            // Populate existing data
            migrationBuilder.Sql(
                """
                insert into ms_queued_note (note_id)
                select id as note_id
                from "note"
                on conflict do nothing
                """
            );
            
            // Create trigger for new data
            migrationBuilder.Sql(
                """
                create or replace function add_note_to_modshark_queue()
                returns trigger as $$
                begin
                    insert into ms_queued_note (note_id)
                    values (new.id)
                    on conflict do nothing;
                    return new;
                end;
                $$ language plpgsql;
                """
            );
            migrationBuilder.Sql(
                """
                create trigger TRG_after_note_insert
                after insert on "note"
                for each row
                execute function add_note_to_modshark_queue();
                """
            );

            // Create indexes LAST to speed up migration
            // https://dba.stackexchange.com/questions/66182/index-creation-before-or-after-loading-data
            migrationBuilder.CreateIndex(
                name: "IX_ms_flagged_note_note_id",
                table: "ms_flagged_note",
                column: "note_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ms_queued_note_note_id",
                table: "ms_queued_note",
                column: "note_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove trigger
            migrationBuilder.Sql("drop trigger TRG_after_note_insert");
            migrationBuilder.Sql("drop function add_note_to_modshark_queue");
            
            migrationBuilder.DropTable(
                name: "ms_flagged_note");

            migrationBuilder.DropTable(
                name: "ms_queued_note");
        }
    }
}
