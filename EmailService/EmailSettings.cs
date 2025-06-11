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
    }
}