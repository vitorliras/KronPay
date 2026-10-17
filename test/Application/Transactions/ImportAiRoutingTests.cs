using Tests.Support;
using Application.DTOs.Transactions;
using Domain.Services.Transactions;
using DomainEntities = global::Domain.Entities;
using DomainExceptions = global::Domain.Exceptions;
using Application.Abstractions.AI;
using Application.Abstractions.Common;
using Application.Abstractions.Import;
using Application.Configuration.AI;
using Application.Services.Transactions;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Infrastructure.Services.Assistant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.Localization;
using Shouldly;

namespace Tests.Application.Transactions;

public class ImportAiRoutingTests
{
    private readonly Mock<ITransactionImportParser> _localCsvParser = new();
    private readonly Mock<ITransactionImportParser> _localOfxParser = new();
    private readonly Mock<ITransactionImportParser> _aiParser = new();
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<ICategoryItemRepository> _items = new();
    private readonly Mock<IPaymentMethodRepository> _paymentMethods = new();
    private readonly Mock<ITransactionAiBatchClassifier> _classifier = new();
    private readonly Mock<IAiAvailability> _availability = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    public ImportAiRoutingTests()
    {
        _currentUser.Setup(c => c.UserId).Returns(1);
        _categories.Setup(c => c.GetAllAsync(It.IsAny<int>())).ReturnsAsync(new List<Category>());
        _items.Setup(i => i.GetAllByUserIdAsync(It.IsAny<int>())).ReturnsAsync(Array.Empty<CategoryItem>());
        _transactions
            .Setup(t => t.GetCategorizedHistoryAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<CategorizedTransactionHistoryItem>());
        _classifier
            .Setup(c => c.SuggestBatchAsync(It.IsAny<int>(), It.IsAny<IReadOnlyList<ImportedTransactionResponse>>(), It.IsAny<bool>()))
            .ReturnsAsync((int _, IReadOnlyList<ImportedTransactionResponse> batch, bool _) =>
                (IReadOnlyList<TransactionAiSuggestion>)batch.Select(_ => new TransactionAiSuggestion(false, 0m, null, null)).ToList());

        _localCsvParser.Setup(p => p.CanParse(It.IsAny<string>())).Returns<string>(n => n.EndsWith(".csv"));
        _localOfxParser.Setup(p => p.CanParse(It.IsAny<string>())).Returns<string>(n => n.EndsWith(".ofx"));
        _aiParser.Setup(p => p.CanParse(It.IsAny<string>())).Returns<string>(n => n.EndsWith(".pdf") || n.EndsWith(".png"));
        _aiParser.Setup(p => p.IsAiBased).Returns(true);
    }

    private static ImportedTransactionResponse Row(string description) =>
        new(DateTime.Today, 10m, description, "E", "P", 1, null, null, null, null);

