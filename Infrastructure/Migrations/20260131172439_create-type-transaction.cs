using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class createtypetransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "type_transaction",
                columns: table => new
                {
                    codigo = table.Column<string>(type: "char(1)", nullable: false),
                    descricao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_type_transaction", x => x.codigo);
                });

            migrationBuilder.InsertData(
                table: "type_transaction",
                columns: new[] { "codigo", "descricao" },
                values: new object[,]
                {
                    { "E", "Expense" },
                    { "I", "Income" },
                    { "V", "Investment" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "type_transaction");
        }
    }
}
