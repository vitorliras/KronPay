using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanningModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "planned_commitment",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    direction = table.Column<string>(type: "char(1)", nullable: false),
                    periodicity = table.Column<string>(type: "char(1)", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    deactivated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planned_commitment", x => x.id);
                    table.ForeignKey(
                        name: "FK_planned_commitment_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planned_commitment_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_planned_commitment_category_id",
                table: "planned_commitment",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_planned_commitment_user_id_active",
                table: "planned_commitment",
                columns: new[] { "user_id", "active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "planned_commitment");
        }
    }
}
