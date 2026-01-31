using Domain.Exceptions;
using Shared.Localization;

namespace Domain.ValueObjects;

public sealed class Email
{
    public string Value { get; private set; }

    private Email() { }

    private Email(string address)
    {
        Value = address;
    }

    public static Email Create(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException(MessageKeys.InvalidEmail);

        if (!address.Contains("@"))
            throw new DomainException(MessageKeys.InvalidEmail);

        return new Email(address.Trim().ToLower());
    }

    public override string ToString() => Value;
}
