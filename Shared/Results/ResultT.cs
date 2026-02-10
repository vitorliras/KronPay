namespace Shared.Results;

public sealed class ResultT<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T? Value { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    private ResultT(bool success, T? value, string? errorCode, string? errorMessage)
    {
        IsSuccess = success;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static ResultT<T> Success(T value, string? message = null, bool success = true)
        => new(true, value, null, message);

    public static ResultT<T> Failure(string code, string? message = null, bool success = false)
        => new(false, default, code, message);
}

