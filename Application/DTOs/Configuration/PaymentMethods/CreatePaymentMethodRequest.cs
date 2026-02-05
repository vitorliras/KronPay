namespace Application.DTOs.Configuration.PaymentMethods;

public sealed record CreatePaymentMethodRequest(
    int UserId,
    string Description,
    string CodTypePaymentMethod
);
