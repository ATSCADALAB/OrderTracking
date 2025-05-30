using Entities.Models;

namespace Contracts
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetCategorysAsync(bool trackChanges);
        Task<Category> GetCategoryAsync(Guid CategoryId, bool trackChanges);
        void CreateCategory(Category Category);
        void DeleteCategory(Category Category);
    }
}
