using Domain.Exceptions;
using Shared.Localization;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects.User;

public sealed class Cpf
{
    public string Value { get; private set; }

    private Cpf() { }

    public Cpf(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(MessageKeys.InvalidCpf);

        var cpf = Regex.Replace(value, @"\D", "");

        if (cpf.Length != 11 || IsRepeated(cpf) || !IsValid(cpf))
            throw new DomainException(MessageKeys.InvalidCpf);

        Value = cpf;
    }

    private static bool IsRepeated(string cpf)
        => new string(cpf[0], cpf.Length) == cpf;

    private static bool IsValid(string cpf)
    {
        int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] mult2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        var temp = cpf[..9];
        var sum = 0;

        for (int i = 0; i < 9; i++)
            sum += (temp[i] - '0') * mult1[i];

        var mod = sum % 11;
        var digit1 = mod < 2 ? 0 : 11 - mod;

        temp += digit1;
        sum = 0;

        for (int i = 0; i < 10; i++)
            sum += (temp[i] - '0') * mult2[i];

        mod = sum % 11;
        var digit2 = mod < 2 ? 0 : 11 - mod;

        return cpf.EndsWith($"{digit1}{digit2}");
    }

    public override string ToString() => Value;
}
