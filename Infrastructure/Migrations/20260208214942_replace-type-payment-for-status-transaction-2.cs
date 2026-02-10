using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class replacetypepaymentforstatustransaction2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "type_payment_method");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "type_transaction",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "status_transaction",
                columns: table => new
                {
                    code = table.Column<string>(type: "char(1)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_status_transaction", x => x.code);
                });

            migrationBuilder.InsertData(
                table: "status_transaction",
                columns: new[] { "code", "description" },
                values: new object[,]
                {
                    { "C", "Canceled" },
                    { "O", "Open" },
                    { "P", "Paid" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_status",
                table: "transaction",
                column: "status");

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_status_transaction_status",
                table: "transaction",
                column: "status",
                principalTable: "status_transaction",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_transaction_status_transaction_status",
                table: "transaction");

            migrationBuilder.DropTable(
                name: "status_transaction");

            migrationBuilder.DropIndex(
                name: "IX_transaction_status",
                table: "transaction");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "type_transaction",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

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

            migrationBuilder.InsertData(
                table: "type_payment_method",
                columns: new[] { "code", "description" },
                values: new object[,]
                {
                    { "P", "payment" },
                    { "R", "Receipt" }
                });
        }
    }
}
