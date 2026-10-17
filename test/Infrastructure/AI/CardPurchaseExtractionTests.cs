using Application.Abstractions.AI;
using Application.Services.Card;
using Infrastructure.AI.Cards;
using Moq;
using Shared.AI;
using Shouldly;

namespace Tests.Infrastructure.AI;

public class ExtractedCardPurchaseValidatorTests
{
    private static readonly DateTime Today = new(2026, 10, 16);

    private static ExtractedCardPurchaseDraft Draft(int? installments = 1, string? description = "loja", string? date = "2026-10-15") =>
        new(date, description, 50m, installments);

    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 3)]
    [InlineData(48, 48)]
    [InlineData(49, 1)]
    [InlineData(0, 1)]
    [InlineData(-2, 1)]
    public void Parcelas_validas_ficam_e_invalidas_viram_uma(int installments, int expected)
    {
        ExtractedCardPurchaseValidator.Validate(new[] { Draft(installments) }, Today).Single().InstallmentsCount.ShouldBe((short)expected);
    }

    [Fact]
    public void Parcelas_ausentes_viram_uma()
    {
        ExtractedCardPurchaseValidator.Validate(new[] { Draft(installments: null) }, Today).Single().InstallmentsCount.ShouldBe((short)1);
    }

    [Fact]
    public void Descricao_e_truncada_em_60_caracteres()
    {
        var result = ExtractedCardPurchaseValidator.Validate(new[] { Draft(description: new string('a', 200)) }, Today);

        result.Single().Description.Length.ShouldBe(60);
    }

    [Fact]
    public void Linhas_invalidas_sao_descartadas()
    {
        var result = ExtractedCardPurchaseValidator.Validate(
            new[] { Draft(date: "lixo"), Draft(description: ""), Draft(description: "ok") },
            Today);

        result.Select(r => r.Description).ShouldBe(new[] { "ok" });
    }
}

public class LlmCardPurchaseExtractionServiceTests
{
    private readonly Mock<IAiCompletionService> _completion = new();

    private LlmCardPurchaseExtractionService Build(string responseText)
    {
        _completion
            .Setup(c => c.CompleteAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiCompletionResult(responseText, 10, 0, 10, 0m));

        return new LlmCardPurchaseExtractionService(_completion.Object);
    }

    private static string Json(int installments) =>
        $"```json\n[{{\"date\":\"{DateTime.Today.AddDays(-1):yyyy-MM-dd}\",\"description\":\"tenis\",\"amount\":300,\"installments\":{installments}}}]\n```";

    [Fact]
    public async Task Texto_preserva_as_parcelas_informadas()
    {
        var result = await Build(Json(3)).ExtractFromTextAsync(1, "tenis 300 em 3x", AiFeatures.CardTextEntry);

        result.Single().InstallmentsCount.ShouldBe((short)3);
    }

    [Fact]
    public async Task Tabela_e_arquivo_forcam_uma_parcela_pois_a_fatura_mostra_so_o_valor_da_linha()
    {
        var table = await Build(Json(10)).ExtractFromTableAsync(1, "t", AiFeatures.CardDocumentImport);
        var file = await Build(Json(10)).ExtractFromFileAsync(
            1, new AiAttachment(new byte[] { 1 }, "application/pdf", "d.pdf"), AiFeatures.CardDocumentImport);

        table.Single().InstallmentsCount.ShouldBe((short)1);
        file.Single().InstallmentsCount.ShouldBe((short)1);
    }

    [Fact]
    public async Task Resposta_sem_json_lanca_excecao()
    {
        await Should.ThrowAsync<InvalidOperationException>(() =>
            Build("nao entendi").ExtractFromTextAsync(1, "x", AiFeatures.CardTextEntry));
    }

    [Fact]
    public async Task Envia_a_funcionalidade_e_o_anexo_para_o_medidor()
    {
        AiCompletionRequest? sent = null;
        var service = Build("[]");
        _completion
            .Setup(c => c.CompleteAsync(It.IsAny<AiCompletionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AiCompletionRequest, CancellationToken>((r, _) => sent = r)
            .ReturnsAsync(new AiCompletionResult("[]", 1, 0, 1, 0m));

        await service.ExtractFromFileAsync(
            7, new AiAttachment(new byte[] { 1 }, "image/png", "i.png"), AiFeatures.CardDocumentImport);

        sent!.UserId.ShouldBe(7);
        sent.Feature.ShouldBe(AiFeatures.CardDocumentImport);
        sent.Attachments!.Single().MediaType.ShouldBe("image/png");
    }
}
