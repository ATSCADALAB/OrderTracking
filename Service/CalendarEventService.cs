namespace Service
{
    using AutoMapper;
    using Contracts;
    using Entities.Models;
    using global::Contracts;
    using Service.Contracts;
    using Shared.DataTransferObjects.CalendarEvent;

    public sealed class CalendarEventService : ICalendarEventService
    {
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;

        public CalendarEventService(IRepositoryManager repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task<CalendarEventDto?> GetByOrderCodeAsync(string orderCode, bool trackChanges)
        {
            var entity = await _repository.CalendarEvent.GetByOrderCodeAsync(orderCode);
            return entity is null ? null : _mapper.Map<CalendarEventDto>(entity);
        }
        public async Task CreateIfNotExistsAsync(CalendarEventDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.OrderCode))
                    throw new ArgumentException("OrderCode is required");

                bool exists = await _repository.CalendarEvent.ExistsAsync(dto.OrderCode);

                if (!exists)
                {
                    var entity = _mapper.Map<CalendarEvent>(dto);

                    // Optional: check again that entity is not null and required fields set
                    if (string.IsNullOrWhiteSpace(entity.OrderCode))
                        throw new Exception("Mapped entity missing OrderCode");

                    _repository.CalendarEvent.CreateCalendarEvent(entity);
                    await _repository.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                // Log chi tiết thay vì throw chung chung
                Console.WriteLine($"[ERROR] {ex.Message} | Inner: {ex.InnerException?.Message}");
                throw; // re-throw for debug
            }
        }

    }
}