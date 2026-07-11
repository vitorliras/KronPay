using Application.DataRetention.Targets;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Moq;
using Shouldly;

namespace Tests.Application.DataRetention.Targets;

public class PaymentMethodRetentionPurgeTargetTests
{
    [Fact]
    public async Task Apaga_forma_de_pagamento_sem_transacoes_vinculadas()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var method = CreateDeactivatedPaymentMethod();

        var paymentMethods = new Mock<IPaymentMethodRepository>();
        paymentMethods.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<PaymentMethod> { method });

        var transactions = new Mock<ITransactionRepository>();
        transactions.Setup(r => r.ExistsByPaymentMethodIdAsync(method.Id)).ReturnsAsync(false);

        var sut = new PaymentMethodRetentionPurgeTarget(paymentMethods.Object, transactions.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(1);
        paymentMethods.Verify(r => r.DeleteRangeAsync(It.Is<IEnumerable<PaymentMethod>>(m => m.Contains(method))), Times.Once);
    }

    [Fact]
    public async Task Mantem_forma_de_pagamento_ainda_referenciada_por_transacao()
    {
        var cutoff = new DateTime(2026, 7, 1);
        var method = CreateDeactivatedPaymentMethod();

        var paymentMethods = new Mock<IPaymentMethodRepository>();
        paymentMethods.Setup(r => r.GetDeactivatedOlderThanAsync(cutoff))
            .ReturnsAsync(new List<PaymentMethod> { method });

        var transactions = new Mock<ITransactionRepository>();
        transactions.Setup(r => r.ExistsByPaymentMethodIdAsync(method.Id)).ReturnsAsync(true);

        var sut = new PaymentMethodRetentionPurgeTarget(paymentMethods.Object, transactions.Object);

        var removed = await sut.PurgeAsync(cutoff);

        removed.ShouldBe(0);
        paymentMethods.Verify(r => r.DeleteRangeAsync(It.IsAny<IEnumerable<PaymentMethod>>()), Times.Never);
    }

    private static PaymentMethod CreateDeactivatedPaymentMethod()
    {
        var method = new PaymentMethod(1, "Pix");
        method.Deactivate();
        return method;
    }
}
