namespace Shared.DataTransferObjects.KpiConfiguration
{
    public record KpiConfigurationDto
    {
        public int Id { get; init; }

        // Stars configuration
        public int Stars_EarlyCompletion { get; init; }
        public int Stars_OnTime { get; init; }
        public int Stars_Late1Day { get; init; }
        public int Stars_Late2Days { get; init; }
        public int Stars_Late3OrMoreDays { get; init; }

        // Order categorization
        public int LightOrder_MaxDays { get; init; }
        public int MediumOrder_MinDays { get; init; }
        public int MediumOrder_MaxDays { get; init; }
        public int HeavyOrder_MinDays { get; init; }

        // HSSL configuration
        public int HSSL_LightOrderFreeCount { get; init; }
        public decimal HSSL_LightOrderMultiplier { get; init; }
        public decimal HSSL_MediumOrderMultiplier { get; init; }
        public decimal HSSL_HeavyOrderMultiplier { get; init; }

        // Penalty configuration
        public decimal Penalty_HeavyError { get; init; }
        public decimal Penalty_LightError { get; init; }
        public decimal Penalty_NoError { get; init; }

        // Reward/Penalty thresholds
        public decimal Reward_HighPerformance_MinStars { get; init; }
        public decimal Reward_HighPerformance_MaxStars { get; init; }
        public decimal Reward_BaseAmount { get; init; }
        public decimal Reward_BasePenalty { get; init; }
        public decimal Penalty_MediumPerformance_MinStars { get; init; }
        public decimal Penalty_MediumPerformance_MaxStars { get; init; }
        public decimal Penalty_LowPerformance_MinStars { get; init; }
        public decimal Penalty_LowPerformance_MaxStars { get; init; }
        public decimal Penalty_LowPerformance_BaseAmount { get; init; }
        public decimal Penalty_LowPerformance_MaxPenalty { get; init; }

        // Metadata
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public string? Description { get; init; }
        public bool IsActive { get; init; }
    }

    public record KpiConfigurationForCreationDto
    {
        // Stars configuration
        public int Stars_EarlyCompletion { get; init; } = 5;
        public int Stars_OnTime { get; init; } = 4;
        public int Stars_Late1Day { get; init; } = 3;
        public int Stars_Late2Days { get; init; } = 2;
        public int Stars_Late3OrMoreDays { get; init; } = 1;

        // Order categorization
        public int LightOrder_MaxDays { get; init; } = 5;
        public int MediumOrder_MinDays { get; init; } = 5;
        public int MediumOrder_MaxDays { get; init; } = 19;
        public int HeavyOrder_MinDays { get; init; } = 20;

        // HSSL configuration
        public int HSSL_LightOrderFreeCount { get; init; } = 5;
        public decimal HSSL_LightOrderMultiplier { get; init; } = 0.1m;
        public decimal HSSL_MediumOrderMultiplier { get; init; } = 1.0m;
        public decimal HSSL_HeavyOrderMultiplier { get; init; } = 2.0m;

        // Penalty configuration
        public decimal Penalty_HeavyError { get; init; } = 500m;
        public decimal Penalty_LightError { get; init; } = 100m;
        public decimal Penalty_NoError { get; init; } = 0m;

        // Reward/Penalty configuration
        public decimal Reward_HighPerformance_MinStars { get; init; } = 3.4m;
        public decimal Reward_HighPerformance_MaxStars { get; init; } = 5.0m;
        public decimal Reward_BaseAmount { get; init; } = 2500000m;
        public decimal Reward_BasePenalty { get; init; } = 500000m;
        public decimal Penalty_MediumPerformance_MinStars { get; init; } = 3.0m;
        public decimal Penalty_MediumPerformance_MaxStars { get; init; } = 3.4m;
        public decimal Penalty_LowPerformance_MinStars { get; init; } = 1.0m;
        public decimal Penalty_LowPerformance_MaxStars { get; init; } = 3.0m;
        public decimal Penalty_LowPerformance_BaseAmount { get; init; } = 500000m;
        public decimal Penalty_LowPerformance_MaxPenalty { get; init; } = 1000000m;

        public string? Description { get; init; }
    }

    public record KpiConfigurationForUpdateDto
    {
        // Stars configuration
        public int Stars_EarlyCompletion { get; init; }
        public int Stars_OnTime { get; init; }
        public int Stars_Late1Day { get; init; }
        public int Stars_Late2Days { get; init; }
        public int Stars_Late3OrMoreDays { get; init; }

        // Order categorization
        public int LightOrder_MaxDays { get; init; }
        public int MediumOrder_MinDays { get; init; }
        public int MediumOrder_MaxDays { get; init; }
        public int HeavyOrder_MinDays { get; init; }

        // HSSL configuration
        public int HSSL_LightOrderFreeCount { get; init; }
        public decimal HSSL_LightOrderMultiplier { get; init; }
        public decimal HSSL_MediumOrderMultiplier { get; init; }
        public decimal HSSL_HeavyOrderMultiplier { get; init; }

        // Penalty configuration
        public decimal Penalty_HeavyError { get; init; }
        public decimal Penalty_LightError { get; init; }
        public decimal Penalty_NoError { get; init; }

        // Reward/Penalty configuration
        public decimal Reward_HighPerformance_MinStars { get; init; }
        public decimal Reward_HighPerformance_MaxStars { get; init; }
        public decimal Reward_BaseAmount { get; init; }
        public decimal Reward_BasePenalty { get; init; }
        public decimal Penalty_MediumPerformance_MinStars { get; init; }
        public decimal Penalty_MediumPerformance_MaxStars { get; init; }
        public decimal Penalty_LowPerformance_MinStars { get; init; }
        public decimal Penalty_LowPerformance_MaxStars { get; init; }
        public decimal Penalty_LowPerformance_BaseAmount { get; init; }
        public decimal Penalty_LowPerformance_MaxPenalty { get; init; }

        public string? Description { get; init; }
        public bool IsActive { get; init; } = true;
    }
}