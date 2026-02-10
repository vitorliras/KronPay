using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class createtransactionsandchangepaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payment_method_type_payment_method_cod_type_payment_method",
                table: "payment_method");

            migrationBuilder.DropIndex(
                name: "IX_payment_method_cod_type_payment_method",
                table: "payment_method");

            migrationBuilder.DropColumn(
                name: "cod_type_payment_method",
                table: "payment_method");

            migrationBuilder.CreateTable(
                name: "transaction_groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "char(1)", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    installments = table.Column<short>(type: "smallint", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    transaction_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    cod_type_transaction = table.Column<string>(type: "char(1)", nullable: false),
                    status = table.Column<string>(type: "char(1)", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    category_item_id = table.Column<int>(type: "int", nullable: true),
                    transaction_group_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_groups_active",
                table: "transaction_groups",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_groups_user_id",
                table: "transaction_groups",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_transaction_date",
                table: "transactions",
                column: "transaction_date");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_transaction_group_id",
                table: "transactions",
                column: "transaction_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_user_id",
                table: "transactions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transaction_groups");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.AddColumn<string>(
                name: "cod_type_payment_method",
                table: "payment_method",
                type: "char(1)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_payment_method_cod_type_payment_method",
                table: "payment_method",
                column: "cod_type_payment_method");

            migrationBuilder.AddForeignKey(
                name: "FK_payment_method_type_payment_method_cod_type_payment_method",
                table: "payment_method",
                column: "cod_type_payment_method",
                principalTable: "type_payment_method",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
