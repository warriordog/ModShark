using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharkeyDB.Migrations
{
    /// <inheritdoc />
    public partial class FixFlagTableRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Commented migrations are intentionally suppressed because we are lying to entity framework.
            // Various open issues prevent any better solution.
            // - https://github.com/dotnet/efcore/issues/15854
            // - https://github.com/dotnet/efcore/issues/13146
            
            
            // migrationBuilder.AlterColumn<string>(
            //     name: "user_id",
            //     table: "ms_flagged_user",
            //     type: "character varying(32)",
            //     maxLength: 32,
            //     nullable: true,
            //     oldClrType: typeof(string),
            //     oldType: "character varying(32)",
            //     oldMaxLength: 32);
            //
            // migrationBuilder.AlterColumn<string>(
            //     name: "note_id",
            //     table: "ms_flagged_note",
            //     type: "character varying(32)",
            //     maxLength: 32,
            //     nullable: true,
            //     oldClrType: typeof(string),
            //     oldType: "character varying(32)",
            //     oldMaxLength: 32);
            //
            // migrationBuilder.AlterColumn<string>(
            //     name: "instance_id",
            //     table: "ms_flagged_instance",
            //     type: "character varying(32)",
            //     maxLength: 32,
            //     nullable: true,
            //     oldClrType: typeof(string),
            //     oldType: "character varying(32)",
            //     oldMaxLength: 32);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ms_queued_user_user_id",
                table: "ms_queued_user",
                column: "user_id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ms_queued_note_note_id",
                table: "ms_queued_note",
                column: "note_id");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ms_queued_instance_instance_id",
                table: "ms_queued_instance",
                column: "instance_id");

            // migrationBuilder.AddForeignKey(
            //     name: "FK_ms_flagged_instance_instance_instance_id",
            //     table: "ms_flagged_instance",
            //     column: "instance_id",
            //     principalTable: "instance",
            //     principalColumn: "id");
            //
            // migrationBuilder.AddForeignKey(
            //     name: "FK_ms_flagged_instance_ms_queued_instance_instance_id",
            //     table: "ms_flagged_instance",
            //     column: "instance_id",
            //     principalTable: "ms_queued_instance",
            //     principalColumn: "instance_id");
            //
            // migrationBuilder.AddForeignKey(
            //     name: "FK_ms_flagged_note_ms_queued_note_note_id",
            //     table: "ms_flagged_note",
            //     column: "note_id",
            //     principalTable: "ms_queued_note",
            //     principalColumn: "note_id");
            //
            // migrationBuilder.AddForeignKey(
            //     name: "FK_ms_flagged_note_note_note_id",
            //     table: "ms_flagged_note",
            //     column: "note_id",
            //     principalTable: "note",
            //     principalColumn: "id");
            //
            // migrationBuilder.AddForeignKey(
            //     name: "FK_ms_flagged_user_ms_queued_user_user_id",
            //     table: "ms_flagged_user",
            //     column: "user_id",
            //     principalTable: "ms_queued_user",
            //     principalColumn: "user_id");
            //
            // migrationBuilder.AddForeignKey(
            //     name: "FK_ms_flagged_user_user_user_id",
            //     table: "ms_flagged_user",
            //     column: "user_id",
            //     principalTable: "user",
            //     principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "FK_ms_flagged_instance_instance_instance_id",
            //     table: "ms_flagged_instance");
            //
            // migrationBuilder.DropForeignKey(
            //     name: "FK_ms_flagged_instance_ms_queued_instance_instance_id",
            //     table: "ms_flagged_instance");
            //
            // migrationBuilder.DropForeignKey(
            //     name: "FK_ms_flagged_note_ms_queued_note_note_id",
            //     table: "ms_flagged_note");
            //
            // migrationBuilder.DropForeignKey(
            //     name: "FK_ms_flagged_note_note_note_id",
            //     table: "ms_flagged_note");
            //
            // migrationBuilder.DropForeignKey(
            //     name: "FK_ms_flagged_user_ms_queued_user_user_id",
            //     table: "ms_flagged_user");
            //
            // migrationBuilder.DropForeignKey(
            //     name: "FK_ms_flagged_user_user_user_id",
            //     table: "ms_flagged_user");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ms_queued_user_user_id",
                table: "ms_queued_user");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ms_queued_note_note_id",
                table: "ms_queued_note");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ms_queued_instance_instance_id",
                table: "ms_queued_instance");

            // migrationBuilder.AlterColumn<string>(
            //     name: "user_id",
            //     table: "ms_flagged_user",
            //     type: "character varying(32)",
            //     maxLength: 32,
            //     nullable: false,
            //     defaultValue: "",
            //     oldClrType: typeof(string),
            //     oldType: "character varying(32)",
            //     oldMaxLength: 32,
            //     oldNullable: true);
            //
            // migrationBuilder.AlterColumn<string>(
            //     name: "note_id",
            //     table: "ms_flagged_note",
            //     type: "character varying(32)",
            //     maxLength: 32,
            //     nullable: false,
            //     defaultValue: "",
            //     oldClrType: typeof(string),
            //     oldType: "character varying(32)",
            //     oldMaxLength: 32,
            //     oldNullable: true);
            //
            // migrationBuilder.AlterColumn<string>(
            //     name: "instance_id",
            //     table: "ms_flagged_instance",
            //     type: "character varying(32)",
            //     maxLength: 32,
            //     nullable: false,
            //     defaultValue: "",
            //     oldClrType: typeof(string),
            //     oldType: "character varying(32)",
            //     oldMaxLength: 32,
            //     oldNullable: true);
        }
    }
}
