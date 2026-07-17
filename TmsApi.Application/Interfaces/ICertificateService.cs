using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;
public interface ICertificateService
{
    Task<CertificateResponseDto?> GetByIdAsync(int id, CancellationToken ct);

    Task<CertificateResponseDto> CreateAsync(
        CreateCertificateRequest request,
        CancellationToken ct);

    Task<bool> SerialNumberExistsAsync(
        string serialNumber,
        CancellationToken ct);

    Task<PagedResponse<CertificateResponseDto>> GetCertificatesAsync(
        PagedRequest request,
        CancellationToken ct);
}