using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStart.Presentation.ActionFilters;
using Service.Contracts;
using Shared.DataTransferObjects.Category;
using System.Security.Claims;

namespace QuickStart.Presentation.Controllers
{
    [Route("api/categories")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly IServiceManager _service;
        public CategoryController(IServiceManager service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _service.CategoryService.GetAllCategorysAsync(trackChanges: false);
            return Ok(categories);
        }

        [HttpGet("{id:guid}", Name = "CategoryById")]
        public async Task<IActionResult> GetCategory(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!await _service.AuthorizationService.HasPermission(userId, "Categorys", "Edit"))
            {
                return Forbid(); // 403 Forbidden
            }
            var Category = await _service.CategoryService.GetCategoryAsync(id, trackChanges: false);
            return Ok(Category);
        }


        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryForCreationDto Category)
        {
            var createdCategory = await _service.CategoryService.CreateCategoryAsync(Category);

            return CreatedAtRoute("CategoryById", new { id = createdCategory.Id }, createdCategory);
        }


        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            await _service.CategoryService.DeleteCategoryAsync(id, trackChanges: false);

            return NoContent();
        }

        [HttpPut("{id:guid}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryForUpdateDto Category)
        {
            await _service.CategoryService.UpdateCategoryAsync(id, Category, trackChanges: true);

            return NoContent();
        }
    }
}
