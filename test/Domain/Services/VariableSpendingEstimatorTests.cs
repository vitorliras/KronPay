using Domain.Enums.Planning;
using Domain.Services.Planning;
using Shouldly;

namespace Tests.Domain.Services;

public class VariableSpendingEstimatorTests
{
    private readonly VariableSpendingEstimator _sut = new();

    [Fact]
    public void Sem_historico_usa_fixos_vezes_fator_com_confianca_baixa()
    {
        var result = _sut.Estimate(Array.Empty<decimal>(), fixedMonthlyTotal: 2000m);

        result.MonthlyAmount.ShouldBe(3000m);
        result.Confidence.ShouldBe(ConfidenceLevel.Low);
    }

    [Fact]
    public void Tres_ou_mais_meses_usa_media_com_confianca_media()
    {
        var result = _sut.Estimate(new[] { 1000m, 1500m, 500m }, fixedMonthlyTotal: 0m);

        result.MonthlyAmount.ShouldBe(1000m);
        result.Confidence.ShouldBe(ConfidenceLevel.Medium);
    }

    [Fact]
    public void Poucos_meses_usa_media_real_com_confianca_baixa()
    {
        var result = _sut.Estimate(new[] { 1000m, 500m }, fixedMonthlyTotal: 0m);

        result.MonthlyAmount.ShouldBe(750m);
        result.Confidence.ShouldBe(ConfidenceLevel.Low);
    }

    [Fact]
    public void Media_e_arredondada_para_duas_casas()
    {
        var result = _sut.Estimate(new[] { 1000m, 2000m, 500m }, fixedMonthlyTotal: 0m);

        result.MonthlyAmount.ShouldBe(1166.67m);
        result.Confidence.ShouldBe(ConfidenceLevel.Medium);
    }
}
