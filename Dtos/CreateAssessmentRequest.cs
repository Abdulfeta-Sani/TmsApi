using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public record CreateAssessmentRequest
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; init; }

    [Range(1, 1000)]
    public decimal MaxScore { get; init; }

    [Range(typeof(decimal), "0.01", "1.00")]
    public decimal Weight { get; init; }
}