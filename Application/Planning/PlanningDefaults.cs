namespace Application.Planning;

public static class PlanningDefaults
{
    public const int DefaultHorizonMonths = 12;
    public const int MaxHorizonMonths = 36;

    public static int NormalizeHorizon(int? horizonMonths)
        => Math.Clamp(horizonMonths ?? DefaultHorizonMonths, 1, MaxHorizonMonths);
}
