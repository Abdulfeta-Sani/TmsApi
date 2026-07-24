using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Commands;

public class DeleteCourseHandler(
    ICourseService courseService,
    ICachedCourseService cachedCourseService)
    : IRequestHandler<DeleteCourseCommand, bool>
{
    public async Task<bool> Handle(
        DeleteCourseCommand command,
        CancellationToken ct)
    {
        var deleted = await courseService.DeleteAsync(command.Id, ct);

        if (!deleted)
        {
            return false;
        }

        await cachedCourseService.InvalidateCourseCacheAsync(ct);

        return true;
    }
}