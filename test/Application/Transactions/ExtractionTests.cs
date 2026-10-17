using Tests.Support;
using Domain.Services.Transactions;
using Application.Abstractions.AI;
using Application.Abstractions.Common;
using Application.Configuration.AI;
using Application.Services.Transactions;
using Application.UseCases.Transactions;
using Application.DTOs.Transactions;
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

public class ExtractedTransactionValidatorTests
{
    private static readonly DateTime Today = new(2026, 10, 16);

    private static ExtractedTransactionDraft Draft(
        string? date = "2026-10-15",
        string? description = "uber",
        decimal? amount = 23.5m,
        string? type = "E") => new(date, description, amount, type);

    [Fact]
    public void Linha_valida_e_mantida()
    {
        var result = ExtractedTransactionValidator.Validate(new[] { Draft() }, Today);

        result.Single().Description.ShouldBe("uber");
        result.Single().Amount.ShouldBe(23.5m);
        result.Single().Type.ShouldBe("E");
    }

    [Theory]
    [InlineData("15/10/2026")]
    [InlineData("amanha")]
    [InlineData(null)]
    [InlineData("1990-01-01")]
    [InlineData("2099-01-01")]
    public void Data_invalida_ou_fora_da_faixa_descarta_a_linha(string? date)
    {
        ExtractedTransactionValidator.Validate(new[] { Draft(date: date) }, Today).ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Descricao_vazia_descarta_a_linha(string? description)
    {
        ExtractedTransactionValidator.Validate(new[] { Draft(description: description) }, Today).ShouldBeEmpty();
    }

    [Fact]
    public void Valor_ausente_descarta_a_linha()
    {
        ExtractedTransactionValidator.Validate(new[] { Draft(amount: null) }, Today).ShouldBeEmpty();
    }

    [Fact]
    public void Valor_zero_descarta_a_linha()
    {
        ExtractedTransactionValidator.Validate(new[] { Draft(amount: 0m) }, Today).ShouldBeEmpty();
    }

    [Fact]
    public void Valor_negativo_vira_positivo()
    {
        ExtractedTransactionValidator.Validate(new[] { Draft(amount: -50m) }, Today).Single().Amount.ShouldBe(50m);
    }

    [Theory]
    [InlineData("I", "I")]
    [InlineData("R", "I")]
    [InlineData("E", "E")]
    [InlineData("V", "E")]
    [InlineData("xyz", "E")]
    [InlineData(null, "E")]
    public void Tipo_e_normalizado_para_entrada_ou_saida(string? type, string expected)
    {
        ExtractedTransactionValidator.Validate(new[] { Draft(type: type) }, Today).Single().Type.ShouldBe(expected);
    }

    [Fact]
    public void Linhas_invalidas_nao_derrubam_as_validas()
    {
        var result = ExtractedTransactionValidator.Validate(
            new[] { Draft(description: "ifood"), Draft(date: "lixo"), Draft(description: "uber") },
            Today);

        result.Select(r => r.Description).ShouldBe(new[] { "ifood", "uber" });
    }

    [Fact]
    public void Descricao_longa_e_truncada_e_espacos_colapsados()
    {
        var result = ExtractedTransactionValidator.Validate(
            new[] { Draft(description: "  mercado    livre " + new string('x', 400)) },
            Today);

        result.Single().Description.Length.ShouldBeLessThanOrEqualTo(300);
        result.Single().Description.ShouldStartWith("mercado livre ");
    }
}

public class TabularTextChunkerTests
{
    [Fact]
    public void Sem_linhas_nao_gera_blocos()
    {
        TabularTextChunker.Chunk(Array.Empty<string>()).ShouldBeEmpty();
    }

    [Fact]
    public void Somente_cabecalho_gera_um_bloco()
    {
        TabularTextChunker.Chunk(new[] { "data | valor" }).ShouldBe(new[] { "data | valor" });
    }

