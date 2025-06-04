using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    internal sealed class EventExclusionKeywordRepository : RepositoryBase<EventExclusionKeyword>, IEventExclusionKeywordRepository
    {
        public EventExclusionKeywordRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public async Task<IEnumerable<EventExclusionKeyword>> GetAllKeywordsAsync(bool trackChanges)
        {
            return await FindAll(trackChanges)
                .OrderBy(k => k.Keyword)
                .ToListAsync();
        }

        public async Task<IEnumerable<EventExclusionKeyword>> GetActiveKeywordsAsync(bool trackChanges)
        {
            return await FindByCondition(k => k.IsActive, trackChanges)
                .OrderBy(k => k.Keyword)
                .ToListAsync();
        }

        public async Task<EventExclusionKeyword?> GetKeywordByIdAsync(int id, bool trackChanges)
        {
            return await FindByCondition(k => k.Id == id, trackChanges)
                .SingleOrDefaultAsync();
        }

        public void CreateKeyword(EventExclusionKeyword keyword) => Create(keyword);
        public void UpdateKeyword(EventExclusionKeyword keyword) => Update(keyword);
        public void DeleteKeyword(EventExclusionKeyword keyword) => Delete(keyword);

        public async Task<bool> ExistsAsync(string keyword)
        {
            return await FindByCondition(k => k.Keyword.ToLower() == keyword.ToLower() && k.IsActive, false)
                .AnyAsync();
        }
    }
}