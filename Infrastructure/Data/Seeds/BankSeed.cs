using Domain.Entities.banks;

namespace KronPay.Infra.Data.Seeds
{
    public static class BankSeed
    {
        public static List<Bank> Data
        {
            get
            {
                var id = 1;

                return
                [
                    new Bank(id++, "99Pay", "#ef294b", "https://cdn.pluggy.ai/assets/connector-icons/804.png", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Ágora Investimentos", "#296fa7", "https://www.agorainvest.com.br/images/OpenFinance/agora.svg", string.Empty, "INVESTMENT", true),
                    new Bank(id++, "ASA", "#2b56c0", "https://cdn.pluggy.ai/assets/connector-icons/883.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Avenue", "#000000", "https://cdn.pluggy.ai/assets/connector-icons/230.svg", string.Empty, "INVESTMENT", true),
                    new Bank(id++, "Banco Bmg", "#fa6300", "https://www.bancobmg.com.br/data/files/8C/A2/7F/0A/FBA318104A94D208970BE9C2/bmg_open_finance.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Banco BRB", "#003D7C", "https://cdn.pluggy.ai/assets/connector-icons/682.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Banco Digio", "#ff6776", "https://static.digio.com.br/media/logo_81e95ae2ab.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Banco do Brasil", "#1194F6", "https://www.bb.com.br/docs/pub/inst/img/LogoBB.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Banco do Nordeste", "#ef294b", "https://cdn.pluggy.ai/assets/connector-icons/671.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Banco Mercantil", "#ef294b", "https://cdn.pluggy.ai/assets/connector-icons/742.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Banco PAN", "#02afff", "https://cdn.pluggy.ai/assets/connector-icons/657.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Banco Sofisa", "#004e46", "https://www.sofisa.com.br/openbanking/logo_sofisa.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Banrisul", "#0B45E4", "https://banrisul.com.br/bob/data/Simbolo-Banrisul.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Bradesco", "#e5173f", "https://banco.bradesco/open-finance/logo/icones_vetorial-pf.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Bradesco Cartões", "#e5173f", "https://cdn.pluggy.ai/assets/connector-icons/203.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "BTG Pactual", "#66768F", "https://banking-public-prd.s3.sa-east-1.amazonaws.com/open-finance/logo/btgbanking/btgbanking.svg", string.Empty, "INVESTMENT", true),
                    new Bank(id++, "BV", "#223AD2", "https://www.bv.com.br/site/resources/open-finance/logo-bv.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "C6 Bank", "#FFE45C", "https://cdn.pluggy.ai/assets/connector-icons/726.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Caixa Econômica Federal", "#296fa7", "https://consentimento.openbanking.caixa.gov.br/assets/images/logomarca_caixa.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Cora", "#f51b81", "https://cdn.pluggy.ai/assets/connector-icons/250.svg", string.Empty, "BUSINESS_BANK", true),
                    new Bank(id++, "Conta Simples", "#2DCC68", "https://cdn.pluggy.ai/assets/connector-icons/250.svg", string.Empty, "BUSINESS_BANK", true),
                    new Bank(id++, "Crefisa", "#23b9e2", "https://cdn.pluggy.ai/assets/connector-icons/750.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Dock", "#ef294b", "https://cdn.pluggy.ai/assets/connector-icons/810.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Efí Bank", "#fb6910", "https://cdn.pluggy.ai/assets/connector-icons/239.svg", string.Empty, "BUSINESS_BANK", true),
                    new Bank(id++, "EQI Investimentos", "#DB671F", "https://cdn.pluggy.ai/assets/connector-icons/271.svg", string.Empty, "INVESTMENT", true),
                    new Bank(id++, "Inter", "#fb6910", "https://cdn.pluggy.ai/assets/connector-icons/215.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Itaú", "#EC7000", "https://www.itau.com.br/assets/dam/publisher/07_itau_empresas/13_open_banking/logos_regulatorio_bacen/opb_log_reg_bac_itau_img_01.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Mercado Pago", "#009ee3", "https://http2.mlstatic.com/frontend-assets/opb-logos/logo.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Nubank", "#8a0fbe", "https://nuapp.nubank.com.br/open-banking/logo.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "PagBank", "#ef294b", "https://cdn.pluggy.ai/assets/connector-icons/692.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "PicPay", "#238662", "https://picpay.s3.sa-east-1.amazonaws.com/openbanking/picpay-logo-icon-pf.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Rico Investimentos", "#ff5200", "https://cdn.pluggy.ai/assets/connector-icons/205.svg", string.Empty, "INVESTMENT", true),
                    new Bank(id++, "Safra", "#00003C", "https://storage.googleapis.com/inic-data/safra-pf.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Santander", "#cc0000", "https://cms.santander.com.br/sites/WPS/imagem/img-santander-chama/21-08-06_200409_P_santander_chama.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Sicoob", "#00AE9D", "https://sicoob-openbanking.s3.sa-east-1.amazonaws.com/logo-sicoob.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Sicredi", "#3FA110", "https://www.sicredi.com.br/openbanking/app/assets/images/shared/logo/logo_sicredi_512.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Stone", "#00A868", "https://cdn.pluggy.ai/assets/connector-icons/787.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Toro Investimentos", "#ef294b", "https://cdn.pluggy.ai/assets/connector-icons/796.svg", string.Empty, "INVESTMENT", true),
                    new Bank(id++, "Unicred", "#1e5c49", "https://www.unicred.com.br/logo.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "Wise", "#9fe870", "https://cdn.pluggy.ai/assets/connector-icons/291.svg", string.Empty, "PERSONAL_BANK", true),
                    new Bank(id++, "XP Investimentos", "#111111", "https://cdn.pluggy.ai/assets/connector-icons/202.svg", string.Empty, "INVESTMENT", true)
                ];
            }
        }
    }
}
