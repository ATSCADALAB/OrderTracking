using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    internal sealed class KpiConfigurationRepository : RepositoryBase<KpiConfiguration>, IKpiConfigurationRepository
    {
        public KpiConfigurationRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public async Task<KpiConfiguration?> GetActiveConfigurationAsync(bool trackChanges)
        {
            return await FindByCondition(c => c.IsActive, trackChanges)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<KpiConfiguration?> GetConfigurationByIdAsync(int id, bool trackChanges)
        {
            return await FindByCondition(c => c.Id == id, trackChanges)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<KpiConfiguration>> GetAllConfigurationsAsync(bool trackChanges)
        {
            return await FindAll(trackChanges)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public void CreateConfiguration(KpiConfiguration configuration) => Create(configuration);
        public void UpdateConfiguration(KpiConfiguration configuration) => Update(configuration);
        public void DeleteConfiguration(KpiConfiguration configuration) => Delete(configuration);
    }
}