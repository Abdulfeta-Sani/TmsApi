namespace TmsApi.Application.Courses.Commands;

public enum CourseDeletionResult
{
    Deleted,
    NotFound,
    HasEnrollments
}