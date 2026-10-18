using Application.Services.AiChat;
using Shouldly;

namespace Tests.Application.AiChat;

public class ScreenFilterBuilderTests
{
    private static readonly DateTime Today = new(2026, 10, 16);
    private readonly ScreenFilterBuilder _builder = new();

    [Fact]
    public void Dashboard_aceita_mes_e_ano_e_usa_o_ano_atual_quando_so_o_mes()
    {
        var result = _builder.Build("dashboard", new ScreenFilterInput(Month: 8), Today);

        result.QueryParams["month"].ShouldBe("8");
        result.QueryParams["year"].ShouldBe("2026");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Mes_invalido_e_ignorado(int month)
    {
        var result = _builder.Build("dashboard", new ScreenFilterInput(Month: month), Today);

        result.QueryParams.ShouldBeEmpty();
        result.Ignored.ShouldNotBeEmpty();
    }

    [Fact]
    public void Ano_fora_do_intervalo_e_ignorado()
    {
        _builder.Build("dashboard", new ScreenFilterInput(Month: 1, Year: 1990), Today).QueryParams.ShouldBeEmpty();
        _builder.Build("dashboard", new ScreenFilterInput(Month: 1, Year: 2040), Today).QueryParams.ShouldBeEmpty();
    }

    [Fact]
    public void Relatorio_aceita_aba_e_periodo_e_completa_o_fim_com_hoje()
    {
        var result = _builder.Build("reports", new ScreenFilterInput(Tab: "Expense", StartDate: new DateTime(2026, 9, 1)), Today);

        result.QueryParams["tab"].ShouldBe("expense");
        result.QueryParams["startDate"].ShouldBe("2026-09-01");
        result.QueryParams["endDate"].ShouldBe("2026-10-16");
    }

    [Fact]
    public void Periodo_invertido_gigante_ou_so_com_fim_e_ignorado()
    {
        _builder.Build("reports", new ScreenFilterInput(StartDate: new DateTime(2026, 9, 30), EndDate: new DateTime(2026, 9, 1)), Today)
            .QueryParams.ShouldBeEmpty();

        _builder.Build("reports", new ScreenFilterInput(StartDate: new DateTime(2024, 1, 1), EndDate: new DateTime(2026, 1, 1)), Today)
            .QueryParams.ShouldBeEmpty();

        _builder.Build("reports", new ScreenFilterInput(EndDate: new DateTime(2026, 9, 30)), Today)
            .QueryParams.ShouldBeEmpty();
    }

    [Fact]
    public void Aba_desconhecida_e_ignorada_e_cada_tela_so_aceita_as_suas_abas()
    {
        _builder.Build("reports", new ScreenFilterInput(Tab: "payments"), Today).QueryParams.ShouldBeEmpty();
        _builder.Build("config", new ScreenFilterInput(Tab: "payments"), Today).QueryParams["tab"].ShouldBe("payments");
        _builder.Build("transactions", new ScreenFilterInput(Tab: "expense"), Today).QueryParams.ShouldBeEmpty();
    }

    [Fact]
    public void Planejamento_so_aceita_horizontes_validos()
    {
        _builder.Build("planning", new ScreenFilterInput(HorizonMonths: 24), Today).QueryParams["horizonMonths"].ShouldBe("24");
        _builder.Build("planning", new ScreenFilterInput(HorizonMonths: 7), Today).QueryParams.ShouldBeEmpty();
    }

    [Fact]
    public void Busca_vazia_ou_gigante_e_ignorada()
    {
        _builder.Build("transactions", new ScreenFilterInput(Search: "  "), Today).QueryParams.ShouldBeEmpty();
        _builder.Build("transactions", new ScreenFilterInput(Search: new string('a', 100)), Today).QueryParams.ShouldBeEmpty();
        _builder.Build("transactions", new ScreenFilterInput(Search: " uber "), Today).QueryParams["search"].ShouldBe("uber");
    }

    [Fact]
    public void Filtro_de_outra_tela_e_ignorado_e_telas_sem_filtro_nao_aplicam_nada()
    {
        var wrong = _builder.Build("planning", new ScreenFilterInput(Month: 3, CardId: 5), Today);
        wrong.QueryParams.ShouldBeEmpty();
        wrong.Ignored.ShouldNotBeEmpty();

        var none = _builder.Build("cards", new ScreenFilterInput(Month: 3), Today);
        none.QueryParams.ShouldBeEmpty();
        none.Ignored.ShouldNotBeEmpty();

        var empty = _builder.Build("cards", new ScreenFilterInput(), Today);
        empty.Ignored.ShouldBeEmpty();
    }

    [Fact]
    public void Tipo_so_aceita_E_I_V()
    {
        _builder.Build("transactions", new ScreenFilterInput(Type: "i"), Today).QueryParams["type"].ShouldBe("I");
        _builder.Build("transactions", new ScreenFilterInput(Type: "R"), Today).QueryParams.ShouldBeEmpty();
    }
}
