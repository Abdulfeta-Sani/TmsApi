using MediatR;

namespace TmsApi.Application.Enrollments.Queries;

public record GetEnrollmentsQuery()
    : IRequest<IReadOnlyList<EnrollmentListItemDto>>;

public record GetEnrollmentByIdQuery(int Id)
    : IRequest<EnrollmentListItemDto?>;

public record EnrollmentListItemDto(
    int Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseName,
    DateTime EnrolledAt);