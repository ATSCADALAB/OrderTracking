using Shared.DataTransferObjects.KpiConfiguration;

namespace Service.Contracts
{
    public interface IKpiConfigurationService
    {
        Task<KpiConfigurationDto?> GetActiveConfigurationAsync();
        Task<KpiConfigurationDto?> GetConfigurationByIdAsync(int id);
        Task<IEnumerable<KpiConfigurationDto>> GetAllConfigurationsAsync();
        Task<KpiConfigurationDto> CreateConfigurationAsync(KpiConfigurationForCreationDto configDto);
        Task UpdateConfigurationAsync(int id, KpiConfigurationForUpdateDto configDto);
        Task DeleteConfigurationAsync(int id);
        Task<KpiConfigurationDto> SetActiveConfigurationAsync(int id);
        Task<KpiConfigurationDto> CreateDefaultConfigurationAsync();

        // Helper methods for KPI calculation
        Task<int> CalculateStarsAsync(int daysLate);
        Task<(int lightOrders, int mediumOrders, int heavyOrders)> CategorizeOrdersAsync(IEnumerable<int> orderDays);
        Task<decimal> CalculateHSSLAsync(int lightOrders, int mediumOrders, int heavyOrders);
        Task<decimal> CalculateRewardAsync(decimal averageStars, decimal hssl, decimal penalty);
    }
}