using Domain.Exceptions;
using Domain.ValueObjects.Planning;
using Shouldly;

namespace Tests.Domain.ValueObjects;

public class RecurrenceTests
{
    [Fact]
    public void Mensal_gera_uma_ocorrencia_por_mes_no_intervalo()
    {
        var sut = new Recurrence("M", new DateTime(2026, 1, 10));

        var occ = sut.OccurrencesBetween(new DateTime(2026, 1, 1), new DateTime(2026, 3, 31)).ToList();

        occ.Count.ShouldBe(3);
        occ[0].ShouldBe(new DateTime(2026, 1, 10));
        occ[1].ShouldBe(new DateTime(2026, 2, 10));
        occ[2].ShouldBe(new DateTime(2026, 3, 10));
    }

    [Fact]
    public void Mensal_com_dia_inexistente_e_ajustado_para_o_ultimo_dia()
    {
        var sut = new Recurrence("M", new DateTime(2026, 1, 31));

        var occ = sut.OccurrencesBetween(new DateTime(2026, 1, 1), new DateTime(2026, 3, 31)).ToList();

        occ[1].ShouldBe(new DateTime(2026, 2, 28));
        occ[2].ShouldBe(new DateTime(2026, 3, 31));
    }

    [Fact]
    public void Unico_retorna_uma_ocorrencia_quando_dentro_do_intervalo()
    {
        var sut = new Recurrence("U", new DateTime(2026, 5, 20));

        sut.OccurrencesBetween(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31))
            .ShouldHaveSingleItem()
            .ShouldBe(new DateTime(2026, 5, 20));
    }

    [Fact]
    public void Unico_fora_do_intervalo_nao_retorna_nada()
    {
        var sut = new Recurrence("U", new DateTime(2025, 5, 20));

        sut.OccurrencesBetween(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31))
            .ShouldBeEmpty();
    }

    [Fact]
    public void Semanal_gera_a_cada_sete_dias()
    {
        var sut = new Recurrence("S", new DateTime(2026, 1, 1));

        var occ = sut.OccurrencesBetween(new DateTime(2026, 1, 1), new DateTime(2026, 1, 21)).ToList();

        occ.ShouldBe(new[]
        {
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 8),
            new DateTime(2026, 1, 15)
        });
    }

    [Fact]
    public void Data_final_limita_as_ocorrencias()
    {
        var sut = new Recurrence("M", new DateTime(2026, 1, 10), new DateTime(2026, 2, 15));

        var occ = sut.OccurrencesBetween(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31)).ToList();

        occ.Count.ShouldBe(2);
        occ[^1].ShouldBe(new DateTime(2026, 2, 10));
    }

    [Fact]
    public void Periodicidade_invalida_lanca_dominio()
    {
        Should.Throw<DomainException>(() => new Recurrence("X", new DateTime(2026, 1, 1)));
    }

    [Fact]
    public void Data_final_anterior_a_inicial_lanca_dominio()
    {
        Should.Throw<DomainException>(() =>
            new Recurrence("M", new DateTime(2026, 5, 1), new DateTime(2026, 4, 1)));
    }
}