    [Fact]
    public void Divide_em_blocos_repetindo_o_cabecalho()
    {
        var rows = new[] { "h" }.Concat(Enumerable.Range(1, 5).Select(i => $"r{i}")).ToList();

        var chunks = TabularTextChunker.Chunk(rows, rowsPerChunk: 2);

        chunks.Count.ShouldBe(3);
        chunks[0].ShouldBe("h\nr1\nr2");
        chunks[2].ShouldBe("h\nr5");
    }

    [Fact]
    public void Planilha_pequena_gera_um_unico_bloco()
    {
        var rows = new[] { "h" }.Concat(Enumerable.Range(1, 200).Select(i => $"r{i}")).ToList();

        TabularTextChunker.Chunk(rows).Count.ShouldBe(1);
    }
}

public class ParseTransactionsFromTextUseCaseTests
{
    private readonly Mock<IAiAvailability> _availability = new();
    private readonly Mock<ITransactionExtractionService> _extraction = new();
    private readonly Mock<ITransactionAiBatchClassifier> _classifier = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<ICategoryItemRepository> _items = new();
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    public ParseTransactionsFromTextUseCaseTests()
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
                (IReadOnlyList<TransactionAiSuggestion>)batch.Select(_ => new TransactionAiSuggestion(false, 0.9m, 4, "E")).ToList());
    }

    private void EnableAi() =>
        _availability.Setup(a => a.GetActive())
            .Returns(new ActiveAiProvider("OpenAI", new AiProviderSettings { Model = "m" }));

    private ParseTransactionsFromTextUseCase Build()
    {
        var memory = new TransactionCategoryMemoryBuilder(
            _transactions.Object, CardPurchaseRepositoryStub.Empty(),
            _items.Object,
            new LevenshteinParameterFuzzyMatcher(new ConfigurationBuilder().Build()));

        var classification = new ImportedTransactionClassificationService(
            memory,
            _classifier.Object,
            NullLogger<ImportedTransactionClassificationService>.Instance);

        return new ParseTransactionsFromTextUseCase(
            _availability.Object,
            _extraction.Object,
            classification,
            _categories.Object,
            _currentUser.Object,
            NullLogger<ParseTransactionsFromTextUseCase>.Instance);
    }

    [Fact]
    public async Task Ia_desligada_retorna_falha_sem_chamar_a_extracao()
    {
        _availability.Setup(a => a.GetActive()).Returns((ActiveAiProvider?)null);

        var result = await Build().ExecuteAsync(new ParseTransactionsFromTextRequest("ifood 45"));

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(MessageKeys.AiUnavailable);
        _extraction.Verify(e => e.ExtractFromTextAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Texto_extraido_vira_previa_classificada()
    {
        EnableAi();
        _extraction
            .Setup(e => e.ExtractFromTextAsync(1, "ifood 45, uber 23", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new ExtractedTransaction(new DateTime(2026, 10, 15), "ifood", 45m, "E"),
                new ExtractedTransaction(new DateTime(2026, 10, 16), "uber", 23m, "E")
            });

        var result = await Build().ExecuteAsync(new ParseTransactionsFromTextRequest("  ifood 45, uber 23  "));

        result.IsSuccess.ShouldBeTrue();
        result.Value!.TotalImported.ShouldBe(0);
        result.Value.Transactions.Select(t => t.Description).ShouldBe(new[] { "ifood", "uber" });
        result.Value.Transactions.ShouldAllBe(t => t.CategoryId == 4);
    }

    [Fact]
    public async Task Nada_extraido_retorna_mensagem_propria()
    {
        EnableAi();
        _extraction
            .Setup(e => e.ExtractFromTextAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ExtractedTransaction>());

        var result = await Build().ExecuteAsync(new ParseTransactionsFromTextRequest("bom dia"));

        result.Message.ShouldBe(MessageKeys.AiNothingExtracted);
    }

    [Fact]
    public async Task Erro_da_ia_retorna_mensagem_amigavel_sem_vazar_a_excecao()
    {
        EnableAi();
        _extraction
            .Setup(e => e.ExtractFromTextAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("segredo interno"));

        var result = await Build().ExecuteAsync(new ParseTransactionsFromTextRequest("ifood 45"));

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(MessageKeys.AiExtractionFailed);
    }
}
