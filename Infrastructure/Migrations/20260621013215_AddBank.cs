using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bank",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    primary_color = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    institution_url = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "bank",
                columns: new[] { "id", "active", "image_url", "institution_url", "name", "primary_color", "type" },
                values: new object[,]
                {
                    { 1, true, "", "https://cdn.pluggy.ai/assets/connector-icons/804.png", "99Pay", "#ef294b", "PERSONAL_BANK" },
                    { 2, true, "", "https://www.agorainvest.com.br/images/OpenFinance/agora.svg", "Ágora Investimentos", "#296fa7", "INVESTMENT" },
                    { 3, true, "", "https://cdn.pluggy.ai/assets/connector-icons/883.svg", "ASA", "#2b56c0", "PERSONAL_BANK" },
                    { 4, true, "", "https://cdn.pluggy.ai/assets/connector-icons/230.svg", "Avenue", "#000000", "INVESTMENT" },
                    { 5, true, "", "https://www.bancobmg.com.br/data/files/8C/A2/7F/0A/FBA318104A94D208970BE9C2/bmg_open_finance.svg", "Banco Bmg", "#fa6300", "PERSONAL_BANK" },
                    { 6, true, "", "https://cdn.pluggy.ai/assets/connector-icons/682.svg", "Banco BRB", "#003D7C", "PERSONAL_BANK" },
                    { 7, true, "", "https://static.digio.com.br/media/logo_81e95ae2ab.svg", "Banco Digio", "#ff6776", "PERSONAL_BANK" },
                    { 8, true, "", "https://www.bb.com.br/docs/pub/inst/img/LogoBB.svg", "Banco do Brasil", "#1194F6", "PERSONAL_BANK" },
                    { 9, true, "", "https://cdn.pluggy.ai/assets/connector-icons/671.svg", "Banco do Nordeste", "#ef294b", "PERSONAL_BANK" },
                    { 10, true, "", "https://cdn.pluggy.ai/assets/connector-icons/742.svg", "Banco Mercantil", "#ef294b", "PERSONAL_BANK" },
                    { 11, true, "", "https://cdn.pluggy.ai/assets/connector-icons/657.svg", "Banco PAN", "#02afff", "PERSONAL_BANK" },
                    { 12, true, "", "https://www.sofisa.com.br/openbanking/logo_sofisa.svg", "Banco Sofisa", "#004e46", "PERSONAL_BANK" },
                    { 13, true, "", "https://banrisul.com.br/bob/data/Simbolo-Banrisul.svg", "Banrisul", "#0B45E4", "PERSONAL_BANK" },
                    { 14, true, "", "https://banco.bradesco/open-finance/logo/icones_vetorial-pf.svg", "Bradesco", "#e5173f", "PERSONAL_BANK" },
                    { 15, true, "", "https://cdn.pluggy.ai/assets/connector-icons/203.svg", "Bradesco Cartões", "#e5173f", "PERSONAL_BANK" },
                    { 16, true, "", "https://banking-public-prd.s3.sa-east-1.amazonaws.com/open-finance/logo/btgbanking/btgbanking.svg", "BTG Pactual", "#66768F", "INVESTMENT" },
                    { 17, true, "", "https://www.bv.com.br/site/resources/open-finance/logo-bv.svg", "BV", "#223AD2", "PERSONAL_BANK" },
                    { 18, true, "", "https://cdn.pluggy.ai/assets/connector-icons/726.svg", "C6 Bank", "#FFE45C", "PERSONAL_BANK" },
                    { 19, true, "", "https://consentimento.openbanking.caixa.gov.br/assets/images/logomarca_caixa.svg", "Caixa Econômica Federal", "#296fa7", "PERSONAL_BANK" },
                    { 20, true, "", "https://cdn.pluggy.ai/assets/connector-icons/250.svg", "Cora", "#f51b81", "BUSINESS_BANK" },
                    { 21, true, "", "https://cdn.pluggy.ai/assets/connector-icons/250.svg", "Conta Simples", "#2DCC68", "BUSINESS_BANK" },
                    { 22, true, "", "https://cdn.pluggy.ai/assets/connector-icons/750.svg", "Crefisa", "#23b9e2", "PERSONAL_BANK" },
                    { 23, true, "", "https://cdn.pluggy.ai/assets/connector-icons/810.svg", "Dock", "#ef294b", "PERSONAL_BANK" },
                    { 24, true, "", "https://cdn.pluggy.ai/assets/connector-icons/239.svg", "Efí Bank", "#fb6910", "BUSINESS_BANK" },
                    { 25, true, "", "https://cdn.pluggy.ai/assets/connector-icons/271.svg", "EQI Investimentos", "#DB671F", "INVESTMENT" },
                    { 26, true, "", "https://cdn.pluggy.ai/assets/connector-icons/215.svg", "Inter", "#fb6910", "PERSONAL_BANK" },
                    { 27, true, "", "https://www.itau.com.br/assets/dam/publisher/07_itau_empresas/13_open_banking/logos_regulatorio_bacen/opb_log_reg_bac_itau_img_01.svg", "Itaú", "#EC7000", "PERSONAL_BANK" },
                    { 28, true, "", "https://http2.mlstatic.com/frontend-assets/opb-logos/logo.svg", "Mercado Pago", "#009ee3", "PERSONAL_BANK" },
                    { 29, true, "", "https://nuapp.nubank.com.br/open-banking/logo.svg", "Nubank", "#8a0fbe", "PERSONAL_BANK" },
                    { 30, true, "", "https://cdn.pluggy.ai/assets/connector-icons/692.svg", "PagBank", "#ef294b", "PERSONAL_BANK" },
                    { 31, true, "", "https://picpay.s3.sa-east-1.amazonaws.com/openbanking/picpay-logo-icon-pf.svg", "PicPay", "#238662", "PERSONAL_BANK" },
                    { 32, true, "", "https://cdn.pluggy.ai/assets/connector-icons/205.svg", "Rico Investimentos", "#ff5200", "INVESTMENT" },
                    { 33, true, "", "https://storage.googleapis.com/inic-data/safra-pf.svg", "Safra", "#00003C", "PERSONAL_BANK" },
                    { 34, true, "", "https://cms.santander.com.br/sites/WPS/imagem/img-santander-chama/21-08-06_200409_P_santander_chama.svg", "Santander", "#cc0000", "PERSONAL_BANK" },
                    { 35, true, "", "https://sicoob-openbanking.s3.sa-east-1.amazonaws.com/logo-sicoob.svg", "Sicoob", "#00AE9D", "PERSONAL_BANK" },
                    { 36, true, "", "https://www.sicredi.com.br/openbanking/app/assets/images/shared/logo/logo_sicredi_512.svg", "Sicredi", "#3FA110", "PERSONAL_BANK" },
                    { 37, true, "", "https://cdn.pluggy.ai/assets/connector-icons/787.svg", "Stone", "#00A868", "PERSONAL_BANK" },
                    { 38, true, "", "https://cdn.pluggy.ai/assets/connector-icons/796.svg", "Toro Investimentos", "#ef294b", "INVESTMENT" },
                    { 39, true, "", "https://www.unicred.com.br/logo.svg", "Unicred", "#1e5c49", "PERSONAL_BANK" },
                    { 40, true, "", "https://cdn.pluggy.ai/assets/connector-icons/291.svg", "Wise", "#9fe870", "PERSONAL_BANK" },
                    { 41, true, "", "https://cdn.pluggy.ai/assets/connector-icons/202.svg", "XP Investimentos", "#111111", "INVESTMENT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bank");
        }
    }
}
