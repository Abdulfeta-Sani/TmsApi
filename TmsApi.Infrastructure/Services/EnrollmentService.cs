using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;
using TmsApi.Application.Interfaces;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Enrollments.Queries;

namespace TmsApi.Infrastructure.Services;

public class EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger) : IEnrollmentService
{
    public Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct)
        =>
        context.Enrollments
            .AsNoTracking()
            .Where(e =>
                e.Id == id &&
                e.CourseId == courseId)
            .Select(e =>
                new EnrollmentResponseDto(
                    e.Id,
                    e.CourseId,
                    e.StudentId,
                    e.EnrolledAt,
                    e.Status.ToString()))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt,
                e.Status.ToString()))
            .ToListAsync(ct);
    }

    public async Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow,
            Status = EnrollmentStatus.Pending
        };

        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Student {StudentId} enrolled in course {CourseId}",
            request.StudentId,
            courseId);

        return (await GetByIdAsync(
            courseId,
            enrollment.Id,
            ct))!;
    }

    public async Task<bool> ExistsAsync(
        int studentId,
        string courseCode,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AnyAsync(e =>
                e.StudentId == studentId &&
                e.Course.Code == courseCode,
                ct);
    }

    public async Task AddAsync(
        Enrollment enrollment,
        CancellationToken ct)
    {
        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Student {StudentId} enrolled in course {CourseId}",
            enrollment.StudentId,
            enrollment.CourseId);
    }

    public async Task<List<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<EnrollmentListItemDto>> GetAllAsync(CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Select(e => new EnrollmentListItemDto(
                e.Id,
                e.StudentId,
                e.Student.Name,
                e.CourseId,
                e.Course.Title,
                e.EnrolledAt,
                e.Status.ToString()))
            .ToListAsync(ct);
    }

    public async Task<EnrollmentListItemDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => e.Id == id)
            .Select(e => new EnrollmentListItemDto(
                e.Id,
                e.StudentId,
                e.Student.Name,
                e.CourseId,
                e.Course.Title,
                e.EnrolledAt,
                e.Status.ToString()))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> UpdateStatusAsync(
        int enrollmentId,
        EnrollmentStatus status,
        CancellationToken ct)
    {
        var enrollment = await context.Enrollments
            .FirstOrDefaultAsync(e => e.Id == enrollmentId, ct);

        if (enrollment is null)
        {
            return false;
        }

        enrollment.Status = status;
        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Enrollment {EnrollmentId} status changed to {Status}",
            enrollmentId,
            status);

        return true;
    }
}