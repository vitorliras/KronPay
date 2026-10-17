using Tests.Support;
using Application.Services.Transactions;
using Domain.Entities.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Transactions;
using Domain.Services.Transactions;
using Infrastructure.Services.Assistant;
using Microsoft.Extensions.Configuration;
using Moq;
using Shouldly;

namespace Tests.Application.Transactions;

public class TransactionCategoryMemoryTests
{
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly Mock<ICategoryItemRepository> _items = new();

    private static T WithId<T>(T entity, int id) where T : class
    {
        typeof(T).GetProperty("Id")!.SetValue(entity, id);
        return entity;
    }

    private static Category Category(int id) => WithId(new Category(1, $"categoria {id}", "E"), id);

    private static CategorizedTransactionHistoryItem History(
        string description,
        string type,
        int categoryId,
        int occurrences,
        int? itemId = null) =>
        new(description, type, categoryId, itemId, occurrences);

    private async Task<TransactionCategoryMemory> Build(
        IEnumerable<CategorizedTransactionHistoryItem> history,
        IEnumerable<Category>? categories = null,
        IEnumerable<CategoryItem>? items = null)
    {
        _transactions
            .Setup(t => t.GetCategorizedHistoryAsync(It.IsAny<int>()))
            .ReturnsAsync(history.ToList());

        _items
            .Setup(i => i.GetAllByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(items ?? Array.Empty<CategoryItem>());

        var fuzzy = new LevenshteinParameterFuzzyMatcher(new ConfigurationBuilder().Build());
        var builder = new TransactionCategoryMemoryBuilder(_transactions.Object, CardPurchaseRepositoryStub.Empty(), _items.Object, fuzzy);

        var activeCategories = categories ?? Enumerable.Range(1, 20).Select(Category);

        return await builder.BuildAsync(1, activeCategories);
    }

    [Fact]
    public async Task Descricao_igual_retorna_a_categoria_do_historico()
    {
        var memory = await Build(new[] { History("internet", "E", 3, 90) });

        var suggestion = memory.Find("Internet", "E");

        suggestion.ShouldNotBeNull();
        suggestion.CategoryId.ShouldBe(3);
        suggestion.Type.ShouldBe("E");
    }

    [Fact]
    public async Task Descricao_com_data_e_mes_casa_com_a_versao_simples()
    {
        var memory = await Build(new[] { History("uber", "E", 5, 3) });

        memory.Find("UBER OUTUBRO 25/10/2025", "E")?.CategoryId.ShouldBe(5);
    }

    [Fact]
    public async Task Descricao_nova_nao_retorna_sugestao()
    {
        var memory = await Build(new[] { History("internet", "E", 3, 90) });

        memory.Find("farmacia", "E").ShouldBeNull();
    }

    [Fact]
    public async Task Entrada_e_saida_nao_se_misturam()
    {
        var memory = await Build(new[] { History("salario", "I", 2, 10) });

        memory.Find("salario", "I").ShouldNotBeNull();
        memory.Find("salario", "E").ShouldBeNull();
    }

    [Fact]
    public async Task Investimento_no_historico_vale_para_saida_do_extrato_e_devolve_o_tipo_final()
    {
        var memory = await Build(new[] { History("tesouro direto", "V", 9, 4) });

        var suggestion = memory.Find("tesouro direto", "E");

        suggestion.ShouldNotBeNull();
        suggestion.Type.ShouldBe("V");
        suggestion.CategoryId.ShouldBe(9);
    }

    [Fact]
    public async Task Categoria_dominante_com_60_por_cento_e_aceita()
    {
        var memory = await Build(new[]
        {
            History("mercado", "E", 1, 6),
            History("mercado", "E", 2, 4)
        });

        memory.Find("mercado", "E")?.CategoryId.ShouldBe(1);
    }

    [Fact]
    public async Task Categoria_sem_dominancia_suficiente_nao_gera_sugestao()
    {
        var memory = await Build(new[]
        {
            History("mercado", "E", 1, 5),
            History("mercado", "E", 2, 5)
        });

        memory.Find("mercado", "E").ShouldBeNull();
    }

    [Fact]
    public async Task Erro_de_digitacao_casa_quando_ha_ocorrencias_suficientes()
    {
        var memory = await Build(new[] { History("energia", "E", 3, 10) });

        memory.Find("energoa", "E")?.CategoryId.ShouldBe(3);
    }

    [Fact]
    public async Task Erro_de_digitacao_com_uma_unica_ocorrencia_nao_casa()
    {
        var memory = await Build(new[] { History("aluguel", "E", 4, 1) });

        memory.Find("alguel", "E").ShouldBeNull();
        memory.Find("aluguel", "E")?.CategoryId.ShouldBe(4);
    }

    [Fact]
    public async Task Descricao_curta_nao_usa_busca_aproximada()
    {
        var memory = await Build(new[] { History("agua", "E", 6, 20) });

        memory.Find("agoa", "E").ShouldBeNull();
    }

    [Fact]
    public async Task Descricao_menor_contida_na_maior_nao_casa()
    {
        var memory = await Build(new[] { History("mercado livre", "E", 7, 12) });

        memory.Find("mercado", "E").ShouldBeNull();
    }

    [Fact]
    public async Task Categoria_inativa_ou_inexistente_e_ignorada()
    {
        var memory = await Build(
            new[] { History("internet", "E", 99, 90) },
            categories: new[] { Category(1), Category(2) });

        memory.Find("internet", "E").ShouldBeNull();
    }

    [Fact]
    public async Task Subcategoria_ativa_da_mesma_categoria_e_mantida()
    {
        var item = WithId(new CategoryItem(3, "fibra"), 11);

        var memory = await Build(
            new[] { History("internet", "E", 3, 10, itemId: 11) },
            items: new[] { item });

        memory.Find("internet", "E")?.CategoryItemId.ShouldBe(11);
    }

    [Fact]
    public async Task Subcategoria_desativada_e_descartada_mas_a_categoria_permanece()
    {
        var memory = await Build(new[] { History("internet", "E", 3, 10, itemId: 11) });

        var suggestion = memory.Find("internet", "E");

        suggestion.ShouldNotBeNull();
        suggestion.CategoryId.ShouldBe(3);
        suggestion.CategoryItemId.ShouldBeNull();
    }

    [Fact]
    public async Task Variacoes_da_mesma_descricao_somam_ocorrencias()
    {
        var memory = await Build(new[]
        {
            History("Salário Lany", "I", 2, 20),
            History("salario lany", "I", 2, 34),
            History("salario lany", "I", 8, 2)
        });

        memory.Find("SALARIO LANY", "I")?.CategoryId.ShouldBe(2);
    }
}
