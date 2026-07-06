namespace Application.DTOs.Goals;

public sealed record SimulateExtraContributionResponse(
    DateTime? OriginalProjectedCompletionDate,
    DateTime? AcceleratedProjectedCompletionDate,
    int DaysAccelerated);
