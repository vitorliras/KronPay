using Tests.Support;
using Application.Abstractions.AI;
using Application.Abstractions.Common;
using Application.Abstractions.Import;
using Application.DTOs.Transactions;
using Application.Services.Transactions;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Domain.Services.Assistant;
using Domain.Services.Transactions;
using Infrastructure.Services.Assistant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Application.Configuration.AI;
using Moq;
using Shared.AI;
using Shouldly;

namespace Tests.Application.Transactions;

public class ImportTransactionsUseCaseTests
{
    private readonly Mock<ITransactionImportParser> _parser = new();
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly Mock<IPaymentMethodRepository> _paymentMethods = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<ICategoryItemRepository> _items = new();
    private readonly Mock<ITransactionAiBatchClassifier> _classifier = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IAiAvailability> _availability = new();

    private readonly List<IReadOnlyList<ImportedTransactionResponse>> _classifierCalls = new();

    public ImportTransactionsUseCaseTests()
    {
        _currentUser.Setup(c => c.UserId).Returns(1);
        _parser.Setup(p => p.CanParse(It.IsAny<string>())).Returns(true);

        _categories
            .Setup(c => c.GetAllAsync(It.IsAny<int>()))
            .ReturnsAsync(Enumerable.Range(1, 20).Select(NewCategory).ToList());

        _items
            .Setup(i => i.GetAllByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<CategoryItem>());

        SetHistory();
    }

    private static Category NewCategory(int id)
    {
        var category = new Category(1, $"categoria {id}", "E");
        typeof(Category).GetProperty("Id")!.SetValue(category, id);
        return category;
    }

    private void SetHistory(params CategorizedTransactionHistoryItem[] history) =>
        _transactions
            .Setup(t => t.GetCategorizedHistoryAsync(It.IsAny<int>()))
            .ReturnsAsync(history.ToList());

    private static ImportedTransactionResponse Imported(
        string description,
        string type = "E",
        int? categoryId = null) =>
        new(DateTime.Today, 10m, description, type, "P", 1, categoryId, null, null, null);

    private ImportTransactionsUseCase Build(params ImportedTransactionResponse[] parsed)
    {
        _parser
            .Setup(p => p.ParseAsync(
                It.IsAny<Stream>(),
                It.IsAny<int>(),
                It.IsAny<IEnumerable<PaymentMethod>?>(),
                It.IsAny<IEnumerable<Category>?>()))
            .ReturnsAsync(parsed.AsEnumerable());

        var memoryBuilder = new TransactionCategoryMemoryBuilder(
            _transactions.Object, CardPurchaseRepositoryStub.Empty(),
            _items.Object,
            new LevenshteinParameterFuzzyMatcher(new ConfigurationBuilder().Build()));

        var classification = new ImportedTransactionClassificationService(
            memoryBuilder,
            _classifier.Object,
            NullLogger<ImportedTransactionClassificationService>.Instance);

        return new ImportTransactionsUseCase(
            new[] { _parser.Object },
            _transactions.Object,
            classification,
            _availability.Object,
            _paymentMethods.Object,
            _categories.Object,
            _unitOfWork.Object,
            _currentUser.Object);
    }

