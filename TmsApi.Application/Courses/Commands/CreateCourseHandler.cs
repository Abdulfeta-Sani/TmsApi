using MediatR;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Dtos;

namespace TmsApi.Application.Courses.Commands;

public class CreateCourseHandler(
    ICourseService courseService,
    ICachedCourseService cachedCourseService)
    : IRequestHandler<CreateCourseCommand, int>
{
    public async Task<int> Handle(
        CreateCourseCommand command,
        CancellationToken ct)
    {
        var createdCourse = await courseService.CreateAsync(
            new CreateCourseRequest
            {
                Code = command.Code,
                Title = command.Title,
                MaxCapacity = command.MaxCapacity
            },
            ct);

        await cachedCourseService.InvalidateCourseCacheAsync(ct);

        return createdCourse.Id;
    }
}