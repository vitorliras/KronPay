using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addNotificationsAndPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_preferences",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    email_on_critical = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    email_on_important = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    email_on_informative = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_preferences", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_preferences_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false),
                    criticality = table.Column<int>(type: "int", nullable: false),
                    message_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    payload_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    related_entity_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    related_entity_id = table.Column<int>(type: "int", nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_resolved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    resolved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_archived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    archived_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_preferences_user_id",
                table: "notification_preferences",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id_is_archived_archived_at",
                table: "notifications",
                columns: new[] { "user_id", "is_archived", "archived_at" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id_is_archived_is_read",
                table: "notifications",
                columns: new[] { "user_id", "is_archived", "is_read" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id_is_resolved_criticality",
                table: "notifications",
                columns: new[] { "user_id", "is_resolved", "criticality" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_preferences");

            migrationBuilder.DropTable(
                name: "notifications");
        }
    }
}
