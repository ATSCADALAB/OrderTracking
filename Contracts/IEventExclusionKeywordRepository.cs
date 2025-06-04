using Entities.Models;

namespace Contracts
{
    public interface IEventExclusionKeywordRepository
    {
        Task<IEnumerable<EventExclusionKeyword>> GetAllKeywordsAsync(bool trackChanges);
        Task<IEnumerable<EventExclusionKeyword>> GetActiveKeywordsAsync(bool trackChanges);
        Task<EventExclusionKeyword?> GetKeywordByIdAsync(int id, bool trackChanges);
        void CreateKeyword(EventExclusionKeyword keyword);
        void UpdateKeyword(EventExclusionKeyword keyword);
        void DeleteKeyword(EventExclusionKeyword keyword);
        Task<bool> ExistsAsync(string keyword);
    }
}