namespace Application.DTOs.Configuration.PaymentMethods;

public sealed record PaymentMethodResponse(
    int Id,
    string Description,
    bool Active
);