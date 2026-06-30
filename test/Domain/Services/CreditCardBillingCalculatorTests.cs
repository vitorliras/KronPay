using Domain.Entities.Card;
using Domain.Services.Card;
using Shouldly;

namespace Tests.Domain.Services;

public class CreditCardBillingCalculatorTests
{
    private readonly CreditCardBillingCalculator _sut = new();

    private static CreditCard Card(short closingDay, short dueDay)
        => new CreditCard(1, 1, 1, "Nubank", dueDay, closingDay, 5000m);

    [Fact]
    public void Compra_antes_do_fechamento_entra_na_fatura_do_mes()
    {
        var cycle = _sut.Resolve(Card(closingDay: 28, dueDay: 5), new DateTime(2026, 6, 10));

        cycle.ReferenceYear.ShouldBe((short)2026);
        cycle.ReferenceMonth.ShouldBe((short)6);
        cycle.ClosingDate.ShouldBe(new DateTime(2026, 6, 28));
        cycle.DueDate.ShouldBe(new DateTime(2026, 7, 5));
    }

    [Fact]
    public void Compra_no_dia_do_fechamento_entra_na_fatura_do_mes()
    {
        var cycle = _sut.Resolve(Card(28, 5), new DateTime(2026, 6, 28));

        cycle.ReferenceMonth.ShouldBe((short)6);
    }

    [Fact]
    public void Compra_apos_o_fechamento_vai_para_o_mes_seguinte()
    {
        var cycle = _sut.Resolve(Card(28, 5), new DateTime(2026, 6, 29));

        cycle.ReferenceMonth.ShouldBe((short)7);
        cycle.DueDate.ShouldBe(new DateTime(2026, 8, 5));
    }

    [Fact]
    public void Virada_de_ano()
    {
        var cycle = _sut.Resolve(Card(28, 5), new DateTime(2026, 12, 29));

        cycle.ReferenceYear.ShouldBe((short)2027);
        cycle.ReferenceMonth.ShouldBe((short)1);
    }

    [Fact]
    public void Fechamento_em_dia_inexistente_no_mes_e_ajustado()
    {
        // fechamento 31 + fevereiro -> clamp para o último dia do mês
        var cycle = _sut.Resolve(Card(31, 10), new DateTime(2026, 2, 5));

        cycle.ClosingDate.ShouldBe(new DateTime(2026, 2, 28));
    }
}
