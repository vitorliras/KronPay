namespace Application.DTOs.Planning;

public sealed record SimulatePurchaseRequest(
    decimal Amount,
    DateTime PurchaseDate,
    bool Installment,
    short InstallmentsCount,
    int? CreditCardId,
    int? HorizonMonths,
    decimal? SafetyReserve);
