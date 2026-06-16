using Microsoft.Extensions.Logging;

public interface ICourseService
{
    Task<CourseRecord> CreateAsync(string code, string title, string? description);
    Task<CourseRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<CourseRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public class CourseService : ICourseService
{
    private readonly Dictionary<string, CourseRecord> _store = new();
    private readonly ILogger<CourseService> _logger;

    public CourseService(ILogger<CourseService> logger) => _logger = logger;

    public Task<CourseRecord> CreateAsync(string code, string title, string? description)
    {
        var existing = _store.Values.FirstOrDefault(c => c.Code == code);
        if (existing is not null)
        {
            _logger.LogWarning("Duplicate course create attempt for {Code} (id {CourseId})", code, existing.Id);
            return Task.FromResult(existing);
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new CourseRecord(id, code, title, description ?? string.Empty, DateTime.UtcNow);
        _store[id] = record;

        _logger.LogInformation("Created course {Code} id {CourseId}", code, id);
        return Task.FromResult(record);
    }

    public Task<CourseRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var record);
        if (record is null) _logger.LogWarning("Course {CourseId} not found", id);
        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<CourseRecord>> GetAllAsync()
    {
        IReadOnlyList<CourseRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);
        if (removed) _logger.LogInformation("Deleted course {CourseId}", id);
        else _logger.LogWarning("Delete failed course {CourseId} not found", id);
        return Task.FromResult(removed);
    }
}

public record CourseRecord(string Id, string Code, string Title, string Description, DateTime CreatedAt);
public record CreateCourseRequest(string Code, string Title, string? Description);
