using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class provideragnosticcolumntypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_type_user_cod_user_type",
                table: "users");

            migrationBuilder.DropForeignKey(
                name: "FK_category_type_transaction_cod_type_transaction",
                table: "category");

            migrationBuilder.DropForeignKey(
                name: "FK_transaction_type_transaction_cod_type_transaction",
                table: "transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_transaction_status_transaction_status",
                table: "transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_type_user",
                table: "type_user");

            migrationBuilder.DropPrimaryKey(
                name: "PK_type_transaction",
                table: "type_transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_status_transaction",
                table: "status_transaction");

            migrationBuilder.AlterColumn<string>(
                name: "cod_user_type",
                table: "users",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "type_user",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "type_transaction",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "transaction_group",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "transaction",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AlterColumn<string>(
                name: "cod_type_transaction",
                table: "transaction",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "status_transaction",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AlterColumn<string>(
                name: "periodicity",
                table: "planned_commitment",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AlterColumn<string>(
                name: "direction",
                table: "planned_commitment",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AlterColumn<string>(
                name: "cod_type_transaction",
                table: "category",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AlterColumn<string>(
                name: "origin",
                table: "card_purchase",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "card_invoice",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "card_installment",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(1)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_type_user",
                table: "type_user",
                column: "code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_type_transaction",
                table: "type_transaction",
                column: "code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_status_transaction",
                table: "status_transaction",
                column: "code");

            migrationBuilder.AddForeignKey(
                name: "FK_users_type_user_cod_user_type",
                table: "users",
                column: "cod_user_type",
                principalTable: "type_user",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_category_type_transaction_cod_type_transaction",
                table: "category",
                column: "cod_type_transaction",
                principalTable: "type_transaction",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_type_transaction_cod_type_transaction",
                table: "transaction",
                column: "cod_type_transaction",
                principalTable: "type_transaction",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_users_type_user_cod_user_type",
                table: "users");

            migrationBuilder.DropForeignKey(
                name: "FK_category_type_transaction_cod_type_transaction",
                table: "category");

            migrationBuilder.DropForeignKey(
                name: "FK_transaction_type_transaction_cod_type_transaction",
                table: "transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_transaction_status_transaction_status",
                table: "transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_type_user",
                table: "type_user");

            migrationBuilder.DropPrimaryKey(
                name: "PK_type_transaction",
                table: "type_transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_status_transaction",
                table: "status_transaction");

            migrationBuilder.AlterColumn<string>(
                name: "cod_user_type",
                table: "users",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "type_user",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "type_transaction",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "transaction_group",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "transaction",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "cod_type_transaction",
                table: "transaction",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "code",
                table: "status_transaction",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "periodicity",
                table: "planned_commitment",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "direction",
                table: "planned_commitment",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "cod_type_transaction",
                table: "category",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "origin",
                table: "card_purchase",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "card_invoice",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "card_installment",
                type: "char(1)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1);

            migrationBuilder.AddPrimaryKey(
                name: "PK_type_user",
                table: "type_user",
                column: "code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_type_transaction",
                table: "type_transaction",
                column: "code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_status_transaction",
                table: "status_transaction",
                column: "code");

            migrationBuilder.AddForeignKey(
                name: "FK_users_type_user_cod_user_type",
                table: "users",
                column: "cod_user_type",
                principalTable: "type_user",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_category_type_transaction_cod_type_transaction",
                table: "category",
                column: "cod_type_transaction",
                principalTable: "type_transaction",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_type_transaction_cod_type_transaction",
                table: "transaction",
                column: "cod_type_transaction",
                principalTable: "type_transaction",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_transaction_status_transaction_status",
                table: "transaction",
                column: "status",
                principalTable: "status_transaction",
                principalColumn: "code",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
