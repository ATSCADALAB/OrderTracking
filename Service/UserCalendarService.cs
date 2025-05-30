using AutoMapper;
using Contracts;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects.UserCalendar;

namespace Service
{
    internal sealed class UserCalendarService : IUserCalendarService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public UserCalendarService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<UserCalendarDto> CreateUserCalendarAsync(UserCalendarForCreationDto userCalendarDto)
        {
            var entity = _mapper.Map<UserCalendar>(userCalendarDto);

            _repository.UserCalendar.CreateUserCalendar(entity);
            await _repository.SaveAsync();

            return _mapper.Map<UserCalendarDto>(entity);
        }

        public async Task DeleteUserCalendarAsync(Guid userCalendarId, bool trackChanges)
        {
            var entity = await GetUserCalendarAndCheckIfItExists(userCalendarId, trackChanges);
            _repository.UserCalendar.DeleteUserCalendar(entity);
            await _repository.SaveAsync();
        }

        public async Task<IEnumerable<UserCalendarDto>> GetAllUserCalendarsAsync(bool trackChanges)
        {
            var entities = await _repository.UserCalendar.GetUserCalendarsAsync(trackChanges);
            return _mapper.Map<IEnumerable<UserCalendarDto>>(entities);
        }

        public async Task<UserCalendarDto> GetUserCalendarAsync(Guid userCalendarId, bool trackChanges)
        {
            var entity = await GetUserCalendarAndCheckIfItExists(userCalendarId, trackChanges);
            return _mapper.Map<UserCalendarDto>(entity);
        }

        private async Task<UserCalendar> GetUserCalendarAndCheckIfItExists(Guid id, bool trackChanges)
        {
            var entity = await _repository.UserCalendar.GetUserCalendarAsync(id, trackChanges);
            if (entity is null)
                throw new Exception($"UserCalendar with id {id} was not found."); // Có thể tạo custom exception sau

            return entity;
        }
    }
}
