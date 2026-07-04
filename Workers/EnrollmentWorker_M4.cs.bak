public class EnrollmentWorker
{
    private readonly IServiceScopeFactory
        _scopeFactory;

    public EnrollmentWorker(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory =
            scopeFactory;
    }

    public async Task<int> SmokeTestAsync()
    {
        // TODO2:Create a short-lived scope using the injected factory.
        using var scope =
            _scopeFactory.CreateScope();
        
        // TODO3:Resolve the scoped service from the new scope's provider.
        var enrollmentService =
            scope.ServiceProvider
            .GetRequiredService<
                IEnrollmentService>();

        // TODO4:Usetheservice, then let the 'using' block dispose the scope
        var tasks =
            Enumerable.Range(1, 15)
            .Select(i =>
                enrollmentService.EnrollAsync(
                    $"S-{i:000}",
                    "CS-101"));

        var results =
            await Task.WhenAll(tasks);

        return results.Length;
    }
}