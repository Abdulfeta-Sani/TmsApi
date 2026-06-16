using Microsoft.Extensions.Logging;

public interface IStudentService
{
    Task<StudentRecord> CreateAsync(string firstName, string lastName, string email);
    Task<StudentRecord?> GetByIdAsync(string id);
    Task<IReadOnlyList<StudentRecord>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}

public class StudentService : IStudentService
{
    private readonly Dictionary<string, StudentRecord> _store = new();
    private readonly ILogger<StudentService> _logger;

    public StudentService(ILogger<StudentService> logger) => _logger = logger;

    public Task<StudentRecord> CreateAsync(string firstName, string lastName, string email)
    {
        var existing = _store.Values.FirstOrDefault(s => s.Email == email);
        if (existing is not null)
        {
            _logger.LogWarning("Duplicate student create attempt for {Email} (id {StudentId})", email, existing.Id);
            return Task.FromResult(existing);
        }

        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new StudentRecord(id, firstName, lastName, email, DateTime.UtcNow);
        _store[id] = record;

        _logger.LogInformation("Created student {Email} id {StudentId}", email, id);
        return Task.FromResult(record);
    }

    public Task<StudentRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var record);
        if (record is null) _logger.LogWarning("Student {StudentId} not found", id);
        return Task.FromResult(record);
    }

    public Task<IReadOnlyList<StudentRecord>> GetAllAsync()
    {
        IReadOnlyList<StudentRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }

    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);
        if (removed) _logger.LogInformation("Deleted student {StudentId}", id);
        else _logger.LogWarning("Delete failed student {StudentId} not found", id);

        return Task.FromResult(removed);
    }
}

public record StudentRecord(string Id, string FirstName, string LastName, string Email, DateTime CreatedAt);
public record CreateStudentRequest(string FirstName, string LastName, string Email);
