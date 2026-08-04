using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;
using TmsApi.Application.Enrollments.Queries;


namespace TmsApi.Application.Interfaces;
public interface IEnrollmentService
{
    Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId, int id, 
        CancellationToken ct);

    Task<EnrollmentResponseDto> CreateAsync(
        int courseId, EnrollStudentRequest request, 
        CancellationToken ct);

    Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId, 
        CancellationToken ct);

    Task<bool> ExistsAsync(
        int studentId,
        string courseCode,
        CancellationToken ct);

    Task AddAsync(
        Enrollment enrollment,
        CancellationToken ct);

    Task<List<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct);

    Task<IReadOnlyList<EnrollmentListItemDto>> GetAllAsync(
        CancellationToken ct);

    Task<EnrollmentListItemDto?> GetByIdAsync(
        int id,
        CancellationToken ct);
}