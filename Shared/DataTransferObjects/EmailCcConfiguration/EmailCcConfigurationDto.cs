using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.EmailCcConfiguration
{
    public record EmailCcConfigurationDto
    {
        public int Id { get; init; }
        public string ConfigKey { get; init; }
        public string ConfigName { get; init; }
        public bool IsEnabled { get; init; }
        public string? Description { get; init; }
        public List<string> DefaultCcEmails { get; init; } = new();
        public List<string> DefaultBccEmails { get; init; } = new();
        public bool AutoAddCcForOrders { get; init; }
        public bool AutoAddCcForNotifications { get; init; }
        public bool AutoAddCcForAlerts { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public string? CreatedBy { get; init; }
        public string? UpdatedBy { get; init; }
        public int Priority { get; init; }
    }

    public record EmailCcConfigurationForCreationDto
    {
        [Required]
        [StringLength(100)]
        public string ConfigKey { get; init; }

        [Required]
        [StringLength(200)]
        public string ConfigName { get; init; }

        public bool IsEnabled { get; init; } = true;
        public string? Description { get; init; }
        public List<string> DefaultCcEmails { get; init; } = new();
        public List<string> DefaultBccEmails { get; init; } = new();
        public bool AutoAddCcForOrders { get; init; } = false;
        public bool AutoAddCcForNotifications { get; init; } = false;
        public bool AutoAddCcForAlerts { get; init; } = false;
        public int Priority { get; init; } = 1;
    }

    public record EmailCcConfigurationForUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string ConfigName { get; init; }

        public bool IsEnabled { get; init; }
        public string? Description { get; init; }
        public List<string> DefaultCcEmails { get; init; } = new();
        public List<string> DefaultBccEmails { get; init; } = new();
        public bool AutoAddCcForOrders { get; init; }
        public bool AutoAddCcForNotifications { get; init; }
        public bool AutoAddCcForAlerts { get; init; }
        public int Priority { get; init; }
    }
}