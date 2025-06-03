using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStart.Presentation.ActionFilters;
using Service.Contracts;
using Shared.DataTransferObjects.KpiConfiguration;

namespace QuickStart.Presentation.Controllers
{
    [Route("api/kpi-configuration")]
    [ApiController]
    public class KpiConfigurationController : ControllerBase
    {
        private readonly IServiceManager _service;

        public KpiConfigurationController(IServiceManager service)
        {
            _service = service;
        }

        [HttpGet("active")]
        [AllowAnonymous] // Cho phép service khác gọi
        public async Task<IActionResult> GetActiveConfiguration()
        {
            var config = await _service.KpiConfigurationService.GetActiveConfigurationAsync();
            if (config == null)
                return NotFound(new { Message = "No active KPI configuration found" });
            return Ok(config);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllConfigurations()
        {
            var configs = await _service.KpiConfigurationService.GetAllConfigurationsAsync();
            return Ok(configs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConfiguration(int id)
        {
            var config = await _service.KpiConfigurationService.GetConfigurationByIdAsync(id);
            if (config == null)
                return NotFound();
            return Ok(config);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateConfiguration([FromBody] KpiConfigurationForCreationDto configDto)
        {
            var createdConfig = await _service.KpiConfigurationService.CreateConfigurationAsync(configDto);
            return CreatedAtAction(nameof(GetConfiguration), new { id = createdConfig.Id }, createdConfig);
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateConfiguration(int id, [FromBody] KpiConfigurationForUpdateDto configDto)
        {
            await _service.KpiConfigurationService.UpdateConfigurationAsync(id, configDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfiguration(int id)
        {
            await _service.KpiConfigurationService.DeleteConfigurationAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> SetActiveConfiguration(int id)
        {
            var config = await _service.KpiConfigurationService.SetActiveConfigurationAsync(id);
            return Ok(config);
        }

        [HttpPost("create-default")]
        public async Task<IActionResult> CreateDefaultConfiguration()
        {
            var config = await _service.KpiConfigurationService.CreateDefaultConfigurationAsync();
            return Ok(config);
        }

        [HttpPost("calculate-stars")]
        [AllowAnonymous]
        public async Task<IActionResult> CalculateStars([FromQuery] int daysLate)
        {
            var stars = await _service.KpiConfigurationService.CalculateStarsAsync(daysLate);
            return Ok(new { DaysLate = daysLate, Stars = stars });
        }

        [HttpPost("calculate-hssl")]
        [AllowAnonymous]
        public async Task<IActionResult> CalculateHSSL([FromQuery] int lightOrders, [FromQuery] int mediumOrders, [FromQuery] int heavyOrders)
        {
            var hssl = await _service.KpiConfigurationService.CalculateHSSLAsync(lightOrders, mediumOrders, heavyOrders);
            return Ok(new { LightOrders = lightOrders, MediumOrders = mediumOrders, HeavyOrders = heavyOrders, HSSL = hssl });
        }

        [HttpPost("calculate-reward")]
        [AllowAnonymous]
        public async Task<IActionResult> CalculateReward([FromQuery] decimal averageStars, [FromQuery] decimal hssl, [FromQuery] decimal penalty)
        {
            var reward = await _service.KpiConfigurationService.CalculateRewardAsync(averageStars, hssl, penalty);
            return Ok(new { AverageStars = averageStars, HSSL = hssl, Penalty = penalty, Reward = reward });
        }
    }
}