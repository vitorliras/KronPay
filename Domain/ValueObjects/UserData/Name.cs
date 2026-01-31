using Domain.Exceptions;
using Shared.Localization;

namespace Domain.ValueObjects;

public sealed class Name
{
    public string Value { get; private set; }

    private Name() { }

    private Name(string value)
    {
        Value = value;
    }

    public static Name Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(MessageKeys.InvaldUser);

        if (value.Length < 2)
            throw new DomainException(MessageKeys.InvaldUser);

        return new Name(value.Trim());
    }

    public override string ToString() => Value;
}
