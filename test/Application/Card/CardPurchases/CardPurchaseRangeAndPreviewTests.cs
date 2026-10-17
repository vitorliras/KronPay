using Application.Abstractions.AI;
using Application.Abstractions.Common;
using Application.Abstractions.Import;
using Application.Configuration.AI;
using Application.DTOs.Card.CardPurchases;
using Application.DTOs.Transactions;
using Application.Services.Card;
using Application.Services.Transactions;
using Application.UseCases.Card.CardPurchases;
using Domain.Entities.Card;
using Domain.Entities.Configuration;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Interfaces.Card;
using Domain.Interfaces.Transactions;
using Domain.Services.Card;
using Domain.Services.Transactions;
using Infrastructure.Services.Assistant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.AI;
using Shared.Localization;
using Shouldly;

namespace Tests.Application.Card.CardPurchases;

public abstract class CardFlowTestBase
{
    protected readonly Mock<ICreditCardRepository> CreditCards = new();
    protected readonly Mock<ICardPurchaseRepository> Purchases = new();
    protected readonly Mock<ICardInvoiceRepository> Invoices = new();
    protected readonly Mock<ICategoryRepository> Categories = new();
    protected readonly Mock<ICategoryItemRepository> CategoryItems = new();
    protected readonly Mock<ITransactionRepository> Transactions = new();
    protected readonly Mock<ITransactionAiBatchClassifier> Classifier = new();
    protected readonly Mock<IAiAvailability> Availability = new();
    protected readonly Mock<ICurrentUserService> CurrentUser = new();
    protected readonly Mock<IUnitOfWork> UnitOfWork = new();
    protected readonly Mock<ICreditCardBillingCalculator> Billing = new();

    protected CardFlowTestBase()
    {
        CurrentUser.Setup(c => c.UserId).Returns(1);
        UnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken?>())).ReturnsAsync(true);

        CreditCards.Setup(c => c.GetByIdAsync(10, 1)).ReturnsAsync(new CreditCard(10, 1, 1, "cartao", 28, 20, 1000m));
        Purchases.Setup(p => p.SumPendingInstallmentsByCardAsync(10, 1)).ReturnsAsync(0m);
        Purchases.Setup(p => p.AddAsync(It.IsAny<CardPurchase>())).ReturnsAsync(true);
        Purchases.Setup(p => p.AddInstallmentsAsync(It.IsAny<IEnumerable<CardInstallment>>())).ReturnsAsync(true);
        Purchases.Setup(p => p.GetCategorizedHistoryAsync(It.IsAny<int>())).ReturnsAsync(new List<CategorizedTransactionHistoryItem>());
        Purchases
            .Setup(p => p.GetActiveByCardAndPeriodAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CardPurchase>());

        Invoices
            .Setup(i => i.GetByCycleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<short>(), It.IsAny<short>()))
            .ReturnsAsync((CardInvoice?)null);
        Invoices.Setup(i => i.AddAsync(It.IsAny<CardInvoice>())).ReturnsAsync(true);
        Invoices.Setup(i => i.GetPaymentTransactionIdsAsync(It.IsAny<int>())).ReturnsAsync(new List<int>());
        Invoices.Setup(i => i.GetPendingAmountsByCardAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new Dictionary<int, decimal>());
        Billing
            .Setup(b => b.Resolve(It.IsAny<CreditCard>(), It.IsAny<DateTime>()))
            .Returns<CreditCard, DateTime>((_, d) => new BillingCycle(
                (short)d.Year, (short)d.Month, new DateTime(d.Year, d.Month, 20), new DateTime(d.Year, d.Month, 28)));

        Categories.Setup(c => c.GetAllAsync(It.IsAny<int>())).ReturnsAsync(new List<Category> { Category(3, "E"), Category(9, "I") });
        CategoryItems.Setup(i => i.GetAllByUserIdAsync(It.IsAny<int>())).ReturnsAsync(Array.Empty<CategoryItem>());
        Transactions.Setup(t => t.GetCategorizedHistoryAsync(It.IsAny<int>())).ReturnsAsync(new List<CategorizedTransactionHistoryItem>());
        Classifier
            .Setup(c => c.SuggestBatchAsync(It.IsAny<int>(), It.IsAny<IReadOnlyList<ImportedTransactionResponse>>(), It.IsAny<bool>()))
            .ReturnsAsync((int _, IReadOnlyList<ImportedTransactionResponse> batch, bool _) =>
                (IReadOnlyList<TransactionAiSuggestion>)batch.Select(_ => new TransactionAiSuggestion(false, 0m, null, null)).ToList());
    }

    protected static Category Category(int id, string type)
    {
        var category = new Category(1, $"categoria {id}", type);
        typeof(Category).GetProperty("Id")!.SetValue(category, id);
        return category;
    }

    protected void AiEnabled(bool enabled) =>
        Availability.Setup(a => a.GetActive())
            .Returns(enabled ? new ActiveAiProvider("OpenAI", new AiProviderSettings { Model = "m", SupportsFiles = true }) : null);

    protected CardPurchasePreviewBuilder PreviewBuilder()
    {
        var memory = new TransactionCategoryMemoryBuilder(
            Transactions.Object,
            Purchases.Object,
            CategoryItems.Object,
            new LevenshteinParameterFuzzyMatcher(new ConfigurationBuilder().Build()));

        var classification = new ImportedTransactionClassificationService(
            memory,
            Classifier.Object,
            NullLogger<ImportedTransactionClassificationService>.Instance);

        return new CardPurchasePreviewBuilder(classification, Categories.Object, Purchases.Object);
    }

    protected static ExtractedCardPurchase Extracted(string description, decimal amount = 50m, short installments = 1, int day = 10) =>
        new(new DateTime(2026, 10, day), description, amount, installments);
}

