using Shared.DataTransferObjects.Category;

namespace Service.Contracts
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategorysAsync(bool trackChanges);
        Task<CategoryDto> GetCategoryAsync(Guid CategoryId, bool trackChanges);
        Task<CategoryDto> CreateCategoryAsync(CategoryForCreationDto Category);
        Task DeleteCategoryAsync(Guid CategoryId, bool trackChanges);
        Task UpdateCategoryAsync(Guid CategoryId, CategoryForUpdateDto CategoryForUpdate, bool trackChanges);
    }
}
