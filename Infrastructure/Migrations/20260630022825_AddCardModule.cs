using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCardModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "card_invoice",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    credit_card_id = table.Column<int>(type: "int", nullable: false),
                    reference_year = table.Column<short>(type: "smallint", nullable: false),
                    reference_month = table.Column<short>(type: "smallint", nullable: false),
                    closing_date = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    due_date = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "char(1)", nullable: false),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    transaction_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_invoice", x => x.id);
                    table.ForeignKey(
                        name: "FK_card_invoice_credit_card_credit_card_id",
                        column: x => x.credit_card_id,
                        principalTable: "credit_card",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_invoice_transaction_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_invoice_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "card_purchase",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    credit_card_id = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    purchase_date = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    installments_count = table.Column<short>(type: "smallint", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    category_item_id = table.Column<int>(type: "int", nullable: true),
                    origin = table.Column<string>(type: "char(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    deactivated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_purchase", x => x.id);
                    table.ForeignKey(
                        name: "FK_card_purchase_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_purchase_category_item_category_item_id",
                        column: x => x.category_item_id,
                        principalTable: "category_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_purchase_credit_card_credit_card_id",
                        column: x => x.credit_card_id,
                        principalTable: "credit_card",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_purchase_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "card_installment",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    card_purchase_id = table.Column<int>(type: "int", nullable: false),
                    card_invoice_id = table.Column<int>(type: "int", nullable: false),
                    installment_number = table.Column<short>(type: "smallint", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "char(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_installment", x => x.id);
                    table.ForeignKey(
                        name: "FK_card_installment_card_invoice_card_invoice_id",
                        column: x => x.card_invoice_id,
                        principalTable: "card_invoice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_installment_card_purchase_card_purchase_id",
                        column: x => x.card_purchase_id,
                        principalTable: "card_purchase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_installment_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_card_installment_card_invoice_id",
                table: "card_installment",
                column: "card_invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_installment_card_purchase_id",
                table: "card_installment",
                column: "card_purchase_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_installment_user_id",
                table: "card_installment",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_invoice_credit_card_id",
                table: "card_invoice",
                column: "credit_card_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_invoice_transaction_id",
                table: "card_invoice",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_invoice_user_id_credit_card_id_reference_year_reference_month",
                table: "card_invoice",
                columns: new[] { "user_id", "credit_card_id", "reference_year", "reference_month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_card_purchase_category_id",
                table: "card_purchase",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_purchase_category_item_id",
                table: "card_purchase",
                column: "category_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_purchase_credit_card_id",
                table: "card_purchase",
                column: "credit_card_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_purchase_user_id_credit_card_id",
                table: "card_purchase",
                columns: new[] { "user_id", "credit_card_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_installment");

            migrationBuilder.DropTable(
                name: "card_invoice");

            migrationBuilder.DropTable(
                name: "card_purchase");
        }
    }
}