    private static void ParserReturns(Mock<ITransactionImportParser> parser, params ImportedTransactionResponse[] rows) =>
        parser
            .Setup(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<IEnumerable<PaymentMethod>?>(), It.IsAny<IEnumerable<Category>?>()))
            .ReturnsAsync(rows.AsEnumerable());

    private static void ParserThrows(Mock<ITransactionImportParser> parser, Exception exception) =>
        parser
            .Setup(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<IEnumerable<PaymentMethod>?>(), It.IsAny<IEnumerable<Category>?>()))
            .ThrowsAsync(exception);

    private void AiEnabled(bool enabled) =>
        _availability.Setup(a => a.GetActive())
            .Returns(enabled ? new ActiveAiProvider("OpenAI", new AiProviderSettings { Model = "m" }) : null);

    private ImportTransactionsUseCase Build()
    {
        var memory = new TransactionCategoryMemoryBuilder(
            _transactions.Object, CardPurchaseRepositoryStub.Empty(),
            _items.Object,
            new LevenshteinParameterFuzzyMatcher(new ConfigurationBuilder().Build()));

        var classification = new ImportedTransactionClassificationService(
            memory,
            _classifier.Object,
            NullLogger<ImportedTransactionClassificationService>.Instance);

        return new ImportTransactionsUseCase(
            new[] { _localCsvParser.Object, _localOfxParser.Object, _aiParser.Object },
            _transactions.Object,
            classification,
            _availability.Object,
            _paymentMethods.Object,
            _categories.Object,
            _unitOfWork.Object,
            _currentUser.Object);
    }

    private static ImportTransactionsRequest Request(string fileName, bool preview = false, bool useAi = false) =>
        new(new MemoryStream(new byte[] { 1 }), fileName, preview, useAi);

    [Fact]
    public async Task Arquivo_lido_por_ia_sempre_abre_a_previa_e_nao_salva()
    {
        AiEnabled(true);
        ParserReturns(_aiParser, Row("mercado"));

        var result = await Build().ExecuteAsync(Request("foto.png", preview: false));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.TotalImported.ShouldBe(0);
        result.Value.Transactions.Count().ShouldBe(1);
        _transactions.Verify(t => t.AddRangeAsync(It.IsAny<IEnumerable<DomainEntities.Transactions.Transaction>>()), Times.Never);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken?>()), Times.Never);
    }

    [Fact]
    public async Task Arquivo_lido_por_ia_classifica_com_ia_mesmo_sem_o_usuario_pedir()
    {
        AiEnabled(true);
        ParserReturns(_aiParser, Row("mercado"));

        await Build().ExecuteAsync(Request("foto.png", useAi: false));

        _classifier.Verify(c => c.SuggestBatchAsync(1, It.IsAny<IReadOnlyList<ImportedTransactionResponse>>(), true), Times.Once);
    }

    [Fact]
    public async Task Ofx_nunca_chama_o_parser_de_ia()
    {
        AiEnabled(true);
        ParserReturns(_localOfxParser, Row("uber"));

        await Build().ExecuteAsync(Request("extrato.ofx", preview: true));

        _aiParser.Verify(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<IEnumerable<PaymentMethod>?>(), It.IsAny<IEnumerable<Category>?>()), Times.Never);
    }

    [Fact]
    public async Task Ofx_com_falha_de_leitura_nao_cai_para_ia()
    {
        AiEnabled(true);
        ParserThrows(_localOfxParser, new FormatException("ofx quebrado"));

        await Should.ThrowAsync<FormatException>(() => Build().ExecuteAsync(Request("extrato.ofx", preview: true)));

        _aiParser.Verify(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<IEnumerable<PaymentMethod>?>(), It.IsAny<IEnumerable<Category>?>()), Times.Never);
    }

    [Fact]
    public async Task Csv_fora_do_padrao_cai_para_ia_quando_ela_esta_ativa()
    {
        AiEnabled(true);
        ParserThrows(_localCsvParser, new FormatException("layout diferente"));
        ParserReturns(_aiParser, Row("padaria"));
        _aiParser.Setup(p => p.CanParse("extrato.csv")).Returns(true);

        var result = await Build().ExecuteAsync(Request("extrato.csv", preview: false));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Transactions.Single().Description.ShouldBe("padaria");
        result.Value.TotalImported.ShouldBe(0);
    }

    [Fact]
    public async Task Csv_fora_do_padrao_sem_ia_mantem_o_erro_original()
    {
        AiEnabled(false);
        ParserThrows(_localCsvParser, new FormatException("layout diferente"));

        await Should.ThrowAsync<FormatException>(() => Build().ExecuteAsync(Request("extrato.csv")));
    }

    [Fact]
    public async Task Csv_vazio_sem_ia_nao_chama_o_parser_de_ia()
    {
        AiEnabled(false);
        ParserReturns(_localCsvParser);

        await Build().ExecuteAsync(Request("extrato.csv", preview: true));

        _aiParser.Verify(p => p.ParseAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<IEnumerable<PaymentMethod>?>(), It.IsAny<IEnumerable<Category>?>()), Times.Never);
    }

    [Fact]
    public async Task Erro_de_dominio_do_parser_de_ia_vira_falha_com_a_chave_da_mensagem()
    {
        AiEnabled(true);
        ParserThrows(_aiParser, new DomainExceptions.DomainException(MessageKeys.AiFilesUnsupported));

        var result = await Build().ExecuteAsync(Request("nota.pdf"));

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(MessageKeys.AiFilesUnsupported);
    }

    [Fact]
    public async Task Formato_desconhecido_continua_retornando_falha()
    {
        var result = await Build().ExecuteAsync(Request("arquivo.zip"));

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(MessageKeys.UnsupportedFileFormat);
    }
}
