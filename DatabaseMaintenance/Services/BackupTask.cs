using Microsoft.Data.SqlClient;
using DatabaseMaintenance.Logging;
namespace DatabaseMaintenance.Services;

public class BackupTask : IDatabaseTask
{
    private readonly string _connectionString;
    private readonly IAppLogger _logger;
    private readonly string _databaseName;
    private readonly string _backupDirectory;

    public BackupTask(
        string connectionString,
        string databaseName,
        string backupDirectory,
        IAppLogger logger)
    {
        _connectionString = connectionString;
        _databaseName = databaseName;
        _backupDirectory = backupDirectory;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_backupDirectory))
        {
            _logger.Warning($"Directory does not exist: {_backupDirectory}");
            _logger.Warning($"Create it? (Y/N): : {_backupDirectory}");
            var response = Console.ReadLine();

            if (string.Equals(response, "Y", StringComparison.OrdinalIgnoreCase))
            {
                Directory.CreateDirectory(_backupDirectory);
                _logger.Warning($"Directory created: {_backupDirectory}");
            }
            else
            {
                throw new DirectoryNotFoundException(
                    $"Dacpac directory '{_backupDirectory}' does not exist.");
            }
        }
        var fileName =
            $"{_databaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";

        var backupPath =
            Path.Combine(_backupDirectory, fileName);

        var sql = $@"
        BACKUP DATABASE [{_databaseName}]
        TO DISK = N'{backupPath}'
        WITH INIT,
             COMPRESSION,
             CHECKSUM;";

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        command.CommandTimeout = 0;

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.Information($"New backup file: {backupPath}");
    }
}