using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class replacecollunstransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Installments",
                table: "transaction",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_payment_method",
                table: "transaction",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "status_transaction",
                columns: new[] { "code", "description" },
                values: new object[] { "E", "Expired" });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_category_id",
                table: "transaction",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_category_item_id",
                table: "transaction",
                column: "category_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_id_payment_method",
                table: "transaction",
                column: "id_payment_method");

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_category_category_id",
                table: "transaction",
                column: "category_id",
                principalTable: "category",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_category_item_category_item_id",
                table: "transaction",
                column: "category_item_id",
                principalTable: "category_item",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_payment_method_id_payment_method",
                table: "transaction",
                column: "id_payment_method",
                principalTable: "payment_method",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_transaction_category_category_id",
                table: "transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_transaction_category_item_category_item_id",
                table: "transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_transaction_payment_method_id_payment_method",
                table: "transaction");

            migrationBuilder.DropIndex(
                name: "IX_transaction_category_id",
                table: "transaction");

            migrationBuilder.DropIndex(
                name: "IX_transaction_category_item_id",
                table: "transaction");

            migrationBuilder.DropIndex(
                name: "IX_transaction_id_payment_method",
                table: "transaction");

            migrationBuilder.DeleteData(
                table: "status_transaction",
                keyColumn: "code",
                keyValue: "E");

            migrationBuilder.DropColumn(
                name: "Installments",
                table: "transaction");

            migrationBuilder.DropColumn(
                name: "id_payment_method",
                table: "transaction");
        }
    }
}
