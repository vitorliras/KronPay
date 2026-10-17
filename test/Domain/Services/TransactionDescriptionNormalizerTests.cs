using Domain.Services.Transactions;
using Shouldly;

namespace Tests.Domain.Services;

public class TransactionDescriptionNormalizerTests
{
    [Theory]
    [InlineData("uber", "uber")]
    [InlineData("UBER OUTUBRO 25/10/2025", "uber")]
    [InlineData("uber novembro", "uber")]
    [InlineData("Salário Lany", "salario lany")]
    [InlineData("SALARIO   LANY", "salario lany")]
    [InlineData("IFOOD *REST 12345", "ifood rest")]
    [InlineData("Cartão de Crédito Itau Vitor", "cartao de credito itau vitor")]
    [InlineData("  Março  ", "")]
    [InlineData("pix enviado joao 01/12", "pix enviado joao")]
    public void Normaliza_descricao_removendo_acento_digitos_pontuacao_e_meses(string input, string expected)
    {
        TransactionDescriptionNormalizer.Normalize(input).ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345 / 2025")]
    public void Descricao_vazia_ou_so_ruido_vira_texto_vazio(string? input)
    {
        TransactionDescriptionNormalizer.Normalize(input).ShouldBeEmpty();
    }

    [Fact]
    public void Abreviacao_de_mes_nao_e_removida()
    {
        TransactionDescriptionNormalizer.Normalize("uber out").ShouldBe("uber out");
        TransactionDescriptionNormalizer.Normalize("conta mar").ShouldBe("conta mar");
    }

    [Fact]
    public void Descricoes_com_e_sem_variacao_geram_a_mesma_chave()
    {
        var simple = TransactionDescriptionNormalizer.Normalize("uber");
        var noisy = TransactionDescriptionNormalizer.Normalize("UBER OUTUBRO 25/10/2025");

        noisy.ShouldBe(simple);
    }

    [Theory]
    [InlineData("E", true)]
    [InlineData("V", true)]
    [InlineData("I", false)]
    [InlineData("R", false)]
    public void Direcao_considera_despesa_e_investimento_como_saida(string type, bool expected)
    {
        TransactionDescriptionNormalizer.IsOutflow(type).ShouldBe(expected);
    }
}
