namespace Application.DTOs.Configuration.PaymentMethods;

public sealed record UpdatePaymentMethodRequest(
    int Id,
    string Description
);
