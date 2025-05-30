using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects.SheetOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickStart.Presentation.Controllers
{
    [Route("api/sheet-orders")]
    [ApiController]
    public class SheetOrderController : ControllerBase
    {
        private readonly IServiceManager _service;

        public SheetOrderController(IServiceManager service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.SheetOrderService.GetAllAsync(trackChanges: false);
            return Ok(result);
        }

        [HttpGet("{orderCode}/{sheetName}")]
        public async Task<IActionResult> GetByCode(string orderCode, string sheetName)
        {
            var result = await _service.SheetOrderService.GetByOrderCodeAsync(orderCode, sheetName, trackChanges: false);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] SheetOrderDto dto)
        {
            await _service.SheetOrderService.CreateIfNotExistsAsync(dto);
            return Ok();
        }
    }
}
