using Entities.Models;

namespace Contracts
{
    public interface IKpiConfigurationRepository
    {
        Task<KpiConfiguration?> GetActiveConfigurationAsync(bool trackChanges);
        Task<KpiConfiguration?> GetConfigurationByIdAsync(int id, bool trackChanges);
        Task<IEnumerable<KpiConfiguration>> GetAllConfigurationsAsync(bool trackChanges);
        void CreateConfiguration(KpiConfiguration configuration);
        void UpdateConfiguration(KpiConfiguration configuration);
        void DeleteConfiguration(KpiConfiguration configuration);
    }
}