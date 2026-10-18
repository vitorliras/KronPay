using Application.Abstractions.AI;
using Application.Configuration.AI;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Infrastructure.AI.Transactions;
using Infrastructure.AI.Transactions.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shared.AI;
using Shouldly;

namespace Tests.Infrastructure.AI;

public class TransactionAiClassifierTests
{
    private readonly Mock<IAiCompletionService> _completion = new();
    private readonly Mock<IAiAvailability> _availability = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<ICategoryItemRepository> _categoryItems = new();

    private TransactionAiClassifier Build(params CategoryItem[] items)
    {
        _categories
            .Setup(c => c.GetAllAsync(It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<Category>());

        _categoryItems
            .Setup(c => c.GetAllByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(items);

        var llm = new LlmTransactionBatchClassifier(_completion.Object, _categories.Object, _categoryItems.Object);

        return new TransactionAiClassifier(
            llm,
            new FallbackTransactionClassifier(),
            _availability.Object,
            NullLogger<TransactionAiClassifier>.Instance);
    }

    private void EnableAi() =>
        _availability
            .Setup(a => a.GetActive())
            .Returns(new ActiveAiProvider("OpenAI", new AiProviderSettings { Model = "gpt-6-luna" }));

    private static List<ImportedTransactionResponse> Batch(int count) =>
        Enumerable.Range(1, count)
            .Select(i => new ImportedTransactionResponse(
                DateTime.Today, 10m, $"COMPRA {i}", "E", "P", 1, null, null, null, null))
            .ToList();

    private static AiCompletionResult Completion(string text) => new(text, 100, 0, 50, 0m);

    [Fact]
    public async Task Sem_pedir_ia_usa_o_fallback_e_nao_chama_a_ia()
    {
        EnableAi();

        var result = await Build().SuggestBatchAsync(1, Batch(2), useAi: false);

        result.Count.ShouldBe(2);
        _completion.Verify(c => c.CompleteAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Ia_desligada_usa_o_fallback_mesmo_pedindo_ia()
    {
        _availability.Setup(a => a.GetActive()).Returns((ActiveAiProvider?)null);

        var result = await Build().SuggestBatchAsync(1, Batch(2), useAi: true);

        result.Count.ShouldBe(2);
        _completion.Verify(c => c.CompleteAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Erro_da_ia_cai_no_fallback_sem_propagar_excecao()
    {
        EnableAi();
        _completion
            .Setup(c => c.CompleteAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("falha"));

        var result = await Build().SuggestBatchAsync(1, Batch(3), useAi: true);

        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Resposta_sem_json_cai_no_fallback()
    {
        EnableAi();
        _completion
            .Setup(c => c.CompleteAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Completion("não sei classificar"));

        var result = await Build().SuggestBatchAsync(1, Batch(2), useAi: true);

        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Resposta_valida_da_ia_e_mapeada_por_indice()
    {
        EnableAi();
        const string json = """
            ```json
            [
              {"index": 2, "isInvestment": false, "confidence": 0.9, "suggestedCategoryId": 7, "suggestedType": "E"},
              {"index": 1, "isInvestment": true, "confidence": 0.8, "suggestedCategoryId": 3, "suggestedType": "V"}
            ]
            ```
            """;
        _completion
            .Setup(c => c.CompleteAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Completion(json));

        var result = await Build().SuggestBatchAsync(1, Batch(2), useAi: true);

        result[0].SuggestedCategoryId.ShouldBe(3);
        result[0].IsInvestment.ShouldBeTrue();
        result[1].SuggestedCategoryId.ShouldBe(7);
        result[1].Confidence.ShouldBe(0.9m);
    }

    private static CategoryItem Item(int id, int categoryId, string description)
    {
        var item = new CategoryItem(categoryId, description);
        typeof(CategoryItem).GetProperty(nameof(CategoryItem.Id))!.SetValue(item, id);
        return item;
    }

    [Fact]
    public async Task Subcategoria_sugerida_so_vale_se_pertencer_a_categoria_sugerida()
    {
        EnableAi();
        const string json = """
            [
              {"index": 1, "isInvestment": false, "confidence": 0.9, "suggestedCategoryId": 5, "suggestedCategoryItemId": 12, "suggestedType": "E"},
              {"index": 2, "isInvestment": false, "confidence": 0.9, "suggestedCategoryId": 5, "suggestedCategoryItemId": 99, "suggestedType": "E"},
              {"index": 3, "isInvestment": false, "confidence": 0.9, "suggestedCategoryId": 6, "suggestedCategoryItemId": 12, "suggestedType": "E"}
            ]
            """;
        _completion
            .Setup(c => c.CompleteAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Completion(json));

        var result = await Build(Item(12, 5, "Feira")).SuggestBatchAsync(1, Batch(3), useAi: true);

        result[0].SuggestedCategoryItemId.ShouldBe(12);
        result[1].SuggestedCategoryItemId.ShouldBeNull();
        result[2].SuggestedCategoryItemId.ShouldBeNull();
    }

    [Fact]
    public async Task Indice_ausente_na_resposta_fica_sem_classificacao()
    {
        EnableAi();
        const string json = """[{"index": 1, "isInvestment": false, "confidence": 0.9, "suggestedCategoryId": 5, "suggestedType": "E"}]""";
        _completion
            .Setup(c => c.CompleteAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Completion(json));

        var result = await Build().SuggestBatchAsync(1, Batch(2), useAi: true);

        result[0].SuggestedCategoryId.ShouldBe(5);
        result[1].Confidence.ShouldBe(0m);
        result[1].SuggestedCategoryId.ShouldBeNull();
    }

    [Fact]
    public async Task Chamada_da_ia_informa_usuario_e_funcionalidade_para_o_registro_de_consumo()
    {
        EnableAi();
        AiCompletionRequest? captured = null;
        _completion
            .Setup(c => c.CompleteAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AiCompletionRequest, CancellationToken>((r, _) => captured = r)
            .ReturnsAsync(Completion("[]"));

        await Build().SuggestBatchAsync(42, Batch(1), useAi: true);

        captured.ShouldNotBeNull();
        captured.UserId.ShouldBe(42);
        captured.Feature.ShouldBe(AiFeatures.TransactionImport);
    }
}
