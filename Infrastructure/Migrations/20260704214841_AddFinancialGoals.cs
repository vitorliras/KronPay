using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category_budget_goals",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    monthly_limit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    priority = table.Column<int>(type: "int", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    deactivated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_budget_goals", x => x.id);
                    table.ForeignKey(
                        name: "FK_category_budget_goals_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "financial_goals",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    target_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    current_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    target_date = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    priority = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    previous_attempt_goal_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_goals", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_budget_goals_category_id",
                table: "category_budget_goals",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_category_budget_goals_user_id_category_id",
                table: "category_budget_goals",
                columns: new[] { "user_id", "category_id" });

            migrationBuilder.CreateIndex(
                name: "IX_financial_goals_user_id_status",
                table: "financial_goals",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_budget_goals");

            migrationBuilder.DropTable(
                name: "financial_goals");
        }
    }
}
