namespace DatabaseMaintenance.Services;

public interface IDatabaseTask
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}