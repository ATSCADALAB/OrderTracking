using Shared.DataTransferObjects.External;

namespace Service.Contracts
{
    public interface ISendMailService
    {
        Task ProcessNewOrdersAsync();
        Task<OrderApiResponse?> GetOrderInfoAsync(string orderCode);
        Task<AccountApiResponse?> GetAccountInfoAsync(string accountCode);
        Task SendOrderNotificationEmailAsync(string orderCode, string email, OrderApiResponse orderInfo);
        Task UpdateSendMailStatusAsync(string orderCode, string status, string? errorMessage = null);
    }
}