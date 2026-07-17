using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class StudentService(
    TmsDbContext context,
    ILogger<StudentService> logger)
    : IStudentService
{
    public Task<StudentResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct)
        =>
        context.Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentResponseDto(
                    s.Id,
                    s.RegistrationNumber,
                    s.Name,
                    s.GPA,
                    s.IsActive,
                    s.Enrollments.Count))
            .FirstOrDefaultAsync(ct);

    public async Task<StudentResponseDto> CreateAsync(
    CreateStudentRequest request,
    CancellationToken ct)
    {
        var student = new Student
        {
            RegistrationNumber = request.RegistrationNumber,
            Name = request.Name,
            GPA = request.GPA,
            IsActive = request.IsActive
        };

        context.Students.Add(student);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created student {StudentId} ({RegistrationNumber})",
            student.Id,
            student.RegistrationNumber);

        return (await GetByIdAsync(student.Id, ct))!;
    }

    public Task<bool> RegistrationNumberExistsAsync(
    string registrationNumber,
    CancellationToken ct)
    {
        return context.Students
            .AsNoTracking()
            .AnyAsync(
                s => s.RegistrationNumber == registrationNumber, ct);
    }

    public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(
    PagedRequest request,
    CancellationToken ct)
    {
        IQueryable<Student> query = context.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(s =>
                EF.Functions.ILike(s.Name, $"%{request.Search}%") ||
                EF.Functions.ILike(s.RegistrationNumber, $"%{request.Search}%"));
        }

        var totalCount = await query.CountAsync(ct);

        IQueryable<Student> sortedQuery = request.OrderBy switch
        {
            "RegistrationNumber" => request.Descending
                ? query.OrderByDescending(s => s.RegistrationNumber)
                : query.OrderBy(s => s.RegistrationNumber),

            "GPA" => request.Descending
                ? query.OrderByDescending(s => s.GPA)
                : query.OrderBy(s => s.GPA),

            _ => request.Descending
                ? query.OrderByDescending(s => s.Name)
                : query.OrderBy(s => s.Name)
        };

        var items = await sortedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StudentResponseDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.GPA,
                s.IsActive,
                s.Enrollments.Count))
            .ToListAsync(ct);

        return new PagedResponse<StudentResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

}