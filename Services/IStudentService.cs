using TmsApi.Dtos;

namespace TmsApi.Services;

public interface IStudentService
{
    Task<StudentResponseDto?> GetByIdAsync(
        int id,
        CancellationToken ct);
    Task<StudentResponseDto> CreateAsync(
        CreateStudentRequest request,
        CancellationToken ct);
    Task<bool> RegistrationNumberExistsAsync(
        string registrationNumber,
        CancellationToken ct);
    Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(
        PagedRequest request,
        CancellationToken ct);
}