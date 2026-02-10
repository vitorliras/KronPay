using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changefksrestricts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_transactions",
                table: "transactions");

            migrationBuilder.RenameTable(
                name: "transactions",
                newName: "transaction");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_user_id",
                table: "transaction",
                newName: "IX_transaction_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_transaction_group_id",
                table: "transaction",
                newName: "IX_transaction_transaction_group_id");

            migrationBuilder.RenameIndex(
                name: "IX_transactions_transaction_date",
                table: "transaction",
                newName: "IX_transaction_transaction_date");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transaction",
                table: "transaction",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_cod_type_transaction",
                table: "transaction",
                column: "cod_type_transaction");

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_transaction_group_transaction_group_id",
                table: "transaction",
                column: "transaction_group_id",
                principalTable: "transaction_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_type_transaction_cod_type_transaction",
                table: "transaction",
                column: "cod_type_transaction",
                principalTable: "type_transaction",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_users_user_id",
                table: "transaction",
                column: "user_id",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_transaction_transaction_group_transaction_group_id",
                table: "transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_transaction_type_transaction_cod_type_transaction",
                table: "transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_transaction_users_user_id",
                table: "transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_transaction",
                table: "transaction");

            migrationBuilder.DropIndex(
                name: "IX_transaction_cod_type_transaction",
                table: "transaction");

            migrationBuilder.RenameTable(
                name: "transaction",
                newName: "transactions");

            migrationBuilder.RenameIndex(
                name: "IX_transaction_user_id",
                table: "transactions",
                newName: "IX_transactions_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_transaction_transaction_group_id",
                table: "transactions",
                newName: "IX_transactions_transaction_group_id");

            migrationBuilder.RenameIndex(
                name: "IX_transaction_transaction_date",
                table: "transactions",
                newName: "IX_transactions_transaction_date");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transactions",
                table: "transactions",
                column: "id");
        }
    }
}
