using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addIdBankInCreditCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "bank_id",
                table: "credit_card",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_credit_card_bank_id",
                table: "credit_card",
                column: "bank_id");

            migrationBuilder.AddForeignKey(
                name: "FK_credit_card_bank_bank_id",
                table: "credit_card",
                column: "bank_id",
                principalTable: "bank",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_credit_card_bank_bank_id",
                table: "credit_card");

            migrationBuilder.DropIndex(
                name: "IX_credit_card_bank_id",
                table: "credit_card");

            migrationBuilder.DropColumn(
                name: "bank_id",
                table: "credit_card");
        }
    }
}
