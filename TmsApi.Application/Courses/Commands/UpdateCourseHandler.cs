using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Commands;

public class UpdateCourseHandler(
    ICourseService courseService,
    ICachedCourseService cachedCourseService)
    : IRequestHandler<UpdateCourseCommand, bool>
{
    public async Task<bool> Handle(
        UpdateCourseCommand command,
        CancellationToken ct)
    {
        var updated = await courseService.UpdateAsync(
            command.Id,
            command.Code,
            command.Title,
            command.MaxCapacity,
            ct);

        if (!updated)
        {
            return false;
        }

        await cachedCourseService.InvalidateCourseCacheAsync(ct);

        return true;
    }
}