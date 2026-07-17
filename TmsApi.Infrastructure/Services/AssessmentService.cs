using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class AssessmentService(
    TmsDbContext context,
    ILogger<AssessmentService> logger)
    : IAssessmentService
{
    public async Task<IReadOnlyList<AssessmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct)
    {
        return await context.Assessments
            .AsNoTracking()
            .Where(a => a.CourseId == courseId)
            .OrderBy(a => a.Title)
            .Select(a => new AssessmentResponseDto(
                a.Id,
                a.Title,
                a.MaxScore,
                a.Weight,
                a.CourseId))
            .ToListAsync(ct);
    }

    public Task<AssessmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct)
    {
        return context.Assessments
            .AsNoTracking()
            .Where(a => a.CourseId == courseId && a.Id == id)
            .Select(a => new AssessmentResponseDto(
                a.Id,
                a.Title,
                a.MaxScore,
                a.Weight,
                a.CourseId))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<AssessmentResponseDto> CreateAsync(
        int courseId,
        CreateAssessmentRequest request,
        CancellationToken ct)
    {
        var assessment = new Assessment
        {
            CourseId = courseId,
            Title = request.Title,
            MaxScore = request.MaxScore,
            Weight = request.Weight
        };

        context.Assessments.Add(assessment);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created assessment {AssessmentId} for course {CourseId}",
            assessment.Id,
            courseId);

        return (await GetByIdAsync(courseId, assessment.Id, ct))!;
    }

    public Task<bool> CourseExistsAsync(
        int courseId,
        CancellationToken ct)
    {
        return context.Courses
            .AsNoTracking()
            .AnyAsync(c => c.Id == courseId, ct);
    }
}