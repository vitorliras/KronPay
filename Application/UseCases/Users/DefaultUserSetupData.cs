namespace Application.UseCases.Users;

internal static class DefaultUserSetupData
{
    public static readonly string[] IncomeCategories =
    [
        "Salário",
        "Freelance/Renda Extra",
        "Rendimentos de Investimento",
        "Presente/Bônus",
        "Outras Receitas"
    ];

    public static readonly string[] InvestmentCategories =
    [
        "Renda Fixa",
        "Renda Variável",
        "Fundos de Investimento",
        "Criptomoedas",
        "Previdência Privada"
    ];

    public static readonly Dictionary<string, string[]> ExpenseCategories = new()
    {
        ["Alimentação"] = ["Mercado", "Restaurante", "Delivery"],
        ["Transporte"] = ["Combustível", "Transporte Público", "Aplicativo (Uber/99)"],
        ["Moradia"] = ["Aluguel", "Contas (Água/Luz/Internet)", "Manutenção"],
        ["Lazer"] = ["Streaming/Cinema", "Viagem", "Hobbies"],
        ["Saúde"] = ["Farmácia", "Consultas", "Plano de Saúde"]
    };

    public static readonly string[] PaymentMethods =
    [
        "Dinheiro",
        "Cartão de Débito",
        "Cartão de Crédito",
        "Pix",
        "Transferência Bancária"
    ];
}
