using Domain.Services.Planning;
using Shouldly;

namespace Tests.Domain.Services;

public class VariableSpendingEstimatorTests
{
    private readonly VariableSpendingEstimator _sut = new();

    [Fact]
    public void Sem_historico_retorna_zeros()
    {
        _sut.Estimate(Array.Empty<decimal>(), 3).ShouldBe(new[] { 0m, 0m, 0m });
    }

    [Fact]
    public void Horizonte_zero_retorna_vazio()
    {
        _sut.Estimate(new[] { 100m }, 0).ShouldBeEmpty();
    }

    [Fact]
    public void Historico_estavel_repete_a_media()
    {
        var result = _sut.Estimate(new[] { 200m, 200m, 200m }, 2);

        result.Count.ShouldBe(2);
        result.ShouldAllBe(v => v == 200m);
    }

    [Fact]
    public void Pondera_meses_recentes_com_maior_peso()
    {
        var result = _sut.Estimate(new[] { 100m, 200m }, 1);

        result[0].ShouldBe(166.67m);
    }

    [Fact]
    public void Tendencia_de_alta_e_limitada()
    {
        var result = _sut.Estimate(new[] { 100m, 100m, 100m, 300m, 300m, 300m }, 1);

        result[0].ShouldBeGreaterThan(242.86m);
        result[0].ShouldBeLessThanOrEqualTo(280m);
    }
}
