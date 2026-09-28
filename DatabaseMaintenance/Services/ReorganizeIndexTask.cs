using Microsoft.Data.SqlClient;

namespace DatabaseMaintenance.Services;

internal class ReorganizeIndexTask : IDatabaseTask
{
    private readonly string _connectionString;
    private readonly string _databaseName;

    public ReorganizeIndexTask(
        string connectionString,
        string databaseName)
    {
        _connectionString = connectionString;
        _databaseName = databaseName;
    }

    public async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        Console.WriteLine(
            $"Starting index reorganization for {_databaseName}");

        var sql = $@"
USE [{_databaseName}];

DECLARE @sql nvarchar(max) = N'';

SELECT @sql +=
    N'ALTER INDEX [' + i.name + N'] ON [' +
    s.name + N'].[' + t.name + N'] REORGANIZE;' +
    CHAR(13) + CHAR(10)
FROM sys.indexes i
INNER JOIN sys.tables t
    ON i.object_id = t.object_id
INNER JOIN sys.schemas s
    ON s.schema_id = t.schema_id
WHERE i.name IS NOT NULL
AND i.type_desc <> 'HEAP'
AND i.is_disabled = 0;

EXEC sp_executesql @sql;
";

        await using var command =
            new SqlCommand(sql, connection);

        command.CommandTimeout = 0;

        await command.ExecuteNonQueryAsync(
            cancellationToken);

        Console.WriteLine(
            $"Completed index reorganization for {_databaseName}");
    }
}