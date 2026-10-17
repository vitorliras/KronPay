namespace Application.Services.Spending;

public sealed record CategoryTotal(
    int? CategoryId,
    string Category,
    decimal Total,
    int Count,
    decimal CardPortion);
