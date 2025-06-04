// Cập nhật CalendarUserKpiDto để bao gồm thông tin penalty
namespace Shared.DataTransferObjects.CalendarReport
{
    public record CalendarUserKpiDto
    {
        public string UserName { get; set; } = string.Empty;
        public int SmallOrders { get; set; }
        public int MediumOrders { get; set; }
        public int LargeOrders { get; set; }
        public double AverageStars { get; set; }
        public double TotalPenalty { get; set; } // Thêm tổng penalty từ PB
        public double RewardOrPenalty { get; set; }
        public string PenaltyDetails { get; set; } = string.Empty; // Thêm chi tiết penalty
    }
}