public class CreateCardPurchaseRangeUseCaseTests : CardFlowTestBase
{
    private CreateCardPurchaseRangeUseCase Build() =>
        new(
            CreditCards.Object,
            Purchases.Object,
            Categories.Object,
            CategoryItems.Object,
            new CardPurchaseRegistrar(Purchases.Object, Invoices.Object, Billing.Object),
            UnitOfWork.Object,
            CurrentUser.Object);

    private static CreateCardPurchaseRangeItem Item(string description, decimal amount, short installments = 1, int? categoryId = null, int? itemId = null) =>
        new(description, amount, new DateTime(2026, 10, 10), installments, categoryId, itemId);

    private static CreateCardPurchaseRangeRequest Request(params CreateCardPurchaseRangeItem[] items) => new(10, items);

    [Fact]
    public async Task Cartao_inexistente_ou_de_outro_usuario_falha()
    {
        var result = await Build().ExecuteAsync(new CreateCardPurchaseRangeRequest(99, new[] { Item("a", 10m) }));

        result.Message.ShouldBe(MessageKeys.CreditCardNotFound);
        UnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken?>()), Times.Never);
    }

    [Fact]
    public async Task Limite_e_verificado_de_forma_acumulada_e_recusa_o_lote_inteiro()
    {
        Purchases.Setup(p => p.SumPendingInstallmentsByCardAsync(10, 1)).ReturnsAsync(400m);

        var result = await Build().ExecuteAsync(Request(Item("a", 300m), Item("b", 301m)));

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(MessageKeys.CreditLimitExceeded);
        Purchases.Verify(p => p.AddAsync(It.IsAny<CardPurchase>()), Times.Never);
        UnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken?>()), Times.Never);
    }

    [Fact]
    public async Task Lote_dentro_do_limite_cria_tudo_com_um_unico_commit()
    {
        Purchases.Setup(p => p.SumPendingInstallmentsByCardAsync(10, 1)).ReturnsAsync(400m);

        var result = await Build().ExecuteAsync(Request(Item("a", 300m), Item("b", 300m, installments: 3)));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Created.ShouldBe(2);
        result.Value.TotalAmount.ShouldBe(600m);
        Purchases.Verify(p => p.AddAsync(It.IsAny<CardPurchase>()), Times.Exactly(2));
        Invoices.Verify(i => i.AddAsync(It.IsAny<CardInvoice>()), Times.Exactly(3));
        UnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken?>()), Times.Once);
    }

    [Fact]
    public async Task Categoria_de_outro_usuario_e_recusada()
    {
        var result = await Build().ExecuteAsync(Request(Item("a", 10m, categoryId: 777)));

        result.Message.ShouldBe(MessageKeys.CategoryNotFound);
        UnitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken?>()), Times.Never);
    }

    [Fact]
    public async Task Subcategoria_que_nao_pertence_a_categoria_e_recusada()
    {
        var item = new CategoryItem(4, "fibra");
        typeof(CategoryItem).GetProperty("Id")!.SetValue(item, 11);
        CategoryItems.Setup(i => i.GetAllByUserIdAsync(It.IsAny<int>())).ReturnsAsync(new[] { item });

        var result = await Build().ExecuteAsync(Request(Item("a", 10m, categoryId: 3, itemId: 11)));

        result.Message.ShouldBe(MessageKeys.CategoryNotFound);
    }

    [Fact]
    public async Task Falha_de_commit_retorna_mensagem_de_persistencia()
    {
        UnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken?>())).ReturnsAsync(false);

        var result = await Build().ExecuteAsync(Request(Item("a", 10m)));

        result.Message.ShouldBe(MessageKeys.DataPersistenceFailed);
    }
}

