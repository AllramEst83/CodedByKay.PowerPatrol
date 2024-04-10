using CodedByKay.PowerPatrol.Models;

namespace CodedByKay.PowerPatrol.Services
{
    public interface ITibberService
    {
        Task<CurrentEnergyPrice> GetEnergyConsumption();
    }
}