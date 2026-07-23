using MediatR;

namespace TmsApi.Application.Courses.Commands;

public record CreateCourseCommand(
    string Code,
    string Title,
    int MaxCapacity
) : IRequest<int>;