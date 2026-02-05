namespace Application.DTOs.Configuration.PaymentMethods;

public sealed record UpdatePaymentMethodRequest(
    int Id,
    int UserId,
    string Description
);
