using CodedByKay.PowerPatrol.Models;

namespace CodedByKay.PowerPatrol.Interfaces
{
    public interface ITibberService
    {
        Task<CurrentEnergyPrice?> GetEnergyConsumption();
        Task<(CurrentSubscription?, string?)> GetCurrentConsumtion();

    }
}
