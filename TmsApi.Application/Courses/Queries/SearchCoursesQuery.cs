using MediatR;
using TmsApi.Application.Dtos;

namespace TmsApi.Application.Courses.Queries;

public record SearchCoursesQuery(
    string? Term)
    : IRequest<PagedResponse<CourseResponseDto>>;