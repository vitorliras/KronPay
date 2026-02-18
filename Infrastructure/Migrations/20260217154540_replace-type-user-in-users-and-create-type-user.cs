using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class replacetypeuserinusersandcreatetypeuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserType",
                table: "users",
                newName: "cod_user_type");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "LastAccessAt",
                table: "users",
                newName: "last_Access_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.AlterColumn<string>(
                name: "cod_user_type",
                table: "users",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "type_user",
                columns: table => new
                {
                    code = table.Column<string>(type: "char(1)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_type_user", x => x.code);
                });

            migrationBuilder.InsertData(
                table: "type_user",
                columns: new[] { "code", "description" },
                values: new object[,]
                {
                    { "A", "Admin" },
                    { "B", "Basic" },
                    { "C", "Corporate" },
                    { "P", "Premium" },
                    { "V", "VIP" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_cod_user_type",
                table: "users",
                column: "cod_user_type");

            migrationBuilder.AddForeignKey(
                name: "FK_users_type_user_cod_user_type",
                table: "users",
                column: "cod_user_type",
                principalTable: "type_user",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_type_user_cod_user_type",
                table: "users");

            migrationBuilder.DropTable(
                name: "type_user");

            migrationBuilder.DropIndex(
                name: "IX_users_cod_user_type",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "last_Access_at",
                table: "users",
                newName: "LastAccessAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "cod_user_type",
                table: "users",
                newName: "UserType");

            migrationBuilder.AlterColumn<int>(
                name: "UserType",
                table: "users",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");
        }
    }
}
