using Application.Planning;
using Domain.Entities.Card;
using Domain.Enums.Planning;
using Domain.Services.Card;
using Shouldly;

namespace Tests.Application.Planning;

public class PurchaseFlowBuilderTests
{
    private readonly PurchaseFlowBuilder _sut = new(new CreditCardBillingCalculator());

    [Fact]
    public void A_vista_gera_um_unico_fluxo_na_data()
    {
        var flows = _sut.Build(null, 300m, new DateTime(2026, 6, 10), installment: false, installmentsCount: 1);

        flows.ShouldHaveSingleItem();
        flows[0].Direction.ShouldBe(FlowDirection.Outflow);
        flows[0].Amount.ShouldBe(300m);
        flows[0].CompetenceDate.ShouldBe(new DateTime(2026, 6, 10));
        flows[0].Origin.ShouldBe(FlowOrigin.Simulation);
    }

    [Fact]
    public void Parcelado_sem_cartao_distribui_mes_a_mes()
    {
        var flows = _sut.Build(null, 300m, new DateTime(2026, 6, 10), installment: true, installmentsCount: 3);

        flows.Count.ShouldBe(3);
        flows[0].CompetenceDate.ShouldBe(new DateTime(2026, 6, 10));
        flows[1].CompetenceDate.ShouldBe(new DateTime(2026, 7, 10));
        flows[2].CompetenceDate.ShouldBe(new DateTime(2026, 8, 10));
        flows.Sum(f => f.Amount).ShouldBe(300m);
    }

    [Fact]
    public void Parcelado_com_cartao_posiciona_nas_faturas_pelo_vencimento()
    {
        var card = new CreditCard(1, 1, 1, "Nubank", (short)5, (short)28, 5000m);

        var flows = _sut.Build(card, 300m, new DateTime(2026, 6, 10), installment: true, installmentsCount: 3);

        flows.Count.ShouldBe(3);
        flows[0].CompetenceDate.ShouldBe(new DateTime(2026, 7, 5));
        flows[1].CompetenceDate.ShouldBe(new DateTime(2026, 8, 5));
        flows[2].CompetenceDate.ShouldBe(new DateTime(2026, 9, 5));
    }
}
