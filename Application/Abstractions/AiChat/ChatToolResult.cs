namespace Application.Abstractions.AiChat;

public sealed record ChatToolResult(bool Ok, string Message, object? Data = null)
{
    public static ChatToolResult Success(object? data = null, string message = "") => new(true, message, data);

    public static ChatToolResult Failure(string message) => new(false, message);
}
