namespace Application.DTOs.Planning;

public sealed record MonthViabilityResponse(
    int Year,
    int Month,
    int Score,
    string Verdict);
