using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SharkeyDB.Migrations
{
    /// <inheritdoc />
    public partial class AddFlaggedHostnameRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ms_flagged_user",
                table: "ms_flagged_user");

            migrationBuilder.AddColumn<int>(
                name: "id",
                table: "ms_flagged_user",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ms_flagged_user",
                table: "ms_flagged_user",
                column: "id");

            migrationBuilder.CreateTable(
                name: "ms_flagged_instance",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    instance_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    flagged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ms_flagged_instance", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ms_queued_instance",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    instance_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ms_queued_instance", x => x.id);
                    table.ForeignKey(
                        name: "FK_ms_queued_instance_instance_instance_id",
                        column: x => x.instance_id,
                        principalTable: "instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ms_flagged_user_user_id",
                table: "ms_flagged_user",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ms_flagged_instance_instance_id",
                table: "ms_flagged_instance",
                column: "instance_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ms_queued_instance_instance_id",
                table: "ms_queued_instance",
                column: "instance_id",
                unique: true);
            
            // Update previous trigger to handle conflicts
            migrationBuilder.Sql(
                """
                create or replace function add_user_to_modshark_queue()
                returns trigger as $$
                begin
                    insert into ms_queued_user (user_id)
                    values (new.id)
                    on conflict do nothing;
                    return new;
                end;
                $$ language plpgsql;
                """
            );
            
            // Populate existing data
            migrationBuilder.Sql(
                """
                insert into ms_queued_instance (instance_id)
                select id as instance_id
                from "instance"
                on conflict do nothing
                """
            );
            
            // Create trigger for new data
            migrationBuilder.Sql(
                """
                create or replace function add_instance_to_modshark_queue()
                returns trigger as $$
                begin
                    insert into ms_queued_instance (instance_id)
                    values (new.id)
                    on conflict do nothing;
                    return new;
                end;
                $$ language plpgsql;
                """
            );
            migrationBuilder.Sql(
                """
                create trigger TRG_after_instance_insert
                after insert on "instance"
                for each row
                execute function add_instance_to_modshark_queue();
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove trigger
            migrationBuilder.Sql("drop trigger TRG_after_instance_insert");
            migrationBuilder.Sql("drop function add_instance_to_modshark_queue");
            
            // Revert previous trigger
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
            
            migrationBuilder.DropTable(
                name: "ms_flagged_instance");

            migrationBuilder.DropTable(
                name: "ms_queued_instance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ms_flagged_user",
                table: "ms_flagged_user");

            migrationBuilder.DropIndex(
                name: "IX_ms_flagged_user_user_id",
                table: "ms_flagged_user");

            migrationBuilder.DropColumn(
                name: "id",
                table: "ms_flagged_user");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ms_flagged_user",
                table: "ms_flagged_user",
                column: "user_id");
        }
    }
}
