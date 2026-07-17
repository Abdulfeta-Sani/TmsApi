using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public record CreateStudentRequest
{
    [Required]
    [RegularExpression(
        @"^TMS-\d{4}-\d{4}$",
        ErrorMessage = "Registration number must follow the pattern TMS-YYYY-NNNN (e.g. TMS-2026-0001).")]
    public required string RegistrationNumber { get; init; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; init; }

    [Range(0.00, 4.00)]
    public decimal GPA { get; init; }

    public bool IsActive { get; init; } = true;
}