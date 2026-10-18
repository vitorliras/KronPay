using Domain.Entities.Card;
using Domain.Entities.Transactions;
using Shouldly;

namespace Tests.Domain.Entities;

public class CardInvoiceTests
{
    private static CardInvoice Paid()
    {
        var invoice = new CardInvoice(1, 10, 2026, 10, new DateTime(2026, 10, 1), new DateTime(2026, 10, 10));
        invoice.AddAmount(100m);
        invoice.Pay(new Transaction(1, 100m, DateTime.UtcNow, "pgto", "E", status: "P"));
        return invoice;
    }

    [Fact]
    public void Reabrir_nao_apaga_a_data_nem_o_vinculo_do_pagamento()
    {
        var invoice = Paid();
        var paidAt = invoice.PaidAt;

        invoice.Reopen();

        invoice.Status.ShouldBe("A");
        invoice.IsPaid.ShouldBeFalse();
        invoice.PaidAt.ShouldBe(paidAt);
        invoice.Transaction.ShouldNotBeNull();
    }

    [Fact]
    public void Restaurar_como_paga_mantem_a_data_original()
    {
        var invoice = Paid();
        var paidAt = invoice.PaidAt;
        invoice.Reopen();

        invoice.RestoreAsPaid();

        invoice.IsPaid.ShouldBeTrue();
        invoice.PaidAt.ShouldBe(paidAt);
    }

    [Fact]
    public void Restaurar_sem_data_anterior_usa_o_vencimento()
    {
        var invoice = new CardInvoice(1, 10, 2026, 10, new DateTime(2026, 10, 1), new DateTime(2026, 10, 10));

        invoice.RestoreAsPaid();

        invoice.IsPaid.ShouldBeTrue();
        invoice.PaidAt.ShouldBe(new DateTime(2026, 10, 10));
    }
}
