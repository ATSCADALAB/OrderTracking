using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository
{
    internal sealed class SendMailRepository : RepositoryBase<SendMail>, ISendMailRepository
    {
        public SendMailRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {
        }

        public void CreateSendMail(SendMail sendMail)
        {
            Create(sendMail);
        }

        public void UpdateSendMail(SendMail sendMail)
        {
            Update(sendMail);
        }

        public async Task<bool> ExistsAsync(string orderCode)
        {
            return await FindByCondition(x => x.OrderCode == orderCode, false)
                .AnyAsync();
        }

        public async Task<SendMail?> GetByOrderCodeAsync(string orderCode, bool trackChanges)
        {
            return await FindByCondition(x => x.OrderCode == orderCode, trackChanges)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<SendMail>> GetPendingSendMailsAsync(bool trackChanges)
        {
            return await FindByCondition(x => x.Status == "Pending", trackChanges)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}