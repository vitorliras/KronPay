using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addGamificationEvaluationState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "consistency_counters",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    counter_key = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    current_streak = table.Column<int>(type: "int", nullable: false),
                    best_streak = table.Column<int>(type: "int", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consistency_counters", x => x.id);
                    table.ForeignKey(
                        name: "FK_consistency_counters_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "gamification_evaluation_runs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ran_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    users_processed = table.Column<int>(type: "int", nullable: false),
                    events_triggered = table.Column<int>(type: "int", nullable: false),
                    badges_unlocked = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gamification_evaluation_runs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_consistency_counters_user_id_counter_key",
                table: "consistency_counters",
                columns: new[] { "user_id", "counter_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consistency_counters");

            migrationBuilder.DropTable(
                name: "gamification_evaluation_runs");
        }
    }
}
