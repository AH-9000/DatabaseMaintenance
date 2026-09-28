using Microsoft.Data.SqlClient;

namespace DatabaseMaintenance.Services;

public class RebuildIndexTask : IDatabaseTask
{
    private readonly string _connectionString;
    private readonly string _databaseName;

    public RebuildIndexTask(
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
            $"Starting index maintenance for {_databaseName}");

        var sql = $@"
        USE [{_databaseName}];
        
        DECLARE @sql nvarchar(max) = '';
        
        SELECT @sql +=
            'ALTER INDEX [' + i.name + '] ON [' +
            s.name + '].[' + t.name +
            '] REORGANIZE;' + CHAR(13)
        FROM sys.indexes i
        INNER JOIN sys.tables t
            ON i.object_id = t.object_id
        INNER JOIN sys.schemas s
            ON t.schema_id = s.schema_id
        WHERE i.name IS NOT NULL
        AND i.type_desc <> 'HEAP';
        
        EXEC sp_executesql @sql;
        ";

        await using var command =
            new SqlCommand(sql, connection);

        command.CommandTimeout = 0;

        await command.ExecuteNonQueryAsync(cancellationToken);

        Console.WriteLine(
            $"Index maintenance completed for {_databaseName}");
    }
}