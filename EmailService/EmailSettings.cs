using System.Collections.Generic;

namespace EmailService
{
    public class EmailSettings
    {
        public int MaxEmailsPerHour { get; set; } = 30;
        public int DelayBetweenEmailsSeconds { get; set; } = 60; // 1 phút
        public int MaxConcurrentConnections { get; set; } = 2;
        public int ProcessingIntervalHours { get; set; } = 2; // 3 tiếng
        public int BatchSize { get; set; } = 3; // Chỉ 3 emails mỗi lần
        public int CircuitBreakerFailureThreshold { get; set; } = 3;
        public int CircuitBreakerTimeoutMinutes { get; set; } = 15;
        public bool EnableCcTracking { get; set; } = true; // Bật/tắt CC tracking
        public List<string> DefaultCcEmails { get; set; } = new List<string>(); // CC mặc định
        public List<string> DefaultBccEmails { get; set; } = new List<string>(); // BCC mặc định
        public bool AutoAddCcForOrders { get; set; } = true; // Tự động CC cho đơn hàng
        public string OrderTrackingCcEmail { get; set; } = ""; // Email CC cho tracking đơn hàng
    }
}