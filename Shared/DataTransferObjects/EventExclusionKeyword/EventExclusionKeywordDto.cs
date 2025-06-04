using System.ComponentModel.DataAnnotations;

namespace Shared.DataTransferObjects.EventExclusion
{
    public record EventExclusionKeywordDto
    {
        public int Id { get; init; }
        public string Keyword { get; init; } = string.Empty;
        public string? Description { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public record EventExclusionKeywordForCreationDto
    {
        [Required(ErrorMessage = "Keyword is required")]
        [StringLength(100, ErrorMessage = "Keyword cannot exceed 100 characters")]
        public string Keyword { get; init; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string? Description { get; init; }

        public bool IsActive { get; init; } = true;
    }

    public record EventExclusionKeywordForUpdateDto
    {
        [Required(ErrorMessage = "Keyword is required")]
        [StringLength(100, ErrorMessage = "Keyword cannot exceed 100 characters")]
        public string Keyword { get; init; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string? Description { get; init; }

        public bool IsActive { get; init; } = true;
    }
}