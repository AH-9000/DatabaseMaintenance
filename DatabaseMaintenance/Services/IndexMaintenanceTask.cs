using Microsoft.Data.SqlClient;

namespace DatabaseMaintenance.Services;

public class IndexMaintenanceTask : IDatabaseTask
{
    private readonly string _connectionString;
    private readonly string _databaseName;

    public IndexMaintenanceTask(
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
        
        DECLARE
            @SchemaName sysname,
            @TableName sysname,
            @IndexName sysname,
            @Fragmentation float,
            @Command nvarchar(max);
        
        DECLARE index_cursor CURSOR FOR
        
        SELECT
            s.name,
            t.name,
            i.name,
            ips.avg_fragmentation_in_percent
        FROM sys.dm_db_index_physical_stats
        (
            DB_ID(),
            NULL,
            NULL,
            NULL,
            'LIMITED'
        ) ips
        INNER JOIN sys.indexes i
            ON ips.object_id = i.object_id
           AND ips.index_id = i.index_id
        INNER JOIN sys.tables t
            ON t.object_id = i.object_id
        INNER JOIN sys.schemas s
            ON s.schema_id = t.schema_id
        WHERE
            ips.page_count > 1000
            AND i.name IS NOT NULL
            AND i.is_disabled = 0
            AND ips.avg_fragmentation_in_percent >= 5;
        
        OPEN index_cursor;
        
        FETCH NEXT FROM index_cursor
        INTO
            @SchemaName,
            @TableName,
            @IndexName,
            @Fragmentation;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
        
            IF @Fragmentation >= 30
            BEGIN
                SET @Command =
                   N'ALTER INDEX [' + @IndexName +
                   N'] ON [' + @SchemaName +
                   N'].[' + @TableName +
                   N'] REBUILD WITH (ONLINE = ON, SORT_IN_TEMPDB = ON, MAXDOP = 4)';
            END
            ELSE
            BEGIN
                SET @Command =
                    N'ALTER INDEX [' + @IndexName +
                    N'] ON [' + @SchemaName +
                    N'].[' + @TableName +
                    N'] REORGANIZE';
            END
        
            PRINT @Command;
        
            EXEC(@Command);
        
            FETCH NEXT FROM index_cursor
            INTO
                @SchemaName,
                @TableName,
                @IndexName,
                @Fragmentation;
        END
        
        CLOSE index_cursor;
        DEALLOCATE index_cursor;
        ";

        await using var command =
            new SqlCommand(sql, connection);

        command.CommandTimeout = 0;

        await command.ExecuteNonQueryAsync(
            cancellationToken);

        Console.WriteLine(
            $"Index maintenance completed for {_databaseName}");
    }
}