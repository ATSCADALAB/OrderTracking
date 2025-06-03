using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class KpiConfiguration
    {
        [Key]
        public int Id { get; set; }

        // Cấu hình chấm điểm sao (theo tiến độ so với deadline)
        [Required]
        public int Stars_EarlyCompletion { get; set; } = 5; // Hoàn thành sớm + QC đạt
        [Required]
        public int Stars_OnTime { get; set; } = 4; // Đúng hạn + QC đạt
        [Required]
        public int Stars_Late1Day { get; set; } = 3; // Trễ 1 ngày
        [Required]
        public int Stars_Late2Days { get; set; } = 2; // Trễ 2 ngày
        [Required]
        public int Stars_Late3OrMoreDays { get; set; } = 1; // Trễ 3+ ngày

        // Cấu hình phân loại đơn hàng theo timeline
        [Required]
        public int LightOrder_MaxDays { get; set; } = 5; // SLn: Đơn nhẹ <5 ngày
        [Required]
        public int MediumOrder_MinDays { get; set; } = 5; // SLv: Đơn vừa 5-19 ngày
        [Required]
        public int MediumOrder_MaxDays { get; set; } = 19;
        [Required]
        public int HeavyOrder_MinDays { get; set; } = 20; // SLl: Đơn nặng ≥20 ngày

        // Cấu hình hệ số số lượng (HSSL)
        [Required]
        public int HSSL_LightOrderFreeCount { get; set; } = 5; // 5 đơn nhẹ đầu tiên không tính điểm
        [Required]
        public decimal HSSL_LightOrderMultiplier { get; set; } = 0.1m; // Đơn nhẹ thứ 6+ x0.1
        [Required]
        public decimal HSSL_MediumOrderMultiplier { get; set; } = 1.0m; // Đơn vừa x1
        [Required]
        public decimal HSSL_HeavyOrderMultiplier { get; set; } = 2.0m; // Đơn nặng x2

        // Cấu hình phạt lỗi (PB)
        [Required]
        public decimal Penalty_HeavyError { get; set; } = 500m; // Lỗi nặng
        [Required]
        public decimal Penalty_LightError { get; set; } = 100m; // Lỗi nhẹ
        [Required]
        public decimal Penalty_NoError { get; set; } = 0m; // Không lỗi

        // Cấu hình thưởng/phạt
        [Required]
        public decimal Reward_HighPerformance_MinStars { get; set; } = 3.4m; // TD >= 3.4
        [Required]
        public decimal Reward_HighPerformance_MaxStars { get; set; } = 5.0m; // TD <= 5.0
        [Required]
        public decimal Reward_BaseAmount { get; set; } = 2500000m; // 2.5M cơ sở
        [Required]
        public decimal Reward_BasePenalty { get; set; } = 500000m; // 500K cơ sở

        [Required]
        public decimal Penalty_MediumPerformance_MinStars { get; set; } = 3.0m; // 3.0 <= TD < 3.4
        [Required]
        public decimal Penalty_MediumPerformance_MaxStars { get; set; } = 3.4m;

        [Required]
        public decimal Penalty_LowPerformance_MinStars { get; set; } = 1.0m; // 1.0 <= TD < 3.0
        [Required]
        public decimal Penalty_LowPerformance_MaxStars { get; set; } = 3.0m;
        [Required]
        public decimal Penalty_LowPerformance_BaseAmount { get; set; } = 500000m; // 500K
        [Required]
        public decimal Penalty_LowPerformance_MaxPenalty { get; set; } = 1000000m; // 1M

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}