namespace Api.Contracts.Errors;

public sealed class ApiErrorResponse
{
    public string Code { get; init; } = default!;
    public string Message { get; init; } = default!;
}
