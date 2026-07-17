using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public record CreateCertificateRequest
{
    [Required]
    [MaxLength(50)]
    public required string SerialNumber { get; init; }

    [Required]
    public int StudentId { get; init; }

    [Required]
    public int CourseId { get; init; }
}