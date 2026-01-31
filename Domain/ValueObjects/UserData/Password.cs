using Domain.Exceptions;
using Shared.Localization;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects.User;

public sealed class Password
{
    public string Value { get; }

    public Password(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(MessageKeys.InvalidEmail);

        if (value.Length < 8)
            throw new DomainException(MessageKeys.LengthPassword8);

        if (!Regex.IsMatch(value, @"[A-Z]"))
            throw new DomainException(MessageKeys.UppercaseLetterPassword);

        if (!Regex.IsMatch(value, @"[a-z]"))
            throw new DomainException(MessageKeys.LowercaseLetterPassword);

        if (!Regex.IsMatch(value, @"[\W_]"))
            throw new DomainException(MessageKeys.SpecialCharacter);

        if (HasSequentialNumbers(value))
            throw new DomainException(MessageKeys.NotSequencialNumberPassword);

        Value = value;
    }

    private static bool HasSequentialNumbers(string value)
    {
        var digits = value.Where(char.IsDigit).Select(c => c - '0').ToList();

        for (int i = 0; i < digits.Count - 2; i++)
        {
            if (digits[i] + 1 == digits[i + 1] &&
                digits[i + 1] + 1 == digits[i + 2])
            {
                return true;
            }
        }

        return false;
    }
}
