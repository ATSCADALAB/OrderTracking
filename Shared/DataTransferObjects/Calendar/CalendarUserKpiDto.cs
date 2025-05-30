namespace Shared.DataTransferObjects.CalendarReport
{
    public record CalendarUserKpiDto
    {
        public string UserName { get; set; } = string.Empty;
        public int SmallOrders { get; set; }
        public int MediumOrders { get; set; }
        public int LargeOrders { get; set; }
        public double AverageStars { get; set; }
        public double RewardOrPenalty { get; set; }
    }
}
