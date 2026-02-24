using Application.DTOs.Configuration.PaymentMethods;

namespace Application.DTOs.Configuration;

public sealed record DeactivatePaymentMethodSelectRequest(
    IReadOnlyCollection<PaymentMethodIdRequest> Categories
);
