namespace Application.Abstractions.AiChat;

public sealed class AiProviderRejectedException : Exception
{
    public AiProviderRejectedException(int statusCode)
        : base($"The AI provider rejected the request with status {statusCode}.")
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}
