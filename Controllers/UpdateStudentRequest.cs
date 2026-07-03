namespace TmsApi;

public record UpdateStudentRequest(
    string Name,
    decimal GPA,
    uint Version);