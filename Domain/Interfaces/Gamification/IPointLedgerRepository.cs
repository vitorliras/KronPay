using Domain.Entities.Gamification;

namespace Domain.Interfaces.Gamification;

public interface IPointLedgerRepository
{
    Task<bool> AddAsync(PointLedgerEntry entry);
}