    private void ClassifierReturns(Func<ImportedTransactionResponse, TransactionAiSuggestion> map) =>
        _classifier
            .Setup(c => c.SuggestBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IReadOnlyList<ImportedTransactionResponse>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((int _, IReadOnlyList<ImportedTransactionResponse> batch, bool _) =>
            {
                _classifierCalls.Add(batch);
                return (IReadOnlyList<TransactionAiSuggestion>)batch.Select(map).ToList();
            });

    private static TransactionAiSuggestion Suggest(int categoryId) =>
        new(false, 0.9m, categoryId, "E", ClassificationSources.Ai);

    private static ImportTransactionsRequest Request(bool useAi = true) =>
        new(new MemoryStream(), "extrato.ofx", Preview: true, UseAi: useAi);

    [Fact]
    public async Task Descricao_conhecida_usa_a_memoria_e_nao_vai_para_o_classificador()
    {
        SetHistory(new CategorizedTransactionHistoryItem("internet", "E", 3, null, 90));
        ClassifierReturns(_ => Suggest(7));

        var result = await Build(Imported("Internet"), Imported("farmacia")).ExecuteAsync(Request());

        var transactions = result.Value!.Transactions.ToList();

        _classifierCalls.Single().Select(t => t.Description).ShouldBe(new[] { "farmacia" });
        transactions.Single(t => t.Description == "Internet").CategoryId.ShouldBe(3);
        transactions.Single(t => t.Description == "Internet").ClassificationSource.ShouldBe(ClassificationSources.Memory);
        transactions.Single(t => t.Description == "farmacia").CategoryId.ShouldBe(7);
        transactions.Single(t => t.Description == "farmacia").ClassificationSource.ShouldBe(ClassificationSources.Ai);
    }

    [Fact]
    public async Task Tudo_conhecido_nao_chama_o_classificador_nem_com_ia_ligada()
    {
        SetHistory(
            new CategorizedTransactionHistoryItem("internet", "E", 3, null, 90),
            new CategorizedTransactionHistoryItem("agua", "E", 4, null, 85));

        var result = await Build(Imported("internet"), Imported("agua")).ExecuteAsync(Request(useAi: true));

        _classifier.Verify(c => c.SuggestBatchAsync(
            It.IsAny<int>(),
            It.IsAny<IReadOnlyList<ImportedTransactionResponse>>(),
            It.IsAny<bool>()), Times.Never);

        result.Value!.Transactions.Select(t => t.CategoryId).ShouldBe(new int?[] { 3, 4 }, ignoreOrder: true);
    }

    [Fact]
    public async Task Descricoes_repetidas_viram_uma_unica_consulta_e_todas_recebem_o_resultado()
    {
        ClassifierReturns(_ => Suggest(5));

        var result = await Build(
            Imported("UBER *TRIP 123"),
            Imported("uber trip 456"),
            Imported("Uber Trip 789 outubro")).ExecuteAsync(Request());

        _classifierCalls.Single().Count.ShouldBe(1);
        result.Value!.Transactions.Count().ShouldBe(3);
        result.Value.Transactions.ShouldAllBe(t => t.CategoryId == 5);
        result.Value.TotalRead.ShouldBe(3);
    }

    [Fact]
    public async Task Entrada_e_saida_com_a_mesma_descricao_nao_sao_agrupadas()
    {
        ClassifierReturns(_ => Suggest(5));

        await Build(Imported("pix joao", "E"), Imported("pix joao", "I")).ExecuteAsync(Request());

        _classifierCalls.Single().Count.ShouldBe(2);
    }

    [Fact]
    public async Task Memoria_vale_mesmo_com_ia_desligada_no_pedido()
    {
        SetHistory(new CategorizedTransactionHistoryItem("internet", "E", 3, null, 90));
        ClassifierReturns(_ => new TransactionAiSuggestion(false, 0.25m, null, null));

        var result = await Build(Imported("internet"), Imported("outra coisa")).ExecuteAsync(Request(useAi: false));

        var transactions = result.Value!.Transactions.ToList();

        transactions.Single(t => t.Description == "internet").CategoryId.ShouldBe(3);
        transactions.Single(t => t.Description == "outra coisa").CategoryId.ShouldBeNull();
    }

    [Fact]
    public async Task Categoria_vinda_do_arquivo_nao_e_sobrescrita_pela_memoria()
    {
        SetHistory(new CategorizedTransactionHistoryItem("internet", "E", 3, null, 90));
        ClassifierReturns(_ => new TransactionAiSuggestion(false, 0.25m, null, null));

        var result = await Build(Imported("internet", categoryId: 12)).ExecuteAsync(Request());

        result.Value!.Transactions.Single().CategoryId.ShouldBe(12);
        result.Value.Transactions.Single().ClassificationSource.ShouldBeNull();
    }

    [Fact]
    public async Task Ordem_e_quantidade_sao_preservadas_com_memoria_ia_e_repetidas_misturadas()
    {
        SetHistory(new CategorizedTransactionHistoryItem("internet", "E", 3, null, 90));
        ClassifierReturns(t => Suggest(t.Description.StartsWith("a") ? 8 : 9));

        var parsed = new[]
        {
            Imported("a novo") with { TransactionDate = new DateTime(2026, 1, 1) },
            Imported("internet") with { TransactionDate = new DateTime(2026, 1, 2) },
            Imported("b novo") with { TransactionDate = new DateTime(2026, 1, 3) },
            Imported("a novo") with { TransactionDate = new DateTime(2026, 1, 4) }
        };

        var result = await Build(parsed).ExecuteAsync(Request());

        result.Value!.Transactions.Select(t => t.CategoryId).ShouldBe(new int?[] { 8, 3, 9, 8 });
        result.Value.TotalRead.ShouldBe(4);
    }

    [Fact]
    public async Task Falha_do_classificador_descarta_o_lote_como_antes()
    {
        SetHistory(new CategorizedTransactionHistoryItem("internet", "E", 3, null, 90));
        _classifier
            .Setup(c => c.SuggestBatchAsync(
                It.IsAny<int>(),
                It.IsAny<IReadOnlyList<ImportedTransactionResponse>>(),
                It.IsAny<bool>()))
            .ThrowsAsync(new InvalidOperationException("falha"));

        var result = await Build(Imported("internet"), Imported("farmacia")).ExecuteAsync(Request());

        result.Value!.Transactions.Select(t => t.Description).ShouldBe(new[] { "internet" });
        result.Value.TotalRead.ShouldBe(2);
    }

    [Fact]
    public async Task Mais_de_quinze_descricoes_unicas_sao_divididas_em_lotes()
    {
        ClassifierReturns(_ => Suggest(5));

        var parsed = Enumerable.Range(1, 20).Select(i => Imported($"loja {(char)('a' + i)}x")).ToArray();

        await Build(parsed).ExecuteAsync(Request());

        _classifierCalls.Select(c => c.Count).ShouldBe(new[] { 15, 5 });
    }
}
