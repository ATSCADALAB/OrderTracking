using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    public class EmailCcConfigurationRepository : RepositoryBase<EmailCcConfiguration>, IEmailCcConfigurationRepository
    {
        public EmailCcConfigurationRepository(RepositoryContext repositoryContext)
            : base(repositoryContext)
        {
        }

        public async Task<IEnumerable<EmailCcConfiguration>> GetAllAsync(bool trackChanges) =>
            await FindAll(trackChanges)
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.ConfigName)
                .ToListAsync();

        public async Task<EmailCcConfiguration> GetByIdAsync(int id, bool trackChanges) =>
            await FindByCondition(x => x.Id.Equals(id), trackChanges)
                .SingleOrDefaultAsync();

        public async Task<EmailCcConfiguration> GetByKeyAsync(string configKey, bool trackChanges) =>
            await FindByCondition(x => x.ConfigKey.Equals(configKey), trackChanges)
                .SingleOrDefaultAsync();

        public async Task<IEnumerable<EmailCcConfiguration>> GetEnabledConfigurationsAsync(bool trackChanges) =>
            await FindByCondition(x => x.IsEnabled, trackChanges)
                .OrderBy(x => x.Priority)
                .ToListAsync();

        public void CreateEmailCcConfiguration(EmailCcConfiguration configuration) =>
            Create(configuration);

        public void UpdateEmailCcConfiguration(EmailCcConfiguration configuration) =>
            Update(configuration);

        public void DeleteEmailCcConfiguration(EmailCcConfiguration configuration) =>
            Delete(configuration);
    }
}