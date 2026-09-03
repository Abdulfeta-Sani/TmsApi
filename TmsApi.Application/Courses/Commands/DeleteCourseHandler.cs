using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Courses.Commands;

public class DeleteCourseHandler(
    ICourseService courseService,
    ICachedCourseService cachedCourseService)
    : IRequestHandler<DeleteCourseCommand, CourseDeletionResult>
{
    public async Task<CourseDeletionResult> Handle(
        DeleteCourseCommand command,
        CancellationToken ct)
    {
        var result = await courseService.DeleteAsync(command.Id, ct);

        if (result == CourseDeletionResult.Deleted)
        {
            await cachedCourseService.InvalidateCourseCacheAsync(ct);
        }

        return result;
    }
}