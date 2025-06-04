using Shared.DataTransferObjects.EventExclusion;

namespace Service.Contracts
{
    public interface IEventExclusionKeywordService
    {
        Task<IEnumerable<EventExclusionKeywordDto>> GetAllKeywordsAsync();
        Task<IEnumerable<EventExclusionKeywordDto>> GetActiveKeywordsAsync();
        Task<EventExclusionKeywordDto?> GetKeywordByIdAsync(int id);
        Task<EventExclusionKeywordDto> CreateKeywordAsync(EventExclusionKeywordForCreationDto keywordDto);
        Task UpdateKeywordAsync(int id, EventExclusionKeywordForUpdateDto keywordDto);
        Task DeleteKeywordAsync(int id);
        Task<bool> ShouldExcludeEventAsync(string eventTitle);
    }
}