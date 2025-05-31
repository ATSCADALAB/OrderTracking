using Entities.Models;

namespace Contracts
{
    public interface ISendMailRepository
    {
        Task<IEnumerable<SendMail>> GetPendingSendMailsAsync(bool trackChanges);
        Task<SendMail?> GetByOrderCodeAsync(string orderCode, bool trackChanges);
        void CreateSendMail(SendMail sendMail);
        void UpdateSendMail(SendMail sendMail);
        Task<bool> ExistsAsync(string orderCode);
    }
}