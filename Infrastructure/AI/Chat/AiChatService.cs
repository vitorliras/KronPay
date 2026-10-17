using System.ClientModel;
using System.Reflection;
using System.Text;
using Application.Abstractions.AI;
using Application.Abstractions.AiChat;
using Application.Configuration.AI;
using Application.Services.AI;
using Domain.Entities.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Shared.AI;

namespace Infrastructure.AI.Chat;

public sealed class AiChatService : IAiChatService
{
    private static readonly int[] RejectedStatuses = { 401, 402, 403, 429 };

    private readonly IAiAvailability _availability;
    private readonly IOptionsMonitor<AiSettings> _settings;
    private readonly IAiChatClientFactory _clientFactory;
    private readonly IAiUsageRecorder _usageRecorder;

    public AiChatService(
        IAiAvailability availability,
        IOptionsMonitor<AiSettings> settings,
        IAiChatClientFactory clientFactory,
        IAiUsageRecorder usageRecorder)
    {
        _availability = availability;
        _settings = settings;
        _clientFactory = clientFactory;
        _usageRecorder = usageRecorder;
    }

    public async Task<AiChatServiceResult> RunAsync(
        int userId,
        string systemPrompt,
        IReadOnlyList<AiChatTurn> history,
        IChatToolbox toolbox,
        CancellationToken cancellationToken = default)
    {
        var provider = _availability.GetActive()
            ?? throw new InvalidOperationException("AI is not enabled.");

        var chat = _settings.CurrentValue.Chat;
        var settings = WithChatOverrides(provider.Settings, chat);

        var tracker = new UsageTrackingChatClient(_clientFactory.Get(new ActiveAiProvider(provider.Name, settings)));

        var client = new ChatClientBuilder(tracker)
            .UseFunctionInvocation(configure: invoker => invoker.MaximumIterationsPerRequest = chat.MaxToolIterations)
            .Build();

        var options = new ChatOptions
        {
            Tools = CreateTools(toolbox),
            MaxOutputTokens = chat.MaxOutputTokens,
            Reasoning = ToReasoning(settings.ToolReasoningEffort),
            ResponseFormat = settings.StructuredChatOutput
                ? ChatResponseFormat.ForJsonSchema(AiChatReplySchema.Schema, AiChatReplySchema.Name)
                : null
        };

        var messages = new List<ChatMessage> { new(ChatRole.System, systemPrompt) };
        messages.AddRange(history.Select(ToChatMessage));

        try
        {
            var response = await client.GetResponseAsync(messages, options, cancellationToken);

            var toolCalls = response.Messages
                .SelectMany(m => m.Contents)
                .OfType<FunctionCallContent>()
                .Count();

            return new AiChatServiceResult(response.Text, toolCalls);
        }
        catch (ClientResultException ex) when (RejectedStatuses.Contains(ex.Status))
        {
            throw new AiProviderRejectedException(ex.Status);
        }
        finally
        {
            await RecordUsageAsync(userId, provider.Name, settings, tracker);
        }
    }

    private async Task RecordUsageAsync(
        int userId,
        string providerName,
        AiProviderSettings settings,
        UsageTrackingChatClient tracker)
    {
        if (tracker.InputTokens == 0 && tracker.OutputTokens == 0)
            return;

        var input = tracker.InputTokens;
        var cached = Math.Min(tracker.CachedInputTokens, input);

        await _usageRecorder.RecordAsync(new AiUsageLog(
            userId,
            AiFeatures.AiChat,
            providerName,
            settings.Model,
            input,
            cached,
            tracker.OutputTokens,
            AiCostCalculator.Calculate(settings, input, cached, tracker.OutputTokens)));
    }

    private static IList<AITool> CreateTools(IChatToolbox toolbox) =>
        typeof(IChatToolbox)
            .GetMethods()
            .Where(m => m.GetCustomAttribute<ChatToolAttribute>() is not null)
            .Select(m => (AITool)AIFunctionFactory.Create(
                m,
                toolbox,
                new AIFunctionFactoryOptions { Name = ToSnakeCase(m.Name) }))
            .ToList();

    private static ReasoningOptions? ToReasoning(string? effort) =>
        effort?.Trim().ToLowerInvariant() switch
        {
            "none" => new ReasoningOptions { Effort = ReasoningEffort.None },
            "low" => new ReasoningOptions { Effort = ReasoningEffort.Low },
            "medium" => new ReasoningOptions { Effort = ReasoningEffort.Medium },
            "high" => new ReasoningOptions { Effort = ReasoningEffort.High },
            "extrahigh" => new ReasoningOptions { Effort = ReasoningEffort.ExtraHigh },
            _ => null
        };

    private static ChatMessage ToChatMessage(AiChatTurn turn) =>
        new(turn.Role == "assistant" ? ChatRole.Assistant : ChatRole.User, turn.Content);

    private static AiProviderSettings WithChatOverrides(AiProviderSettings source, AiChatSettings chat) =>
        new()
        {
            BaseUrl = source.BaseUrl,
            ApiKey = source.ApiKey,
            TimeoutSeconds = source.TimeoutSeconds,
            SupportsFiles = source.SupportsFiles,
            ToolReasoningEffort = source.ToolReasoningEffort,
            StructuredChatOutput = source.StructuredChatOutput,
            Model = string.IsNullOrWhiteSpace(chat.Model) ? source.Model : chat.Model,
            InputPricePerMillion = chat.InputPricePerMillion ?? source.InputPricePerMillion,
            CachedInputPricePerMillion = chat.CachedInputPricePerMillion ?? source.CachedInputPricePerMillion,
            OutputPricePerMillion = chat.OutputPricePerMillion ?? source.OutputPricePerMillion
        };

    private static string ToSnakeCase(string name)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && i > 0)
                sb.Append('_');

            sb.Append(char.ToLowerInvariant(name[i]));
        }

        return sb.ToString();
    }
}
