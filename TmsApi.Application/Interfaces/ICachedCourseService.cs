using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;

public interface ICachedCourseService
{
    Task<CourseResponseDto> GetCourseAsync(
        string code,
        CancellationToken ct);

    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct);

    Task InvalidateCourseCacheAsync(
        CancellationToken ct);
}