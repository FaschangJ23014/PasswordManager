using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Passwords",
                table: "Passwords");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "Passwords",
                newName: "passwords",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "Website",
                schema: "public",
                table: "passwords",
                newName: "\"Website\"");

            migrationBuilder.RenameColumn(
                name: "Username",
                schema: "public",
                table: "passwords",
                newName: "\"Username\"");

            migrationBuilder.RenameColumn(
                name: "EncryptedPassword",
                schema: "public",
                table: "passwords",
                newName: "\"EncryptedPassword\"");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "passwords",
                newName: "\"Id\"");

            migrationBuilder.AlterColumn<string>(
                name: "\"Website\"",
                schema: "public",
                table: "passwords",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "\"Username\"",
                schema: "public",
                table: "passwords",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "\"EncryptedPassword\"",
                schema: "public",
                table: "passwords",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "\"Id\"",
                schema: "public",
                table: "passwords",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                schema: "public",
                table: "passwords",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_passwords",
                schema: "public",
                table: "passwords",
                column: "\"Id\"");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_passwords_UserId",
                schema: "public",
                table: "passwords",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_passwords_users_UserId",
                schema: "public",
                table: "passwords",
                column: "UserId",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_passwords_users_UserId",
                schema: "public",
                table: "passwords");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");

            migrationBuilder.DropPrimaryKey(
                name: "PK_passwords",
                schema: "public",
                table: "passwords");

            migrationBuilder.DropIndex(
                name: "IX_passwords_UserId",
                schema: "public",
                table: "passwords");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "public",
                table: "passwords");

            migrationBuilder.RenameTable(
                name: "passwords",
                schema: "public",
                newName: "Passwords");

            migrationBuilder.RenameColumn(
                name: "\"Website\"",
                table: "Passwords",
                newName: "Website");

            migrationBuilder.RenameColumn(
                name: "\"Username\"",
                table: "Passwords",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "\"EncryptedPassword\"",
                table: "Passwords",
                newName: "EncryptedPassword");

            migrationBuilder.RenameColumn(
                name: "\"Id\"",
                table: "Passwords",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "Passwords",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Passwords",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EncryptedPassword",
                table: "Passwords",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Passwords",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Passwords",
                table: "Passwords",
                column: "Id");
        }
    }
}
