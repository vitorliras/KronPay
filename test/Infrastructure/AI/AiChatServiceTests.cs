using Application.Abstractions.AI;
using Application.Abstractions.AiChat;
using Application.Configuration.AI;
using Domain.Entities.AI;
using Infrastructure.AI;
using Infrastructure.AI.Chat;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Moq;
using Shared.AI;
using Shouldly;

namespace Tests.Infrastructure.AI;

public class AiChatServiceTests
{
    private readonly Mock<IAiAvailability> _availability = new();
    private readonly Mock<IOptionsMonitor<AiSettings>> _settings = new();
    private readonly Mock<IAiUsageRecorder> _recorder = new();
    private readonly Mock<IChatToolbox> _toolbox = new();
    private readonly List<AiUsageLog> _logs = new();
    private readonly AiSettings _aiSettings = new()
    {
        Chat = new AiChatSettings { Model = "chat-model", MaxOutputTokens = 500, MaxToolIterations = 4 }
    };

    public AiChatServiceTests()
    {
        _settings.Setup(s => s.CurrentValue).Returns(_aiSettings);
        _availability
            .Setup(a => a.GetActive())
            .Returns(new ActiveAiProvider("OpenAI", new AiProviderSettings
            {
                Model = "base-model",
                BaseUrl = "https://x",
                ApiKey = "k",
                ToolReasoningEffort = "none",
                InputPricePerMillion = 0.10m,
                CachedInputPricePerMillion = 0.01m,
                OutputPricePerMillion = 0.50m
            }));

        _recorder
            .Setup(r => r.RecordAsync(It.IsAny<AiUsageLog>()))
            .Callback<AiUsageLog>(_logs.Add)
            .Returns(Task.CompletedTask);
    }

    private AiChatService Build(ScriptedChatClient client) =>
        new(_availability.Object, _settings.Object, new FakeFactory(client), _recorder.Object);

    private static ChatResponse Call(string name, IDictionary<string, object?> args, long input, long output, long cached = 0) =>
        new(new ChatMessage(ChatRole.Assistant, new List<AIContent> { new FunctionCallContent($"call-{name}", name, args) }))
        {
            Usage = new UsageDetails { InputTokenCount = input, OutputTokenCount = output, CachedInputTokenCount = cached }
        };

    private static ChatResponse Text(string text, long input, long output, long cached = 0) =>
        new(new ChatMessage(ChatRole.Assistant, text))
        {
            Usage = new UsageDetails { InputTokenCount = input, OutputTokenCount = output, CachedInputTokenCount = cached }
        };

    [Fact]
    public async Task Executa_a_ferramenta_pedida_pelo_modelo_e_devolve_a_resposta_final()
    {
        _toolbox.Setup(t => t.GetMonthlyTotals(3)).ReturnsAsync(ChatToolResult.Success(new[] { new { month = "2026-10", expenses = 10m } }));
        var client = new ScriptedChatClient(
            Call("get_monthly_totals", new Dictionary<string, object?> { ["months"] = 3 }, 100, 20, 10),
            Text("{\"reply\":\"ok\"}", 150, 30, 20));

        var result = await Build(client).RunAsync(
            1, "SYSTEM", new[] { new AiChatTurn("user", "quanto gastei?") }, _toolbox.Object);

        result.Text.ShouldContain("ok");
        result.ToolCalls.ShouldBe(1);
        _toolbox.Verify(t => t.GetMonthlyTotals(3), Times.Once);
    }

    [Fact]
    public async Task Resultado_da_ferramenta_chega_ao_modelo_na_segunda_chamada()
    {
        _toolbox.Setup(t => t.GetMonthlyTotals(It.IsAny<int>())).ReturnsAsync(ChatToolResult.Success(new[] { new { month = "2026-10", expenses = 123.45m } }));
        var client = new ScriptedChatClient(
            Call("get_monthly_totals", new Dictionary<string, object?> { ["months"] = 1 }, 10, 5),
            Text("{\"reply\":\"ok\"}", 10, 5));

        await Build(client).RunAsync(1, "SYSTEM", new[] { new AiChatTurn("user", "oi") }, _toolbox.Object);

        var secondCall = client.Calls[1];
        var toolResult = secondCall.SelectMany(m => m.Contents).OfType<FunctionResultContent>().Single();
        System.Text.Json.JsonSerializer.Serialize(toolResult.Result).ShouldContain("123.45");
    }

    [Fact]
    public async Task Soma_os_tokens_de_todas_as_chamadas_em_um_unico_registro_de_consumo_com_o_modelo_do_chat()
    {
        _toolbox.Setup(t => t.GetMonthlyTotals(It.IsAny<int>())).ReturnsAsync(ChatToolResult.Success());
        var client = new ScriptedChatClient(
            Call("get_monthly_totals", new Dictionary<string, object?> { ["months"] = 2 }, 100, 20, 10),
            Text("{\"reply\":\"ok\"}", 150, 30, 20));

        await Build(client).RunAsync(7, "SYSTEM", new[] { new AiChatTurn("user", "oi") }, _toolbox.Object);

        var log = _logs.Single();
        log.UserId.ShouldBe(7);
        log.Feature.ShouldBe(AiFeatures.AiChat);
        log.Model.ShouldBe("chat-model");
        log.InputTokens.ShouldBe(250);
        log.CachedInputTokens.ShouldBe(30);
        log.OutputTokens.ShouldBe(50);
        log.EstimatedCostUsd.ShouldBeGreaterThan(0m);
    }

