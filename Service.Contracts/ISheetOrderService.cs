namespace Service.Contracts
{
    using Shared.DataTransferObjects.SheetOrder;

    public interface ISheetOrderService
    {
        Task<IEnumerable<SheetOrderDto>> GetAllAsync(bool trackChanges);
        Task<SheetOrderDto?> GetByOrderCodeAsync(string orderCode, string sheetName, bool trackChanges);
        Task CreateIfNotExistsAsync(SheetOrderDto dto);
    }
}