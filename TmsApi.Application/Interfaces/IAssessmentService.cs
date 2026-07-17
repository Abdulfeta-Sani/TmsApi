using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;
public interface IAssessmentService
{
    Task<IReadOnlyList<AssessmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct);

    Task<AssessmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct);

    Task<AssessmentResponseDto> CreateAsync(
        int courseId,
        CreateAssessmentRequest request,
        CancellationToken ct);

    Task<bool> CourseExistsAsync(
        int courseId,
        CancellationToken ct);
}