using Domain.Exceptions;
using Shared.Localization;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects.User;

public sealed class Phone
{
    public string Value { get; private set; }

    private Phone() { }

    public Phone(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(MessageKeys.InvalidPhone);

        var digits = Regex.Replace(value, @"\D", "");

        if (digits.Length < 10 || digits.Length > 11)
            throw new DomainException(MessageKeys.InvalidPhone);

        Value = digits;
    }

    public override string ToString() => Value;
}
