namespace Service
{
    using AutoMapper;
    using Contracts;
    using Entities.Models;
    using global::Contracts;
    using Service.Contracts;
    using Shared.DataTransferObjects.SheetOrder;

    public sealed class SheetOrderService : ISheetOrderService
    {
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;

        public SheetOrderService(IRepositoryManager repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SheetOrderDto>> GetAllAsync(bool trackChanges)
        {
            var entities = await _repository.SheetOrder.GetAllAsync(trackChanges);
            return _mapper.Map<IEnumerable<SheetOrderDto>>(entities);
        }

        public async Task<SheetOrderDto?> GetByOrderCodeAsync(string orderCode, string sheetName, bool trackChanges)
        {
            var entity = await _repository.SheetOrder.GetByOrderCodeAsync(orderCode);
            return entity is null ? null : _mapper.Map<SheetOrderDto>(entity);
        }

        public async Task CreateIfNotExistsAsync(SheetOrderDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.OrderCode))
                    throw new ArgumentException("OrderCode is required");

                bool exists = await _repository.SheetOrder.ExistsAsync(dto.OrderCode, dto.SheetName ?? "");

                if (!exists)
                {
                    var entity = _mapper.Map<SheetOrder>(dto);

                    // Optional: check again that entity is not null and required fields set
                    if (string.IsNullOrWhiteSpace(entity.OrderCode))
                        throw new Exception("Mapped entity missing OrderCode");

                    _repository.SheetOrder.CreateSheetOrder(entity);
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