public class CardPurchasePreviewBuilderTests : CardFlowTestBase
{
    [Fact]
    public async Task Memoria_do_cartao_sugere_categoria_de_despesa_e_marca_a_origem()
    {
        Purchases.Setup(p => p.GetCategorizedHistoryAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<CategorizedTransactionHistoryItem> { new("ifood", "E", 3, null, 12) });

        var preview = await PreviewBuilder().BuildAsync(1, 10, new[] { Extracted("iFood") }, 0, useAi: false);

        preview.Purchases.Single().CategoryId.ShouldBe(3);
        preview.Purchases.Single().ClassificationSource.ShouldBe(ClassificationSources.Memory);
    }

    [Fact]
    public async Task Categoria_que_nao_e_de_despesa_e_descartada()
    {
        Transactions.Setup(t => t.GetCategorizedHistoryAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<CategorizedTransactionHistoryItem> { new("salario", "E", 9, null, 12) });

        var preview = await PreviewBuilder().BuildAsync(1, 10, new[] { Extracted("salario") }, 0, useAi: false);

        preview.Purchases.Single().CategoryId.ShouldBeNull();
        preview.Purchases.Single().ClassificationSource.ShouldBeNull();
    }

    [Fact]
    public async Task Compra_igual_a_existente_e_marcada_como_possivel_duplicata()
    {
        var existing = new CardPurchase(1, 10, "IFOOD  *Pedido", 50m, new DateTime(2026, 10, 10), 1);
        Purchases
            .Setup(p => p.GetActiveByCardAndPeriodAsync(10, 1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<CardPurchase> { existing });

        var preview = await PreviewBuilder().BuildAsync(
            1, 10, new[] { Extracted("ifood pedido", 50m), Extracted("ifood pedido", 51m), Extracted("uber", 50m) }, 0, useAi: false);

        preview.Purchases.Select(p => p.PossibleDuplicate).ShouldBe(new[] { true, false, false });
    }

    [Fact]
    public async Task Parcelas_e_valores_sao_preservados_e_o_total_lido_inclui_os_ignorados()
    {
        var preview = await PreviewBuilder().BuildAsync(1, 10, new[] { Extracted("tenis", 300m, 3) }, skipped: 2, useAi: false);

        preview.Purchases.Single().InstallmentsCount.ShouldBe((short)3);
        preview.Purchases.Single().TotalAmount.ShouldBe(300m);
        preview.TotalRead.ShouldBe(3);
        preview.Skipped.ShouldBe(2);
    }
}

public class ParseCardPurchasesFromTextUseCaseTests : CardFlowTestBase
{
    private readonly Mock<ICardPurchaseExtractionService> _extraction = new();

    private ParseCardPurchasesFromTextUseCase Build() =>
        new(
            Availability.Object,
            CreditCards.Object,
            _extraction.Object,
            PreviewBuilder(),
            CurrentUser.Object,
            NullLogger<ParseCardPurchasesFromTextUseCase>.Instance);

