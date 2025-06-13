using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.EmailCcConfiguration;
using System.Text.Json;

namespace Service
{
    public class EmailCcConfigurationService : IEmailCcConfigurationService
    {
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;

        public EmailCcConfigurationService(IRepositoryManager repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EmailCcConfigurationDto>> GetAllConfigurationsAsync()
        {
            var configurations = await _repository.EmailCcConfiguration.GetAllAsync(trackChanges: false);
            var configurationsDto = _mapper.Map<IEnumerable<EmailCcConfigurationDto>>(configurations);

            return configurationsDto.Select(config => config with
            {
                DefaultCcEmails = DeserializeEmails(configurations.First(c => c.Id == config.Id).DefaultCcEmails),
                DefaultBccEmails = DeserializeEmails(configurations.First(c => c.Id == config.Id).DefaultBccEmails)
            });
        }

        public async Task<EmailCcConfigurationDto> GetConfigurationByIdAsync(int id)
        {
            var configuration = await GetEmailCcConfigurationAndCheckIfItExists(id, trackChanges: false);
            var configurationDto = _mapper.Map<EmailCcConfigurationDto>(configuration);

            return configurationDto with
            {
                DefaultCcEmails = DeserializeEmails(configuration.DefaultCcEmails),
                DefaultBccEmails = DeserializeEmails(configuration.DefaultBccEmails)
            };
        }

        public async Task<EmailCcConfigurationDto> GetConfigurationByKeyAsync(string configKey)
        {
            var configuration = await _repository.EmailCcConfiguration.GetByKeyAsync(configKey, trackChanges: false);
            if (configuration == null)
                throw new EmailCcConfigurationNotFoundException(configKey);

            var configurationDto = _mapper.Map<EmailCcConfigurationDto>(configuration);
            return configurationDto with
            {
                DefaultCcEmails = DeserializeEmails(configuration.DefaultCcEmails),
                DefaultBccEmails = DeserializeEmails(configuration.DefaultBccEmails)
            };
        }

        public async Task<IEnumerable<EmailCcConfigurationDto>> GetEnabledConfigurationsAsync()
        {
            var configurations = await _repository.EmailCcConfiguration.GetEnabledConfigurationsAsync(trackChanges: false);
            var configurationsDto = _mapper.Map<IEnumerable<EmailCcConfigurationDto>>(configurations);

            return configurationsDto.Select(config => config with
            {
                DefaultCcEmails = DeserializeEmails(configurations.First(c => c.Id == config.Id).DefaultCcEmails),
                DefaultBccEmails = DeserializeEmails(configurations.First(c => c.Id == config.Id).DefaultBccEmails)
            });
        }

        public async Task<EmailCcConfigurationDto> CreateConfigurationAsync(EmailCcConfigurationForCreationDto configDto)
        {
            var configuration = _mapper.Map<EmailCcConfiguration>(configDto);
            configuration.DefaultCcEmails = SerializeEmails(configDto.DefaultCcEmails);
            configuration.DefaultBccEmails = SerializeEmails(configDto.DefaultBccEmails);
            configuration.CreatedAt = DateTime.UtcNow;

            _repository.EmailCcConfiguration.CreateEmailCcConfiguration(configuration);
            await _repository.SaveAsync();

            var configurationToReturn = _mapper.Map<EmailCcConfigurationDto>(configuration);
            return configurationToReturn with
            {
                DefaultCcEmails = configDto.DefaultCcEmails,
                DefaultBccEmails = configDto.DefaultBccEmails
            };
        }

        public async Task UpdateConfigurationAsync(int id, EmailCcConfigurationForUpdateDto configDto)
        {
            var configuration = await GetEmailCcConfigurationAndCheckIfItExists(id, trackChanges: true);

            _mapper.Map(configDto, configuration);
            configuration.DefaultCcEmails = SerializeEmails(configDto.DefaultCcEmails);
            configuration.DefaultBccEmails = SerializeEmails(configDto.DefaultBccEmails);
            configuration.UpdatedAt = DateTime.UtcNow;

            await _repository.SaveAsync();
        }

        public async Task DeleteConfigurationAsync(int id)
        {
            var configuration = await GetEmailCcConfigurationAndCheckIfItExists(id, trackChanges: false);
            _repository.EmailCcConfiguration.DeleteEmailCcConfiguration(configuration);
            await _repository.SaveAsync();
        }

        public async Task<bool> ToggleConfigurationAsync(int id)
        {
            var configuration = await GetEmailCcConfigurationAndCheckIfItExists(id, trackChanges: true);
            configuration.IsEnabled = !configuration.IsEnabled;
            configuration.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveAsync();
            return configuration.IsEnabled;
        }

        public async Task<List<string>> GetCcEmailsForConfigAsync(string configKey)
        {
            var configuration = await _repository.EmailCcConfiguration.GetByKeyAsync(configKey, trackChanges: false);
            if (configuration == null || !configuration.IsEnabled)
                return new List<string>();

            return DeserializeEmails(configuration.DefaultCcEmails);
        }

        public async Task<bool> IsConfigEnabledAsync(string configKey)
        {
            var configuration = await _repository.EmailCcConfiguration.GetByKeyAsync(configKey, trackChanges: false);
            return configuration?.IsEnabled ?? false;
        }

        // Helper methods
        private async Task<EmailCcConfiguration> GetEmailCcConfigurationAndCheckIfItExists(int id, bool trackChanges)
        {
            var configuration = await _repository.EmailCcConfiguration.GetByIdAsync(id, trackChanges);
            if (configuration == null)
                throw new EmailCcConfigurationNotFoundException(id);
            return configuration;
        }

        private string SerializeEmails(List<string> emails)
        {
            if (emails == null || !emails.Any())
                return null;
            return JsonSerializer.Serialize(emails);
        }

        private List<string> DeserializeEmails(string emailsJson)
        {
            if (string.IsNullOrEmpty(emailsJson))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(emailsJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}