using Microsoft.AspNetCore.Mvc;
using QuickStart.Presentation.ActionFilters;
using Service.Contracts;
using Shared.DataTransferObjects.EmailCcConfiguration;

namespace QuickStart.Presentation.Controllers
{
    [Route("api/email-cc-configuration")]
    [ApiController]
    public class EmailCcConfigurationController : ControllerBase
    {
        private readonly IServiceManager _service;

        public EmailCcConfigurationController(IServiceManager service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllConfigurations()
        {
            var configurations = await _service.EmailCcConfigurationService.GetAllConfigurationsAsync();
            return Ok(configurations);
        }

        [HttpGet("enabled")]
        public async Task<IActionResult> GetEnabledConfigurations()
        {
            var configurations = await _service.EmailCcConfigurationService.GetEnabledConfigurationsAsync();
            return Ok(configurations);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetConfiguration(int id)
        {
            var configuration = await _service.EmailCcConfigurationService.GetConfigurationByIdAsync(id);
            return Ok(configuration);
        }

        [HttpGet("by-key/{configKey}")]
        public async Task<IActionResult> GetConfigurationByKey(string configKey)
        {
            var configuration = await _service.EmailCcConfigurationService.GetConfigurationByKeyAsync(configKey);
            return Ok(configuration);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateConfiguration([FromBody] EmailCcConfigurationForCreationDto configDto)
        {
            var configuration = await _service.EmailCcConfigurationService.CreateConfigurationAsync(configDto);
            return CreatedAtAction(nameof(GetConfiguration), new { id = configuration.Id }, configuration);
        }

        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateConfiguration(int id, [FromBody] EmailCcConfigurationForUpdateDto configDto)
        {
            await _service.EmailCcConfigurationService.UpdateConfigurationAsync(id, configDto);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteConfiguration(int id)
        {
            await _service.EmailCcConfigurationService.DeleteConfigurationAsync(id);
            return NoContent();
        }

        [HttpPost("{id:int}/toggle")]
        public async Task<IActionResult> ToggleConfiguration(int id)
        {
            var isEnabled = await _service.EmailCcConfigurationService.ToggleConfigurationAsync(id);
            return Ok(new { Id = id, IsEnabled = isEnabled });
        }

        [HttpGet("cc-emails/{configKey}")]
        public async Task<IActionResult> GetCcEmailsForConfig(string configKey)
        {
            var emails = await _service.EmailCcConfigurationService.GetCcEmailsForConfigAsync(configKey);
            return Ok(emails);
        }

        [HttpGet("is-enabled/{configKey}")]
        public async Task<IActionResult> IsConfigEnabled(string configKey)
        {
            var isEnabled = await _service.EmailCcConfigurationService.IsConfigEnabledAsync(configKey);
            return Ok(new { ConfigKey = configKey, IsEnabled = isEnabled });
        }
    }
}