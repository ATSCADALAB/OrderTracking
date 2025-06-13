using Shared.DataTransferObjects.EmailCcConfiguration;

namespace Service.Contracts
{
    public interface IEmailCcConfigurationService
    {
        Task<IEnumerable<EmailCcConfigurationDto>> GetAllConfigurationsAsync();
        Task<EmailCcConfigurationDto> GetConfigurationByIdAsync(int id);
        Task<EmailCcConfigurationDto> GetConfigurationByKeyAsync(string configKey);
        Task<IEnumerable<EmailCcConfigurationDto>> GetEnabledConfigurationsAsync();
        Task<EmailCcConfigurationDto> CreateConfigurationAsync(EmailCcConfigurationForCreationDto configDto);
        Task UpdateConfigurationAsync(int id, EmailCcConfigurationForUpdateDto configDto);
        Task DeleteConfigurationAsync(int id);
        Task<bool> ToggleConfigurationAsync(int id);
        Task<List<string>> GetCcEmailsForConfigAsync(string configKey);
        Task<bool> IsConfigEnabledAsync(string configKey);
    }
}