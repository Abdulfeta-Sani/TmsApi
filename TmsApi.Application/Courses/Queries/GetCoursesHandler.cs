using MediatR;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Queries;

public class GetCoursesHandler(
    ICachedCourseService cachedCourseService)
    : IRequestHandler<GetCoursesQuery, PagedResponse<CourseResponseDto>>
{
    public Task<PagedResponse<CourseResponseDto>> Handle(
        GetCoursesQuery query,
        CancellationToken ct)
    {
        return cachedCourseService.GetCoursesAsync(
            query.Request,
            ct);
    }
}