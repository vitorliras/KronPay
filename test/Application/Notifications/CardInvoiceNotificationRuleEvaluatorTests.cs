using Application.Notifications;
using Application.Notifications.Rules;
using Domain.Entities.Card;
using Domain.Enums.Notifications;
using Domain.Interfaces.Card;
using Moq;
using Shouldly;

namespace Tests.Application.Notifications;

public class CardInvoiceNotificationRuleEvaluatorTests
{
    private static readonly DateTime Today = new(2026, 10, 17);
    private readonly Mock<ICardInvoiceRepository> _repo = new();

    private static CardInvoice Invoice(int id, DateTime due, decimal total)
    {
        var invoice = new CardInvoice(1, 10, (short)due.Year, (short)due.Month, due.AddDays(-7), due);
        invoice.AddAmount(total);
        typeof(CardInvoice).GetProperty("Id")!.SetValue(invoice, id);
        return invoice;
    }

    private async Task<IReadOnlyList<NotificationCandidate>> Evaluate(CardInvoice invoice, Dictionary<int, decimal> pending)
    {
        _repo.Setup(r => r.GetByUserAsync(1)).ReturnsAsync(new List<CardInvoice> { invoice });
        _repo.Setup(r => r.GetPendingAmountsByUserAsync(1)).ReturnsAsync(pending);

        return await new CardInvoiceNotificationRuleEvaluator(_repo.Object).EvaluateAsync(1, Today);
    }

    [Fact]
    public async Task Fatura_vencida_com_valor_pendente_gera_alerta_critico_com_o_valor_pendente()
    {
        var invoice = Invoice(5, new DateTime(2026, 10, 10), 500m);

        var candidates = await Evaluate(invoice, new Dictionary<int, decimal> { [5] = 120m });

        var overdue = candidates.Single(c => c.Type == NotificationType.CardInvoiceOverdue);
        overdue.Criticality.ShouldBe(NotificationCriticality.Critical);
        overdue.Args["amount"].ShouldBe(120m.ToString("F2"));
    }

    [Fact]
    public async Task Fatura_vencida_sem_valor_pendente_nao_gera_alerta()
    {
        var invoice = Invoice(5, new DateTime(2026, 10, 10), 1075.25m);

        (await Evaluate(invoice, new Dictionary<int, decimal>())).ShouldBeEmpty();
    }
}
