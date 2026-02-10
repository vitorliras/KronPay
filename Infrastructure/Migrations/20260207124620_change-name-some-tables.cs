using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changenamesometables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_transaction_groups",
                table: "transaction_groups");

            migrationBuilder.RenameTable(
                name: "transaction_groups",
                newName: "transaction_group");

            migrationBuilder.RenameIndex(
                name: "IX_transaction_groups_user_id",
                table: "transaction_group",
                newName: "IX_transaction_group_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_transaction_groups_active",
                table: "transaction_group",
                newName: "IX_transaction_group_active");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transaction_group",
                table: "transaction_group",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_transaction_group",
                table: "transaction_group");

            migrationBuilder.RenameTable(
                name: "transaction_group",
                newName: "transaction_groups");

            migrationBuilder.RenameIndex(
                name: "IX_transaction_group_user_id",
                table: "transaction_groups",
                newName: "IX_transaction_groups_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_transaction_group_active",
                table: "transaction_groups",
                newName: "IX_transaction_groups_active");

            migrationBuilder.AddPrimaryKey(
                name: "PK_transaction_groups",
                table: "transaction_groups",
                column: "id");
        }
    }
}
