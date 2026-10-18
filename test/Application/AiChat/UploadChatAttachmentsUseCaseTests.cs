using Application.Abstractions.AI;
using Application.Abstractions.Import;
using Application.Configuration.AI;
using Application.DTOs.AiChat;
using Application.Services.AiChat;
using Application.Services.Transactions;
using Application.UseCases.AiChat;
using Application.Validators.AiChat;
using Domain.Entities.Card;
using Domain.Exceptions;
using Infrastructure.AI.Chat;
using Infrastructure.Services.Assistant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Localization;
using Shouldly;
using Tests.Application.Card.CardPurchases;

namespace Tests.Application.AiChat;

public class UploadChatAttachmentsUseCaseTests : CardFlowTestBase
{
    private readonly Mock<ITransactionImportParser> _parser = new();
    private readonly InMemoryChatAttachmentStore _store;
    private readonly AiSettings _settings = new() { Chat = new AiChatSettings { MaxAttachmentsPerMessage = 2, MaxAttachmentBytes = 1000 } };
    private readonly Mock<IOptionsMonitor<AiSettings>> _monitor = new();

    public UploadChatAttachmentsUseCaseTests()
    {
        _monitor.Setup(m => m.CurrentValue).Returns(_settings);
        _store = new InMemoryChatAttachmentStore(_monitor.Object);

        _parser.Setup(p => p.CanParse(It.IsAny<string>())).Returns<string>(n => n.EndsWith(".pdf") || n.EndsWith(".ofx"));
        _parser.Setup(p => p.IsAiBased).Returns(false);
        _parser
            .Setup(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<int>(), null, null))
            .ReturnsAsync(new[]
            {
                Imported("2026-09-10", "Padaria", 15m, "E"),
                Imported("2026-09-12", "Salario", 3000m, "I")
            });