    [Fact]
    public async Task Envia_o_prompt_de_sistema_primeiro_o_historico_depois_e_os_limites_de_saida()
    {
        var client = new ScriptedChatClient(Text("{\"reply\":\"ok\"}", 10, 5));

        await Build(client).RunAsync(
            1,
            "SYSTEM PROMPT",
            new[] { new AiChatTurn("user", "a"), new AiChatTurn("assistant", "b"), new AiChatTurn("user", "c") },
            _toolbox.Object);

        var messages = client.Calls[0];
        messages[0].Role.ShouldBe(ChatRole.System);
        messages[0].Text.ShouldBe("SYSTEM PROMPT");
        messages.Skip(1).Select(m => (m.Role, m.Text)).ShouldBe(new[]
        {
            (ChatRole.User, "a"), (ChatRole.Assistant, "b"), (ChatRole.User, "c")
        });
        client.LastOptions!.MaxOutputTokens.ShouldBe(500);
    }

    [Fact]
    public async Task Expoe_todas_as_ferramentas_marcadas_com_nomes_em_snake_case_e_parametros_opcionais()
    {
        var client = new ScriptedChatClient(Text("{\"reply\":\"ok\"}", 1, 1));

        await Build(client).RunAsync(1, "S", new[] { new AiChatTurn("user", "oi") }, _toolbox.Object);

        var names = client.LastOptions!.Tools!.OfType<AIFunction>().Select(t => t.Name).ToList();

        names.ShouldContain("get_spending_by_category");
        names.ShouldContain("search_transactions");
        names.ShouldContain("prepare_invoice_payment");
        names.ShouldContain("prepare_transactions");
        names.ShouldContain("open_screen");
        names.Count.ShouldBe(18);
        names.ShouldContain("prepare_category");
        names.ShouldContain("prepare_financial_goal");
        names.ShouldContain("prepare_category_budget_goal");
        names.ShouldContain("prepare_goal_contribution");
        names.ShouldContain("prepare_planned_commitment");

        var search = client.LastOptions.Tools!.OfType<AIFunction>().Single(t => t.Name == "search_transactions");
        var required = search.JsonSchema.TryGetProperty("required", out var req) ? req.GetArrayLength() : 0;
        required.ShouldBe(0);
    }

    [Fact]
    public async Task Raciocinio_e_desligado_quando_ha_ferramentas_se_o_provedor_exigir()
    {
        var client = new ScriptedChatClient(Text("{\"reply\":\"ok\"}", 1, 1));

        await Build(client).RunAsync(1, "S", new[] { new AiChatTurn("user", "oi") }, _toolbox.Object);

        client.LastOptions!.Reasoning!.Effort.ShouldBe(ReasoningEffort.None);
    }

    [Fact]
    public async Task Sem_configuracao_nao_envia_opcao_de_raciocinio()
    {
        _availability
            .Setup(a => a.GetActive())
            .Returns(new ActiveAiProvider("Groq", new AiProviderSettings { Model = "llama", BaseUrl = "https://x", ApiKey = "k" }));
        var client = new ScriptedChatClient(Text("{\"reply\":\"ok\"}", 1, 1));

        await Build(client).RunAsync(1, "S", new[] { new AiChatTurn("user", "oi") }, _toolbox.Object);

        client.LastOptions!.Reasoning.ShouldBeNull();
    }

    [Fact]
    public async Task Ferramentas_de_escrita_nao_existem_no_conjunto_exposto()
    {
        var client = new ScriptedChatClient(Text("{\"reply\":\"ok\"}", 1, 1));

        await Build(client).RunAsync(1, "S", new[] { new AiChatTurn("user", "oi") }, _toolbox.Object);

        var names = client.LastOptions!.Tools!.OfType<AIFunction>().Select(t => t.Name).ToList();

        names.ShouldNotContain(n => n.StartsWith("pay_") || n.StartsWith("delete_") || n.StartsWith("create_") || n.StartsWith("update_") || n.StartsWith("save_"));
    }

    [Fact]
    public async Task Ia_desligada_lanca_excecao()
    {
        _availability.Setup(a => a.GetActive()).Returns((ActiveAiProvider?)null);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            Build(new ScriptedChatClient()).RunAsync(1, "S", new[] { new AiChatTurn("user", "oi") }, _toolbox.Object));
    }

    [Fact]
    public async Task Erro_do_cliente_ainda_registra_o_consumo_ja_incorrido()
    {
        var client = new ScriptedChatClient(
            Call("get_monthly_totals", new Dictionary<string, object?> { ["months"] = 1 }, 100, 10),
            null);
        _toolbox.Setup(t => t.GetMonthlyTotals(It.IsAny<int>())).ReturnsAsync(ChatToolResult.Success());

        await Should.ThrowAsync<InvalidOperationException>(() =>
            Build(client).RunAsync(1, "S", new[] { new AiChatTurn("user", "oi") }, _toolbox.Object));

        _logs.Single().InputTokens.ShouldBe(100);
    }

    private sealed class FakeFactory : IAiChatClientFactory
    {
        private readonly IChatClient _client;

        public FakeFactory(IChatClient client) => _client = client;

        public IChatClient Get(ActiveAiProvider provider) => _client;
    }

    private sealed class ScriptedChatClient : IChatClient
    {
        private readonly Queue<ChatResponse?> _responses;

        public ScriptedChatClient(params ChatResponse?[] responses) => _responses = new Queue<ChatResponse?>(responses);

        public List<List<ChatMessage>> Calls { get; } = new();

        public ChatOptions? LastOptions { get; private set; }

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            Calls.Add(messages.ToList());
            LastOptions = options;

            var next = _responses.Count > 0 ? _responses.Dequeue() : null;

            return next is null
                ? throw new InvalidOperationException("no scripted response")
                : Task.FromResult(next);
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }
}
