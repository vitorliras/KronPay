using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class createclassconfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "users",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "users",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "users",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Cpf",
                table: "users",
                newName: "cpf");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Username",
                table: "users",
                newName: "IX_users_username");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "users",
                newName: "IX_users_email");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Cpf",
                table: "users",
                newName: "IX_users_cpf");

            migrationBuilder.RenameColumn(
                name: "descricao",
                table: "type_transaction",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "codigo",
                table: "type_transaction",
                newName: "code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    cod_type_transaction = table.Column<string>(type: "char(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.id);
                    table.ForeignKey(
                        name: "FK_category_type_transaction_cod_type_transaction",
                        column: x => x.cod_type_transaction,
                        principalTable: "type_transaction",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_category_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "credit_card",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    due_day = table.Column<short>(type: "smallint", nullable: false),
                    closing_day = table.Column<short>(type: "smallint", nullable: false),
                    credit_limit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credit_card", x => x.id);
                    table.ForeignKey(
                        name: "FK_credit_card_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "type_payment_method",
                columns: table => new
                {
                    code = table.Column<string>(type: "char(1)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_type_payment_method", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "category_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_category_item_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_method",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(440)", maxLength: 440, nullable: false),
                    cod_type_payment_method = table.Column<string>(type: "char(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_method", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_method_type_payment_method_cod_type_payment_method",
                        column: x => x.cod_type_payment_method,
                        principalTable: "type_payment_method",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_method_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "type_payment_method",
                columns: new[] { "code", "description" },
                values: new object[,]
                {
                    { "P", "payment" },
                    { "R", "Receipt" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_cod_type_transaction",
                table: "category",
                column: "cod_type_transaction");

            migrationBuilder.CreateIndex(
                name: "IX_category_user_id_description",
                table: "category",
                columns: new[] { "user_id", "description" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_category_item_category_id",
                table: "category_item",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_credit_card_user_id_description",
                table: "credit_card",
                columns: new[] { "user_id", "description" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_method_cod_type_payment_method",
                table: "payment_method",
                column: "cod_type_payment_method");

            migrationBuilder.CreateIndex(
                name: "IX_payment_method_user_id_description",
                table: "payment_method",
                columns: new[] { "user_id", "description" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_item");

            migrationBuilder.DropTable(
                name: "credit_card");

            migrationBuilder.DropTable(
                name: "payment_method");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "type_payment_method");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "Users",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Users",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "cpf",
                table: "Users",
                newName: "Cpf");

            migrationBuilder.RenameIndex(
                name: "IX_users_username",
                table: "Users",
                newName: "IX_Users_Username");

            migrationBuilder.RenameIndex(
                name: "IX_users_email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.RenameIndex(
                name: "IX_users_cpf",
                table: "Users",
                newName: "IX_Users_Cpf");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "type_transaction",
                newName: "descricao");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "type_transaction",
                newName: "codigo");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");
        }
    }
}