        CreditCards.Setup(c => c.GetAllAsync(1)).ReturnsAsync(new List<CreditCard>());
        Transactions
            .Setup(t => t.GetByPeriodAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<global::Domain.Entities.Transactions.Transaction>());
    }

    private static ImportedTransactionResponse Imported(string date, string description, decimal amount, string type) =>
        new(DateTime.Parse(date), amount, description, type, "P", 1, null, null, null, null);

    private UploadChatAttachmentsUseCase Build(params ITransactionImportParser[] extraParsers)
    {
        var fuzzy = new LevenshteinParameterFuzzyMatcher(new ConfigurationBuilder().Build());
        var memory = new TransactionCategoryMemoryBuilder(Transactions.Object, Purchases.Object, CategoryItems.Object, fuzzy);
        var classification = new ImportedTransactionClassificationService(
            memory, Classifier.Object, NullLogger<ImportedTransactionClassificationService>.Instance);

        var tools = new ChatAttachmentToolService(
            _store,
            new ChatAttachmentContext(),
            new ChatAttachmentAnalyzer(Transactions.Object, CreditCards.Object, new CardInvoicePaymentDetector(), new AttachmentDuplicateDetector()),
            Categories.Object,
            new TransactionDraftCategorizer(classification),
            new ChatActionCollector(),
            CurrentUser.Object);

        return new UploadChatAttachmentsUseCase(
            new[] { _parser.Object }.Concat(extraParsers).ToArray(), Availability.Object, _store, tools, CurrentUser.Object, NullLogger<UploadChatAttachmentsUseCase>.Instance);
    }

    private static UploadChatAttachmentsRequest Request(params string[] names) =>
        new(names.Select(n => new ChatUploadFile(n, 10, new MemoryStream(new byte[10]))).ToList());

    [Fact]
    public async Task Arquivo_valido_vira_anexo_com_resumo_e_fica_guardado_para_o_usuario()
    {
        var result = await Build().ExecuteAsync(Request("extrato.ofx"));

        result.IsSuccess.ShouldBeTrue();

        var summary = result.Value!.Attachments.Single();
        summary.TransactionCount.ShouldBe(2);
        summary.PeriodStart.ShouldBe("2026-09-10");
        summary.PeriodEnd.ShouldBe("2026-09-12");
        summary.TotalIncome.ShouldBe(3000m);
        summary.TotalExpense.ShouldBe(15m);

        _store.Get(1, summary.Id).ShouldNotBeNull();
        _store.Get(2, summary.Id).ShouldBeNull();
    }

    [Fact]
    public async Task Formato_desconhecido_falha_sem_guardar_nada()
    {
        var result = await Build().ExecuteAsync(Request("foto.xyz"));

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(MessageKeys.UnsupportedFileFormat);
    }

    [Fact]
    public async Task Formato_de_ia_com_ia_desligada_falha()
    {
        _parser.Setup(p => p.IsAiBased).Returns(true);
        AiEnabled(false);

        var result = await Build().ExecuteAsync(Request("extrato.pdf"));

        result.Message.ShouldBe(MessageKeys.AiUnavailable);
    }

    [Fact]
    public async Task Nada_extraido_ou_erro_de_dominio_do_leitor_falha()
    {
        _parser.Setup(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<int>(), null, null)).ReturnsAsync(Array.Empty<ImportedTransactionResponse>());
        (await Build().ExecuteAsync(Request("a.ofx"))).Message.ShouldBe(MessageKeys.AiNothingExtracted);

        _parser.Setup(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<int>(), null, null)).ThrowsAsync(new DomainException(MessageKeys.AiExtractionFailed));
        (await Build().ExecuteAsync(Request("a.ofx"))).Message.ShouldBe(MessageKeys.AiExtractionFailed);
    }

    [Fact]
    public async Task Excecao_inesperada_vira_falha_de_extracao()
    {
        _parser.Setup(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<int>(), null, null)).ThrowsAsync(new InvalidOperationException("x"));

        (await Build().ExecuteAsync(Request("a.ofx"))).Message.ShouldBe(MessageKeys.AiExtractionFailed);
    }

    [Fact]
    public async Task Csv_que_o_leitor_tabular_nao_entende_cai_para_a_leitura_por_ia()
    {
        var csv = new Mock<ITransactionImportParser>();
        csv.Setup(p => p.CanParse(It.IsAny<string>())).Returns<string>(n => n.EndsWith(".csv"));
        csv.Setup(p => p.IsAiBased).Returns(false);
        csv.Setup(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<int>(), null, null)).ThrowsAsync(new IndexOutOfRangeException());

        var ai = new Mock<ITransactionImportParser>();
        ai.Setup(p => p.CanParse(It.IsAny<string>())).Returns(false);
        ai.Setup(p => p.IsAiBased).Returns(true);
        ai.Setup(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<int>(), null, null))
            .ReturnsAsync(new[] { Imported("2026-10-05", "Salary payment", 3041.22m, "I") });

        AiEnabled(true);

        var result = await Build(csv.Object, ai.Object).ExecuteAsync(Request("Nubank.csv"));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Attachments.Single().TransactionCount.ShouldBe(1);
    }

    [Fact]
    public async Task Validador_limita_quantidade_e_tamanho()
    {
        var validator = new UploadChatAttachmentsRequestValidator(_monitor.Object);

        (await validator.ValidateAsync(Request("a.ofx", "b.ofx"))).IsValid.ShouldBeTrue();
        (await validator.ValidateAsync(Request("a.ofx", "b.ofx", "c.ofx"))).Errors.Single().ErrorMessage.ShouldBe(MessageKeys.ChatAttachmentTooMany);
        (await validator.ValidateAsync(Request())).IsValid.ShouldBeFalse();

        var big = new UploadChatAttachmentsRequest(new[] { new ChatUploadFile("a.ofx", 5000, new MemoryStream()) });
        (await validator.ValidateAsync(big)).Errors.Single().ErrorMessage.ShouldBe(MessageKeys.ChatAttachmentTooLarge);
    }
}