    [Fact]
    public async Task Ia_desligada_falha_sem_chamar_a_extracao()
    {
        AiEnabled(false);

        var result = await Build().ExecuteAsync(new ParseCardPurchasesFromTextRequest(10, "tenis 300 em 3x"));

        result.Message.ShouldBe(MessageKeys.AiUnavailable);
        _extraction.Verify(e => e.ExtractFromTextAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cartao_de_outro_usuario_falha()
    {
        AiEnabled(true);

        var result = await Build().ExecuteAsync(new ParseCardPurchasesFromTextRequest(99, "x"));

        result.Message.ShouldBe(MessageKeys.CreditCardNotFound);
    }

    [Fact]
    public async Task Texto_vira_previa_com_parcelas_reais_e_funcionalidade_do_cartao()
    {
        AiEnabled(true);
        string? feature = null;
        _extraction
            .Setup(e => e.ExtractFromTextAsync(1, "tenis 300 em 3x", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<int, string, string, CancellationToken>((_, _, f, _) => feature = f)
            .ReturnsAsync(new[] { Extracted("tenis", 300m, 3) });

        var result = await Build().ExecuteAsync(new ParseCardPurchasesFromTextRequest(10, "  tenis 300 em 3x  "));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Purchases.Single().InstallmentsCount.ShouldBe((short)3);
        feature.ShouldBe(AiFeatures.CardTextEntry);
    }

    [Fact]
    public async Task Nada_extraido_retorna_mensagem_propria()
    {
        AiEnabled(true);
        _extraction
            .Setup(e => e.ExtractFromTextAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ExtractedCardPurchase>());

        var result = await Build().ExecuteAsync(new ParseCardPurchasesFromTextRequest(10, "bom dia"));

        result.Message.ShouldBe(MessageKeys.AiNothingExtracted);
    }

    [Fact]
    public async Task Erro_da_ia_retorna_mensagem_amigavel()
    {
        AiEnabled(true);
        _extraction
            .Setup(e => e.ExtractFromTextAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("segredo"));

        var result = await Build().ExecuteAsync(new ParseCardPurchasesFromTextRequest(10, "x"));

        result.Message.ShouldBe(MessageKeys.AiExtractionFailed);
    }
}

public class ImportCardPurchasesUseCaseTests : CardFlowTestBase
{
    private readonly Mock<ITransactionImportParser> _ofx = new();
    private readonly Mock<ITransactionImportParser> _csv = new();
    private readonly Mock<ITransactionImportParser> _aiParser = new();
    private readonly Mock<IAiDocumentReader> _reader = new();
    private readonly Mock<ICardPurchaseExtractionService> _extraction = new();

    public ImportCardPurchasesUseCaseTests()
    {
        _ofx.Setup(p => p.CanParse(It.IsAny<string>())).Returns<string>(n => n.EndsWith(".ofx"));
        _csv.Setup(p => p.CanParse(It.IsAny<string>())).Returns<string>(n => n.EndsWith(".csv"));
        _aiParser.Setup(p => p.CanParse(It.IsAny<string>())).Returns<string>(n => n.EndsWith(".pdf"));
        _aiParser.Setup(p => p.IsAiBased).Returns(true);
    }

    private ImportCardPurchasesUseCase Build() =>
        new(
            new[] { _ofx.Object, _csv.Object, _aiParser.Object },
            Availability.Object,
            _reader.Object,
            _extraction.Object,
            CreditCards.Object,
            Categories.Object,
            PreviewBuilder(),
            CurrentUser.Object,
            NullLogger<ImportCardPurchasesUseCase>.Instance);

    private static ImportedTransactionResponse Row(string description, string type = "E", decimal amount = 10m) =>
        new(new DateTime(2026, 10, 10), amount, description, type, "P", 1, null, null, null, null);

    private static void Parses(Mock<ITransactionImportParser> parser, params ImportedTransactionResponse[] rows) =>
        parser
            .Setup(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<IEnumerable<PaymentMethod>?>(), It.IsAny<IEnumerable<Category>?>()))
            .ReturnsAsync(rows.AsEnumerable());

    private static void ParserThrows(Mock<ITransactionImportParser> parser) =>
        parser
            .Setup(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<IEnumerable<PaymentMethod>?>(), It.IsAny<IEnumerable<Category>?>()))
            .ThrowsAsync(new FormatException("ruim"));

    private static ImportCardPurchasesRequest Request(string fileName, bool useAi = false, int cardId = 10) =>
        new(new MemoryStream(new byte[] { 1 }), fileName, cardId, useAi);

    [Fact]
    public async Task Ofx_usa_o_parser_local_descarta_entradas_e_nunca_chama_a_ia()
    {
        AiEnabled(true);
        Parses(_ofx, Row("loja a", "E", 30m), Row("pagamento fatura", "I", 500m), Row("loja b", "E", 40m));

        var result = await Build().ExecuteAsync(Request("fatura.ofx"));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Purchases.Select(p => p.Description).ShouldBe(new[] { "loja a", "loja b" });
        result.Value.Skipped.ShouldBe(1);
        result.Value.Purchases.ShouldAllBe(p => p.InstallmentsCount == 1);
        _reader.Verify(r => r.ReadAsync(It.IsAny<Stream>()), Times.Never);
        _extraction.Verify(e => e.ExtractFromTableAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Descricao_longa_do_arquivo_e_truncada_em_60_caracteres()
    {
        Parses(_ofx, Row(new string('x', 90)));

        var result = await Build().ExecuteAsync(Request("fatura.ofx"));

        result.Value!.Purchases.Single().Description.Length.ShouldBe(60);
    }

    [Fact]
    public async Task Ofx_com_falha_de_leitura_nao_cai_para_ia()
    {
        AiEnabled(true);
        ParserThrows(_ofx);

        await Should.ThrowAsync<FormatException>(() => Build().ExecuteAsync(Request("fatura.ofx")));

        _reader.Verify(r => r.ReadAsync(It.IsAny<Stream>()), Times.Never);
    }

    [Fact]
    public async Task Arquivo_de_ia_le_pelo_leitor_e_importa_cada_linha_como_compra_de_uma_parcela()
    {
        AiEnabled(true);
        var attachment = new AiAttachment(new byte[] { 1 }, "application/pdf", "document.pdf");
        string? feature = null;
        _reader.Setup(r => r.ReadAsync(It.IsAny<Stream>())).ReturnsAsync(new AiDocument(attachment, Array.Empty<string>()));
        _extraction
            .Setup(e => e.ExtractFromFileAsync(1, attachment, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<int, AiAttachment, string, CancellationToken>((_, _, f, _) => feature = f)
            .ReturnsAsync(new[] { Extracted("loja x 03/10", 50m), Extracted("outra", 20m) });

        var result = await Build().ExecuteAsync(Request("fatura.pdf"));

        result.Value!.Purchases.Count.ShouldBe(2);
        feature.ShouldBe(AiFeatures.CardDocumentImport);
    }

    [Fact]
    public async Task Csv_fora_do_padrao_cai_para_ia_quando_ela_esta_ativa()
    {
        AiEnabled(true);
        ParserThrows(_csv);
        _reader.Setup(r => r.ReadAsync(It.IsAny<Stream>())).ReturnsAsync(new AiDocument(null, new[] { "tabela" }));
        _extraction
            .Setup(e => e.ExtractFromTableAsync(1, "tabela", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { Extracted("padaria") });

        var result = await Build().ExecuteAsync(Request("fatura.csv"));

        result.Value!.Purchases.Single().Description.ShouldBe("padaria");
    }

    [Fact]
    public async Task Csv_fora_do_padrao_sem_ia_mantem_o_erro_original()
    {
        AiEnabled(false);
        ParserThrows(_csv);

        await Should.ThrowAsync<FormatException>(() => Build().ExecuteAsync(Request("fatura.csv")));
    }

    [Fact]
    public async Task Erro_de_dominio_do_leitor_vira_falha_com_a_chave_da_mensagem()
    {
        AiEnabled(true);
        _reader.Setup(r => r.ReadAsync(It.IsAny<Stream>())).ThrowsAsync(new DomainException(MessageKeys.AiFilesUnsupported));

        var result = await Build().ExecuteAsync(Request("fatura.pdf"));

        result.Message.ShouldBe(MessageKeys.AiFilesUnsupported);
    }

    [Fact]
    public async Task Erro_inesperado_da_ia_vira_mensagem_amigavel()
    {
        AiEnabled(true);
        _reader.Setup(r => r.ReadAsync(It.IsAny<Stream>())).ReturnsAsync(new AiDocument(null, new[] { "t" }));
        _extraction
            .Setup(e => e.ExtractFromTableAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("sk-123"));

        var result = await Build().ExecuteAsync(Request("fatura.pdf"));

        result.Message.ShouldBe(MessageKeys.AiExtractionFailed);
    }

    [Fact]
    public async Task Sem_nenhuma_compra_retorna_mensagem_propria()
    {
        Parses(_ofx, Row("pagamento", "I"));

        var result = await Build().ExecuteAsync(Request("fatura.ofx"));

        result.Message.ShouldBe(MessageKeys.AiNothingExtracted);
    }

    [Fact]
    public async Task Formato_desconhecido_e_cartao_de_outro_usuario_falham()
    {
        (await Build().ExecuteAsync(Request("arquivo.zip"))).Message.ShouldBe(MessageKeys.UnsupportedFileFormat);
        (await Build().ExecuteAsync(Request("fatura.ofx", cardId: 99))).Message.ShouldBe(MessageKeys.CreditCardNotFound);
    }
}
