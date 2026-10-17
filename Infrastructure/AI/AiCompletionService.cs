using Application.Abstractions.AI;
using Application.Services.AI;
using Domain.Entities.AI;
using Microsoft.Extensions.AI;

namespace Infrastructure.AI;

public sealed class AiCompletionService : IAiCompletionService
{
    private readonly IAiAvailability _availability;
    private readonly IAiChatClientFactory _clientFactory;
    private readonly IAiUsageRecorder _usageRecorder;

    public AiCompletionService(
        IAiAvailability availability,
        IAiChatClientFactory clientFactory,
        IAiUsageRecorder usageRecorder)
    {
        _availability = availability;
        _clientFactory = clientFactory;
        _usageRecorder = usageRecorder;
    }

    public async Task<AiCompletionResult> CompleteAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var provider = _availability.GetActive()
            ?? throw new InvalidOperationException("AI is not enabled.");

        var hasAttachments = request.Attachments is { Count: > 0 };

        if (hasAttachments && !provider.Settings.SupportsFiles)
            throw new InvalidOperationException("The active AI provider does not support files.");

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, request.SystemPrompt),
            BuildUserMessage(request)
        };

        var response = await _clientFactory
            .Get(provider)
            .GetResponseAsync(messages, cancellationToken: cancellationToken);

        var inputTokens = (int)(response.Usage?.InputTokenCount ?? 0);
        var cachedInputTokens = Math.Min((int)(response.Usage?.CachedInputTokenCount ?? 0), inputTokens);
        var outputTokens = (int)(response.Usage?.OutputTokenCount ?? 0);

        var cost = AiCostCalculator.Calculate(
            provider.Settings,
            inputTokens,
            cachedInputTokens,
            outputTokens);

        await _usageRecorder.RecordAsync(new AiUsageLog(
            request.UserId,
            request.Feature,
            provider.Name,
            provider.Settings.Model,
            inputTokens,
            cachedInputTokens,
            outputTokens,
            cost));

        return new AiCompletionResult(
            response.Text,
            inputTokens,
            cachedInputTokens,
            outputTokens,
            cost);
    }

    private static ChatMessage BuildUserMessage(AiCompletionRequest request)
    {
        var contents = new List<AIContent> { new TextContent(request.UserPrompt) };

        foreach (var attachment in request.Attachments ?? Array.Empty<AiAttachment>())
        {
            contents.Add(new DataContent(attachment.Content, attachment.MediaType)
            {
                Name = attachment.FileName
            });
        }

        return new ChatMessage(ChatRole.User, contents);
    }
}
