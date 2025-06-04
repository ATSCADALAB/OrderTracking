using AutoMapper;
using Contracts;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.EventExclusion;

namespace Service
{
    internal sealed class EventExclusionKeywordService : IEventExclusionKeywordService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public EventExclusionKeywordService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EventExclusionKeywordDto>> GetAllKeywordsAsync()
        {
            var keywords = await _repository.EventExclusionKeyword.GetAllKeywordsAsync(false);
            return _mapper.Map<IEnumerable<EventExclusionKeywordDto>>(keywords);
        }

        public async Task<IEnumerable<EventExclusionKeywordDto>> GetActiveKeywordsAsync()
        {
            var keywords = await _repository.EventExclusionKeyword.GetActiveKeywordsAsync(false);
            return _mapper.Map<IEnumerable<EventExclusionKeywordDto>>(keywords);
        }

        public async Task<EventExclusionKeywordDto?> GetKeywordByIdAsync(int id)
        {
            var keyword = await _repository.EventExclusionKeyword.GetKeywordByIdAsync(id, false);
            return keyword == null ? null : _mapper.Map<EventExclusionKeywordDto>(keyword);
        }

        public async Task<EventExclusionKeywordDto> CreateKeywordAsync(EventExclusionKeywordForCreationDto keywordDto)
        {
            // Check if keyword already exists
            var exists = await _repository.EventExclusionKeyword.ExistsAsync(keywordDto.Keyword);
            if (exists)
                throw new Exception($"Keyword '{keywordDto.Keyword}' already exists");

            var keyword = _mapper.Map<EventExclusionKeyword>(keywordDto);
            keyword.CreatedAt = DateTime.UtcNow;

            _repository.EventExclusionKeyword.CreateKeyword(keyword);
            await _repository.SaveAsync();

            return _mapper.Map<EventExclusionKeywordDto>(keyword);
        }

        public async Task UpdateKeywordAsync(int id, EventExclusionKeywordForUpdateDto keywordDto)
        {
            var keyword = await _repository.EventExclusionKeyword.GetKeywordByIdAsync(id, true);
            if (keyword == null)
                throw new Exception($"Keyword with id '{id}' not found");

            // Check if new keyword already exists (except current one)
            var existingKeyword = await _repository.EventExclusionKeyword
                .GetActiveKeywordsAsync(false);

            if (existingKeyword.Any(k => k.Id != id &&
                string.Equals(k.Keyword, keywordDto.Keyword, StringComparison.OrdinalIgnoreCase)))
                throw new Exception($"Keyword '{keywordDto.Keyword}' already exists");

            _mapper.Map(keywordDto, keyword);
            keyword.UpdatedAt = DateTime.UtcNow;

            _repository.EventExclusionKeyword.UpdateKeyword(keyword);
            await _repository.SaveAsync();
        }

        public async Task DeleteKeywordAsync(int id)
        {
            var keyword = await _repository.EventExclusionKeyword.GetKeywordByIdAsync(id, true);
            if (keyword == null)
                throw new Exception($"Keyword with id '{id}' not found");

            _repository.EventExclusionKeyword.DeleteKeyword(keyword);
            await _repository.SaveAsync();
        }

        public async Task<bool> ShouldExcludeEventAsync(string eventTitle)
        {
            if (string.IsNullOrWhiteSpace(eventTitle))
                return false;

            var activeKeywords = await _repository.EventExclusionKeyword.GetActiveKeywordsAsync(false);

            return activeKeywords.Any(keyword =>
                eventTitle.Contains(keyword.Keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}