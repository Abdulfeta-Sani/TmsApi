using MediatR;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Queries;

public class SearchCoursesHandler(
    ICachedCourseService cachedCourseService)
    : IRequestHandler<SearchCoursesQuery, PagedResponse<CourseResponseDto>>
{
    public Task<PagedResponse<CourseResponseDto>> Handle(
        SearchCoursesQuery query,
        CancellationToken ct)
    {
        var request = new PagedRequest
        {
            Search = query.Term
        };

        return cachedCourseService.GetCoursesAsync(request, ct);
    }
}