using MediatR;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Queries;

public class GetEnrollmentsHandler(IEnrollmentService enrollmentService)
    : IRequestHandler<GetEnrollmentsQuery, IReadOnlyList<EnrollmentListItemDto>>,
      IRequestHandler<GetEnrollmentByIdQuery, EnrollmentListItemDto?>
{
    public async Task<IReadOnlyList<EnrollmentListItemDto>> Handle(
        GetEnrollmentsQuery request,
        CancellationToken ct)
    {
        return await enrollmentService.GetAllAsync(ct);
    }

    public async Task<EnrollmentListItemDto?> Handle(
        GetEnrollmentByIdQuery request,
        CancellationToken ct)
    {
        return await enrollmentService.GetByIdAsync(request.Id, ct);
    }
}