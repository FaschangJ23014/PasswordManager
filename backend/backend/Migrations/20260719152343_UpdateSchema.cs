using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_passwords_users_UserId",
                schema: "public",
                table: "passwords");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "public",
                table: "passwords",
                newName: "\"UserId\"");

            migrationBuilder.RenameIndex(
                name: "IX_passwords_UserId",
                schema: "public",
                table: "passwords",
                newName: "IX_passwords_\"UserId\"");

            migrationBuilder.AddForeignKey(
                name: "FK_passwords_users_\"UserId\"",
                schema: "public",
                table: "passwords",
                column: "\"UserId\"",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_passwords_users_\"UserId\"",
                schema: "public",
                table: "passwords");

            migrationBuilder.RenameColumn(
                name: "\"UserId\"",
                schema: "public",
                table: "passwords",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_passwords_\"UserId\"",
                schema: "public",
                table: "passwords",
                newName: "IX_passwords_UserId");

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
    }
}
