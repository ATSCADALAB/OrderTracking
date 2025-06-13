using Entities.Models;

namespace Contracts
{
    public interface IEmailCcConfigurationRepository
    {
        Task<IEnumerable<EmailCcConfiguration>> GetAllAsync(bool trackChanges);
        Task<EmailCcConfiguration> GetByIdAsync(int id, bool trackChanges);
        Task<EmailCcConfiguration> GetByKeyAsync(string configKey, bool trackChanges);
        Task<IEnumerable<EmailCcConfiguration>> GetEnabledConfigurationsAsync(bool trackChanges);
        void CreateEmailCcConfiguration(EmailCcConfiguration configuration);
        void UpdateEmailCcConfiguration(EmailCcConfiguration configuration);
        void DeleteEmailCcConfiguration(EmailCcConfiguration configuration);
    }
}