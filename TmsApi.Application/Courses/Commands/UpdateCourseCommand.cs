using MediatR;

namespace TmsApi.Application.Courses.Commands;

public record UpdateCourseCommand(
    int Id,
    string Code,
    string Title,
    int MaxCapacity
) : IRequest<bool>;