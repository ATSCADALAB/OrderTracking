namespace Contracts
{
    using Entities.Models;

    public interface ISheetOrderRepository
    {
        Task<IEnumerable<SheetOrder>> GetAllAsync(bool trackChanges);
        Task<SheetOrder?> GetByOrderCodeAsync(string orderCode);
        Task<bool> ExistsAsync(string orderCode, string sheetName);
        void CreateSheetOrder(SheetOrder order);
    }
}