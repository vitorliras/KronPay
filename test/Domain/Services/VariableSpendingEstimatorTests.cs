using Domain.Enums.Planning;
using Domain.Services.Planning;
using Shouldly;

namespace Tests.Domain.Services;

public class VariableSpendingEstimatorTests
{
    private readonly VariableSpendingEstimator _sut = new();

    [Fact]
    public void Sem_historico_nao_assume_nenhum_desvio()
    {
        var result = _sut.Estimate(Array.Empty<decimal>());

        result.CentralDeviation.ShouldBe(0m);
        result.Confidence.ShouldBe(ConfidenceLevel.Low);
    }

    [Fact]
    public void Desvio_positivo_consistente_e_reconhecido_com_confianca_media()
    {
        var result = _sut.Estimate(new[] { 1000m, 1000m, 1000m });

        result.CentralDeviation.ShouldBe(1000m);
        result.Confidence.ShouldBe(ConfidenceLevel.Medium);
    }

    [Fact]
    public void Desvio_negativo_consistente_tambem_e_reconhecido()
    {
        var result = _sut.Estimate(new[] { -500m, -500m, -500m });

        result.CentralDeviation.ShouldBe(-500m);
        result.Confidence.ShouldBe(ConfidenceLevel.Medium);
    }

    [Fact]
    public void Tres_ou_mais_meses_usa_confianca_media()
    {
        var result = _sut.Estimate(new[] { 1000m, 1500m, 500m });

        result.Confidence.ShouldBe(ConfidenceLevel.Medium);
    }

    [Fact]
    public void Poucos_meses_usa_confianca_baixa()
    {
        var result = _sut.Estimate(new[] { 1000m, 500m });

        result.Confidence.ShouldBe(ConfidenceLevel.Low);
    }

    [Fact]
    public void Comportamento_muito_instavel_usa_confianca_baixa_mesmo_com_varios_meses()
    {
        var result = _sut.Estimate(new[] { 10000m, -10000m, 100m, -9000m, 9500m });

        result.Confidence.ShouldBe(ConfidenceLevel.Low);
    }

    [Fact]
    public void Meses_recentes_pesam_mais_que_meses_antigos()
    {
        var pioraRecente = _sut.Estimate(new[] { 500m, 500m, 1500m, 1500m });
        var melhoraRecente = _sut.Estimate(new[] { 1500m, 1500m, 500m, 500m });

        pioraRecente.CentralDeviation.ShouldBeGreaterThan(1000m);
        melhoraRecente.CentralDeviation.ShouldBeLessThan(1000m);
        pioraRecente.CentralDeviation.ShouldBeGreaterThan(melhoraRecente.CentralDeviation);
    }

    [Fact]
    public void Um_mes_antigo_com_valor_extremo_nao_distorce_o_resultado()
    {
        var result = _sut.Estimate(new[] { 170000m, 500m, 500m, 500m, 500m, 500m, 500m });

        result.CentralDeviation.ShouldBe(500m);
    }
}
