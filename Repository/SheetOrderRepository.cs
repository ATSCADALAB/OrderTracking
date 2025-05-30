using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Repository
{
    internal sealed class SheetOrderRepository : RepositoryBase<SheetOrder>, ISheetOrderRepository
    {
        public SheetOrderRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public async Task<IEnumerable<SheetOrder>> GetAllAsync(bool trackChanges) =>
            await FindAll(trackChanges).ToListAsync();

        public async Task<SheetOrder?> GetByOrderCodeAsync(string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
            {
                return null;
            }

            orderCode = orderCode.Trim();
            if (!Regex.IsMatch(orderCode, @"^[a-zA-Z0-9-]+$"))
            {
                return null;
            }

            return await FindByCondition(x => x.OrderCode.Contains(orderCode), false)
                .SingleOrDefaultAsync();
        }

        public async Task<bool> ExistsAsync(string orderCode, string sheetName) =>
            await FindByCondition(x => x.OrderCode == orderCode && x.SheetName == sheetName, false)
                .AnyAsync();

        public void CreateSheetOrder(SheetOrder order)
        {
            Create(order);
        }     
    }
}
