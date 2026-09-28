using DatabaseMaintenance.Configuration;
using DatabaseMaintenance.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using DatabaseMaintenance.Logging;


const string appSettingsFile = "appsettings.json";
IAppLogger logger = new ConsoleLogger();
var fullPath = Path.GetFullPath(appSettingsFile);
var fileInfo = new FileInfo(fullPath);

if (!File.Exists(appSettingsFile))
{
    var defaultSettings = new AppSettings
    {
        Database = new DatabaseSettings
        {
            Server = "SERVERNAME",
            Database = "SQL01",
            BackupDirectory = @"E:\SqlBackups",
            DacpacDirectory = @"E:\SqlBackups",
            SqlPackagePath = @"C:\Program Files\Microsoft SQL Server\170\DAC\bin\SqlPackage.exe",
            PerformIndexMaintenance = false
        }
    };

    var json = JsonSerializer.Serialize(
        defaultSettings,
        new JsonSerializerOptions
        {
            WriteIndented = true
        });

    await File.WriteAllTextAsync(appSettingsFile, json);
    logger.Warning(
        $"This file was missing: '{appSettingsFile}'.");

    logger.Error(
        "Please configure the SQL Server settings in appsettings.json and run this program again.");
    return;
}

var configuration = new ConfigurationBuilder()
    .AddJsonFile(appSettingsFile)
    .Build();

var settings = configuration.Get<AppSettings>()!;

var connectionString =
    $"Server={settings.Database.Server};" +
    "Integrated Security=True;" +
    "TrustServerCertificate=True;" +
    "Database=master;";
logger.Information($"Starting backup...");
logger.Information($"Connection string - {connectionString}");
logger.Information($"Server={settings.Database.Server}");
logger.Information($"Database={settings.Database.Database}");
logger.Information($"BackupDirectory={settings.Database.BackupDirectory}");
try
{
    IDatabaseTask task =
        new BackupTask(
            connectionString,
            settings.Database.Database,
            settings.Database.BackupDirectory,
            logger);

    await task.ExecuteAsync(CancellationToken.None);
}
catch (Exception ex)
{
    logger.Error($"This exception occured in BackupTask {ex.Message}");
}

try
{
    IDatabaseTask taskDP =
        new DacpacExportTask(settings.Database, logger);

    await taskDP.ExecuteAsync(CancellationToken.None);
}
catch (Exception ex)
{
    logger.Warning($"This exception occured in DacpacExportTask {ex.Message}");
}

if (settings.Database.PerformIndexMaintenance)
{
    IDatabaseTask taskIndex =
        new IndexMaintenanceTask(
            connectionString,
            settings.Database.Database);

    await taskIndex.ExecuteAsync(
        CancellationToken.None);
}

Console.WriteLine("Press any key to continue");
Console.ReadLine();