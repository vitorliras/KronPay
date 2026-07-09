using Shared.Localization;

namespace Application.Services.Assistant;

public sealed class AssistantTree
{
    public const string Root = "root";
    public const string Guide = "guide";
    public const string TopicTransactions = "topic-transactions";
    public const string TransactionsRecent = "transactions-recent";
    public const string TransactionsCategorySelect = "transactions-category-select";
    public const string TransactionsCategorySearch = "transactions-category-search";
    public const string TopicGoals = "topic-goals";
    public const string TopicCard = "topic-card";
    public const string TopicNotifications = "topic-notifications";
    public const string GoalsSelect = "goals-select";
    public const string CardSelect = "card-select";
    public const string NotificationsList = "notifications-list";

    private static readonly IReadOnlyDictionary<string, string> Parents = new Dictionary<string, string>
    {
        [Guide] = Root,
        [TopicTransactions] = Root,
        [TransactionsRecent] = TopicTransactions,
        [TransactionsCategorySelect] = TopicTransactions,
        [TransactionsCategorySearch] = TransactionsCategorySelect,
        [TopicGoals] = Root,
        [TopicCard] = Root,
        [TopicNotifications] = Root,
        [GoalsSelect] = TopicGoals,
        [CardSelect] = TopicCard,
        [NotificationsList] = TopicNotifications,
    };

    public IReadOnlyList<AssistantTopic> Topics { get; } = new List<AssistantTopic>
    {
        new(TopicTransactions, MessageKeys.AssistantOptionTopicTransactions,
            new[]
            {
                "quanto gastei", "gasto total do mês", "quanto rolou de", "transações", "meus gastos",
                "quanto gastei com", "gastei em", "quanto eu gastei este mês", "extrato de gastos",
                "como estão os gastos", "como está o gasto de", "como estão os gastos de categoria",
                "como andam meus gastos", "gastos por categoria este mês", "quanto eu gastei em",
                "resumo de gastos", "quanto tá saindo de dinheiro", "pra onde tá indo meu dinheiro",
                "quais foram meus gastos", "últimas compras", "histórico de transações",
                "extrato do mês", "quanto eu já gastei", "minhas transações", "quanto eu paguei em",
                "quanto estão meus gastos de alimentação", "quanto está meu gasto de transporte",
                "quanto são meus gastos com lazer", "quanto é meu gasto com mercado",
                "quanto ficou meu gasto com farmácia", "quanto foi meu gasto em transporte esse mês",
                "meus gastos com alimentação estão em quanto", "quanto tá dando meu gasto de comida",
                "me mostra minhas últimas compras", "mostra minhas compras recentes",
                "quero ver meu extrato", "quanto eu desembolsei esse mês",
                "transactions", "spending", "expenses", "how much did i spend", "how are my expenses",
                "my spending this month", "spending summary", "where did my money go", "recent purchases",
                "how much are my expenses with", "what are my expenses on", "show me my recent purchases",
            }),
        new(TopicGoals, MessageKeys.AssistantOptionTopicGoals,
            new[]
            {
                "como está minha meta", "progresso do objetivo", "meus objetivos", "quanto falta pra bater a meta",
                "quanto já guardei para o objetivo", "acompanhar meta de economia",
                "como está minha meta de viagem", "como está meu objetivo de", "progresso da minha meta de",
                "como anda meu objetivo", "falta quanto pra minha meta", "minhas metas financeiras",
                "meus objetivos de economia", "quanto já economizei", "estou perto de bater minha meta",
                "quanto falta pra minha meta", "como estão meus objetivos",
                "quanto falta pra eu bater minha meta de economia", "falta pouco pra eu bater a meta",
                "estou perto de bater a minha meta", "quanto falta para eu atingir meu objetivo",
                "goals", "objectives", "progress", "how is my savings goal", "how is my goal progress",
                "my financial goals", "how much have i saved", "am i close to my goal",
                "what's my goal progress", "how close am i to my goal",
            }),
        new(TopicCard, MessageKeys.AssistantOptionTopicCard,
            new[]
            {
                "fatura do cartão", "quanto é a fatura", "cartão de crédito", "quanto devo no cartão",
                "quando vence a fatura", "fatura fechou em quanto", "limite do cartão", "gastos no cartão",
                "quanto gastei no cartão", "valor da fatura", "data de vencimento do cartão",
                "como está a fatura do meu cartão", "quanto tá minha fatura",
                "qual o limite do meu cartão", "qual o limite do meu cartão de crédito",
                "qual o limite do meu cartao de credito", "qual o limite do meu cartão de credito",
                "quanto de limite eu tenho no cartão", "qual é o limite disponível do meu cartão",
                "credit card", "invoice", "how much is my credit card bill", "when is my card due",
                "card statement", "credit card balance", "when does my invoice close",
                "when does my credit card statement close",
            }),
        new(TopicNotifications, MessageKeys.AssistantOptionTopicNotifications,
            new[]
            {
                "notificação", "tenho notificação pendente", "avisos", "minhas notificações",
                "tem algum alerta", "notificações não lidas", "avisos pendentes", "o que preciso saber",
                "tenho algum aviso", "quais são minhas notificações", "tem algo que eu precise ver",
                "notifications", "alerts", "do i have any notifications", "unread notifications",
                "pending alerts", "do i have any unread alerts", "any new alerts for me",
                "is there anything i need to know",
            }),
    };

    public IReadOnlyList<AssistantTopic> TransactionsSubTopics { get; } = new List<AssistantTopic>
    {
        new(TransactionsCategorySelect, MessageKeys.AssistantOptionTransactionsByCategory,
            new[]
            {
                "gastos por categoria", "quanto gastei em", "por categoria", "quanto gastei em alimentação",
                "quero ver por categoria", "detalhar por categoria",
                "spending by category", "by category", "category breakdown",
            }),
        new(TransactionsRecent, MessageKeys.AssistantOptionRecentTransactions,
            new[]
            {
                "últimas transações", "transações recentes", "minhas últimas compras", "o que eu comprei",
                "histórico recente",
                "recent transactions", "latest transactions", "recent purchases", "what did i buy",
            }),
    };

    public IReadOnlyList<GuideScreen> GuideScreens { get; } = new List<GuideScreen>
    {
        new("guide-dashboard", MessageKeys.AssistantGuideScreenDashboard, "/dashboard"),
        new("guide-transactions", MessageKeys.AssistantGuideScreenTransactions, "/transaction"),
        new("guide-reports", MessageKeys.AssistantGuideScreenReports, "/report"),
        new("guide-planning", MessageKeys.AssistantGuideScreenPlanning, "/planning"),
        new("guide-cards", MessageKeys.AssistantGuideScreenCards, "/credit-card/cards"),
        new("guide-goals", MessageKeys.AssistantGuideScreenGoals, "/goals"),
        new("guide-config", MessageKeys.AssistantGuideScreenConfig, "/config"),
    };

    public string? GetParent(string nodeId) => Parents.TryGetValue(nodeId, out var parent) ? parent : null;
}
