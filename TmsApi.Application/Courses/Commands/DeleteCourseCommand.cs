using MediatR;

namespace TmsApi.Application.Courses.Commands;

public record DeleteCourseCommand(
    int Id
) : IRequest<bool>;