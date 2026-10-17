using Application.Configuration.AI;
using Application.Services.AI;
using Shouldly;

namespace Tests.Application.AI;

public class AiCostCalculatorTests
{
    private static readonly AiProviderSettings Luna = new()
    {
        InputPricePerMillion = 0.10m,
        CachedInputPricePerMillion = 0.01m,
        OutputPricePerMillion = 0.50m
    };

    [Fact]
    public void Um_milhao_de_tokens_de_entrada_custa_o_preco_de_entrada()
    {
        AiCostCalculator.Calculate(Luna, 1_000_000, 0, 0).ShouldBe(0.10m);
    }

    [Fact]
    public void Um_milhao_de_tokens_de_saida_custa_o_preco_de_saida()
    {
        AiCostCalculator.Calculate(Luna, 0, 0, 1_000_000).ShouldBe(0.50m);
    }

    [Fact]
    public void Tokens_em_cache_sao_cobrados_pelo_preco_de_cache_e_nao_pelo_de_entrada()
    {
        AiCostCalculator.Calculate(Luna, 1_000_000, 1_000_000, 0).ShouldBe(0.01m);
    }

    [Fact]
    public void Combina_entrada_nao_cacheada_cache_e_saida()
    {
        var cost = AiCostCalculator.Calculate(Luna, 1_000_000, 400_000, 200_000);

        cost.ShouldBe(0.06m + 0.004m + 0.10m);
    }

    [Fact]
    public void Sem_uso_o_custo_e_zero()
    {
        AiCostCalculator.Calculate(Luna, 0, 0, 0).ShouldBe(0m);
    }
}
