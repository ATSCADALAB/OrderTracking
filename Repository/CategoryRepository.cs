using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    internal sealed class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
    {
        public CategoryRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public void CreateCategory(Category Category)
        {
            Create(Category);
        }

        public void DeleteCategory(Category Category)
        {
            Delete(Category);
        }

        public async Task<Category> GetCategoryAsync(Guid CategoryId, bool trackChanges)
        {
            return await FindByCondition(c => c.Id.Equals(CategoryId), trackChanges)
 
                .SingleOrDefaultAsync();

        }

        public async Task<IEnumerable<Category>> GetCategorysAsync(bool trackChanges)
        {
            return await FindAll(trackChanges)
                .ToListAsync();
        }
    }
}
