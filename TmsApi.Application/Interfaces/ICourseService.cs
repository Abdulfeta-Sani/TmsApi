using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;


namespace TmsApi.Application.Interfaces;

public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct);

    Task<CourseResponseDto> CreateAsync(
        CreateCourseRequest request,
        CancellationToken ct);

    Task<bool> CodeExistsAsync(
        string code,
        CancellationToken ct);

    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(
        PagedRequest request,
        CancellationToken ct);

    Task<Course?> GetByCodeAsync(
        string courseCode,
        CancellationToken ct);

    Task<IReadOnlyList<Course>> GetAllAsync(
    CancellationToken ct);

    Task<bool> UpdateAsync(
        int id,
        string code,
        string title,
        int maxCapacity,
        CancellationToken ct);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken ct);

    Task<Course?> GetEntityByIdAsync(
        int id,
        CancellationToken ct);
}