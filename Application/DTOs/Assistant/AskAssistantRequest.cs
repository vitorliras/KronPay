namespace Application.DTOs.Assistant;

public sealed record AskAssistantRequest(string? CurrentNodeId, string? SelectedOptionId, string? FreeText);
