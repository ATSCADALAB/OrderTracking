using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.Logging;
using Shared.DataTransferObjects.CalendarReport;
using System;
using System.Threading.Tasks;

namespace Service.Contracts
{
    public interface ICalendarReportService
    {
        Task<byte[]> GenerateReportAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<CalendarUserKpiDto>> GetUserKpiReportAsync(DateTime startDate, DateTime endDate);
        Task<UnifiedOrderDto?> SearchOrderFromAllSourcesAsync(string orderCode);
        Task SyncSheetOrdersAsync(CancellationToken cancellationToken);
        Task SyncCalendarEventsAsync(CancellationToken cancellationToken);
    }
}
