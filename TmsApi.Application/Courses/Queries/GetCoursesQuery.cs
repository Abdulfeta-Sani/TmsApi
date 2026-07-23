using MediatR;
using TmsApi.Application.Dtos;

namespace TmsApi.Application.Courses.Queries;

public record GetCoursesQuery(
    PagedRequest Request)
    : IRequest<PagedResponse<CourseResponseDto>>;