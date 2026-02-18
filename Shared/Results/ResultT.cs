namespace Shared.Results;

public sealed class ResultEntity<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorCode { get; }
    public string? Message { get; set; }


    private ResultEntity(bool success, T? value, string? errorCode, string? message)
    {
        IsSuccess = success;
        Value = value;
        ErrorCode = errorCode;
        Message = message;
    }

    public static ResultEntity<T> Success(T value, string? message = null, bool success = true)
        => new(true, value, null, message);

    public static ResultEntity<T> Failure(string? message = null, string? code= "", bool success = false)
        => new(false, default, code, message);
}

