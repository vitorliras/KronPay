using Domain.Exceptions;
using Shared.Localization;

public sealed class Email
{
    public string Value { get; }

    protected Email() { } 

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(MessageKeys.InvalidEmail);

        if (!value.Contains("@"))
            throw new DomainException(MessageKeys.InvalidEmail);

        Value = value.Trim().ToLower();
    }

    public override bool Equals(object? obj)
        => obj is Email other && Value == other.Value;

    public override int GetHashCode()
        => Value.GetHashCode();
}
