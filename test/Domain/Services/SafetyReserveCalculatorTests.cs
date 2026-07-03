using Domain.Models.Planning;
using Domain.Services.Planning;
using Shouldly;

namespace Tests.Domain.Services;

public class SafetyReserveCalculatorTests
{
    private readonly SafetyReserveCalculator _sut = new();

    private static ProjectionMonth Month(decimal probableOutflow)
        => new(2026, 6, 0m, 0m, 0m, probableOutflow, 0m, 0m);

    [Fact]
    public void Sem_meses_usa_o_piso_minimo()
    {
        _sut.Calculate(new FinancialProjection(Array.Empty<ProjectionMonth>())).ShouldBe(100m);
    }

    [Fact]
    public void Usa_a_media_mensal_das_saidas()
    {
        var projection = new FinancialProjection(new[] { Month(1000m), Month(2000m) });

        _sut.Calculate(projection).ShouldBe(1500m);
    }

    [Fact]
    public void Nunca_fica_abaixo_do_piso()
    {
        var projection = new FinancialProjection(new[] { Month(40m), Month(60m) });

        _sut.Calculate(projection).ShouldBe(100m);
    }
}
