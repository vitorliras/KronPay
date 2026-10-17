using Domain.Entities.AI;
using Domain.Exceptions;
using Shouldly;

namespace Tests.Domain.Entities;

public class AiUsageLogTests
{
    [Fact]
    public void Cria_registro_valido_com_data_utc()
    {
        var log = new AiUsageLog(1, "TransactionImport", "OpenAI", "gpt-6-luna", 1000, 200, 300, 0.0002m);

        log.UserId.ShouldBe(1);
        log.Feature.ShouldBe("TransactionImport");
        log.InputTokens.ShouldBe(1000);
        log.CachedInputTokens.ShouldBe(200);
        log.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public void Usuario_invalido_lanca_excecao_de_dominio()
    {
        Should.Throw<DomainException>(() =>
            new AiUsageLog(0, "TransactionImport", "OpenAI", "gpt-6-luna", 1, 0, 1, 0m));
    }

    [Theory]
    [InlineData("", "OpenAI", "gpt-6-luna")]
    [InlineData("TransactionImport", " ", "gpt-6-luna")]
    [InlineData("TransactionImport", "OpenAI", "")]
    public void Feature_provedor_e_modelo_sao_obrigatorios(string feature, string provider, string model)
    {
        Should.Throw<DomainException>(() =>
            new AiUsageLog(1, feature, provider, model, 1, 0, 1, 0m));
    }

    [Fact]
    public void Cache_maior_que_a_entrada_lanca_excecao_de_dominio()
    {
        Should.Throw<DomainException>(() =>
            new AiUsageLog(1, "TransactionImport", "OpenAI", "gpt-6-luna", 100, 101, 1, 0m));
    }

    [Fact]
    public void Valores_negativos_lancam_excecao_de_dominio()
    {
        Should.Throw<DomainException>(() =>
            new AiUsageLog(1, "TransactionImport", "OpenAI", "gpt-6-luna", -1, 0, 1, 0m));
        Should.Throw<DomainException>(() =>
            new AiUsageLog(1, "TransactionImport", "OpenAI", "gpt-6-luna", 1, 0, 1, -0.01m));
    }
}
