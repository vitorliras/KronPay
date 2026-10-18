using Application.Abstractions.AiChat;
using Application.Services.AiChat;
using Domain.Entities.Card;
using Shared.AI;
using Shouldly;
using DomainTransactions = global::Domain.Entities.Transactions;

namespace Tests.Application.AiChat;

public class ChatAttachmentDetectorsTests
{
    private static readonly List<CreditCard> Cards = new()
    {
        new CreditCard(10, 1, 1, "Nubank Vitor", 28, 20, 5000m),
        new CreditCard(11, 1, 1, "Itau Lany", 28, 20, 3000m)
    };

    private readonly CardInvoicePaymentDetector _invoice = new();
    private readonly AttachmentDuplicateDetector _duplicates = new();

    [Theory]
    [InlineData("PAGAMENTO FATURA NUBANK", true, 10)]
    [InlineData("Pgto Fatura Cartão Itaú 12/10", true, 11)]
    [InlineData("PAGAMENTO DE FATURA", true, null)]
    [InlineData("Pag fatura do cartão", true, null)]
    [InlineData("FATURA ITAU", true, 11)]
    [InlineData("Pagamento cartão de crédito", true, null)]
    public void Reconhece_pagamento_de_fatura_e_o_cartao(string description, bool expected, int? cardId)
    {
        var match = _invoice.Detect(description, Cards);

        match.IsPayment.ShouldBe(expected);
        match.CardId.ShouldBe(cardId);
    }

    [Theory]
    [InlineData("FATURA ENERGIA CEMIG")]
    [InlineData("Pagamento de boleto")]
    [InlineData("UBER *TRIP")]
    [InlineData("Mercado do Vitor")]
    [InlineData("")]
    public void Nao_confunde_gasto_comum_com_fatura_de_cartao(string description)
    {
        _invoice.Detect(description, Cards).IsPayment.ShouldBeFalse();
    }

    [Fact]
    public void Palavras_genericas_do_nome_do_cartao_nao_bastam()
    {
        var cards = new List<CreditCard> { new(12, 1, 1, "Cartao Visa", 28, 20, 1000m) };

        _invoice.Detect("Fatura de cartao visa", cards).IsPayment.ShouldBeTrue();
        _invoice.Detect("Fatura de energia visa", cards).IsPayment.ShouldBeFalse();
    }

    private static ChatAttachment Attachment(string id, params (string date, string description, decimal amount)[] rows) =>
        new(id, $"{id}.pdf", rows.Select(r => new AttachmentTransaction(DateTime.Parse(r.date), r.description, r.amount, "E")).ToList(), DateTime.UtcNow);

    private static DomainTransactions.Transaction Existing(string date, string description, decimal amount) =>
        new(userId: 1, amount: amount, transactionDate: DateTime.Parse(date), description: description,
            codTypeTransaction: "E", categoryId: null, categoryItemId: null, idPaymentMethod: 1, status: "P");

    [Fact]
    public void Marca_como_ja_lancada_quando_data_valor_e_descricao_batem()
    {
        var flags = _duplicates.Detect(
            new[] { Attachment("a", ("2026-09-12", "UBER *TRIP 123", 25m), ("2026-09-13", "Padaria", 8m)) },
            new[] { Existing("2026-09-12", "Uber", 25m) });

        flags[0].ShouldBe(DuplicateKinds.AlreadyRegistered);
        flags[1].ShouldBeNull();
    }

    [Fact]
    public void Valor_ou_data_diferentes_nao_sao_repetidas()
    {
        var flags = _duplicates.Detect(
            new[] { Attachment("a", ("2026-09-12", "Uber", 26m), ("2026-09-14", "Uber", 25m)) },
            new[] { Existing("2026-09-12", "Uber", 25m) });

        flags.ShouldAllBe(f => f == null);
    }

    [Fact]
    public void Dois_cafes_iguais_no_mesmo_dia_com_so_um_lancado_marca_apenas_um()
    {
        var flags = _duplicates.Detect(
            new[] { Attachment("a", ("2026-09-12", "Cafe", 8m), ("2026-09-12", "Cafe", 8m)) },
            new[] { Existing("2026-09-12", "Cafe", 8m) });

        flags.Count(f => f == DuplicateKinds.AlreadyRegistered).ShouldBe(1);
        flags.Count(f => f == null).ShouldBe(1);
    }

    [Fact]
    public void Linhas_do_segundo_arquivo_repetidas_do_primeiro_sao_marcadas_mas_dentro_do_mesmo_arquivo_nao()
    {
        var first = Attachment("a", ("2026-09-12", "Cafe", 8m), ("2026-09-12", "Cafe", 8m));
        var second = Attachment("b", ("2026-09-12", "Cafe", 8m), ("2026-09-20", "Livro", 50m));

        var flags = _duplicates.Detect(new[] { first, second }, Array.Empty<DomainTransactions.Transaction>());

        flags[0].ShouldBeNull();
        flags[1].ShouldBeNull();
        flags[2].ShouldBe(DuplicateKinds.RepeatedInAttachment);
        flags[3].ShouldBeNull();
    }

    [Fact]
    public void Ja_lancada_tem_prioridade_sobre_repetida_no_arquivo()
    {
        var first = Attachment("a", ("2026-09-12", "Cafe", 8m));
        var second = Attachment("b", ("2026-09-12", "Cafe", 8m));

        var flags = _duplicates.Detect(new[] { first, second }, new[] { Existing("2026-09-12", "Cafe", 8m) });

        flags[0].ShouldBe(DuplicateKinds.AlreadyRegistered);
        flags[1].ShouldBe(DuplicateKinds.RepeatedInAttachment);
    }
}
