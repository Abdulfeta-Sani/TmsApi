using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public record SubmitGradeRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive integer.")]
    public required int StudentId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a positive integer.")]
    public required int CourseId { get; init; }

    [Range(typeof(decimal), "0", "100", ErrorMessage = "Score must be between 0 and 100.")]
    public required decimal Score { get; init; }
}