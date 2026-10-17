using Application.Abstractions;
using Application.Abstractions.AI;
using Application.Abstractions.AiChat;
using Application.Abstractions.Common;
using Application.Configuration.AI;
using Application.DTOs.AiChat;
using Application.Services.AiChat;
using Application.Services.Spending;
using Domain.Interfaces;
using Domain.Interfaces.AI;
using Domain.Services.Gamification;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.AI;
using Shared.Localization;
using Shared.Results;

namespace Application.UseCases.AiChat;

public sealed class SendAiChatMessageUseCase
    : IUseCase<AiChatRequest, AiChatResponse>
{
    private readonly IAiAvailability _availability;
    private readonly IOptionsMonitor<AiSettings> _settings;
    private readonly IAiUsageLogRepository _usageRepository;
    private readonly FinancialProfileBuilder _profileBuilder;
    private readonly IAiChatService _chatService;
    private readonly IChatToolbox _toolbox;
    private readonly ChatActionCollector _actions;
    private readonly IGamificationService _gamificationService;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SendAiChatMessageUseCase> _logger;

    public SendAiChatMessageUseCase(
        IAiAvailability availability,
        IOptionsMonitor<AiSettings> settings,
        IAiUsageLogRepository usageRepository,
        FinancialProfileBuilder profileBuilder,
        IAiChatService chatService,
        IChatToolbox toolbox,
        ChatActionCollector actions,
        IGamificationService gamificationService,
        IUnitOfWork uow,
        ICurrentUserService currentUser,
        ILogger<SendAiChatMessageUseCase> logger)
    {
        _availability = availability;
        _settings = settings;
        _usageRepository = usageRepository;
        _profileBuilder = profileBuilder;
        _chatService = chatService;
        _toolbox = toolbox;
        _actions = actions;
        _gamificationService = gamificationService;
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ResultEntity<AiChatResponse>> ExecuteAsync(AiChatRequest request)
    {
        if (_availability.GetActive() is null)
            return ResultEntity<AiChatResponse>.Failure(MessageKeys.AiUnavailable);

        var userId = _currentUser.UserId;
        var chat = _settings.CurrentValue.Chat;

        var sentToday = await _usageRepository.CountByFeatureSinceAsync(userId, AiFeatures.AiChat, DateTime.UtcNow.Date);
        if (sentToday >= chat.MaxMessagesPerDay)
            return ResultEntity<AiChatResponse>.Failure(MessageKeys.AiChatDailyLimit);

        var history = request.Messages
            .TakeLast(Math.Max(1, chat.MaxHistoryMessages))
            .Select(m => new AiChatTurn(m.Role.Trim().ToLowerInvariant(), m.Content.Trim()))
            .ToList();

        var profile = await _profileBuilder.BuildAsync(userId, BrazilClock.Today());

        AiChatServiceResult result;

        try
        {
            result = await _chatService.RunAsync(userId, AiChatSystemPrompt.Build(profile), history, _toolbox);
        }
        catch (AiProviderRejectedException ex)
        {
            _logger.LogWarning("AI chat rejected by the provider with status {Status}.", ex.StatusCode);
            return ResultEntity<AiChatResponse>.Failure(MessageKeys.AiChatProviderRejected);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI chat failed.");
            return ResultEntity<AiChatResponse>.Failure(MessageKeys.AiChatFailed);
        }

        var reply = AiChatReplyParser.Parse(result.Text);

        if (reply.Reply.Length == 0)
            return ResultEntity<AiChatResponse>.Failure(MessageKeys.AiChatFailed);

        await TriggerGamificationBestEffort(userId);

        return ResultEntity<AiChatResponse>.Success(
            new AiChatResponse(reply.Reply, reply.Suggestions, _actions.Actions.ToList()),
            MessageKeys.OperationSuccess);
    }

    private async Task TriggerGamificationBestEffort(int userId)
    {
        try
        {
            await _gamificationService.NotifyAssistantConversationAsync(userId);

            if (!await _uow.CommitAsync())
                _logger.LogError("Falha ao persistir a avaliação de gamificação do chat para o usuário {UserId}.", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha inesperada ao avaliar gamificação do chat para o usuário {UserId}.", userId);
        }
    }
}
