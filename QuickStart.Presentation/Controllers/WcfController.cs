// QuickStart.Presentation/Controllers/WcfController.c
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Service.Contracts;

namespace QuickStart.Presentation.Controllers
{
    [Route("api/wcf")]
    [ApiController]
    public class WcfController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public WcfController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpPost("read")]
        public async Task<IActionResult> Read()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();
                var tagNames = JsonSerializer.Deserialize<string[]>(json);

                if (tagNames == null || !tagNames.Any())
                {
                    throw new Exception("No tags provided");
                }

                var result = await _serviceManager.WcfService.ReadTagsAsync(tagNames);
                return Ok(new { Status = true, Result = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Read error: {ex.Message}");
                return Ok(new { Status = false });
            }
        }

        [HttpPost("start-polling")]
        public async Task<IActionResult> StartPolling([FromQuery] int intervalMs = 1000)
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var json = await reader.ReadToEndAsync();
                var tagNames = JsonSerializer.Deserialize<string[]>(json);

                if (tagNames == null || !tagNames.Any())
                {
                    throw new Exception("No tags provided");
                }

                await _serviceManager.WcfService.StartPollingAsync(tagNames, intervalMs);
                return Ok(new { Status = true, Message = "Polling started" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Start polling error: {ex.Message}");
                return Ok(new { Status = false });
            }
        }
    }
}