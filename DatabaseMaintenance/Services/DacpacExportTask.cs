using DatabaseMaintenance.Configuration;
using DatabaseMaintenance.Logging;
using System.Diagnostics;

namespace DatabaseMaintenance.Services;

public class DacpacExportTask : IDatabaseTask
{
    private readonly DatabaseSettings _settings;
    private readonly IAppLogger _logger;

    public DacpacExportTask(
        DatabaseSettings settings,
        IAppLogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        var sqlPackagePath = _settings.SqlPackagePath;

        if (!File.Exists(sqlPackagePath))
        {
            _logger.Warning(
                $"Configured SqlPackage.exe not found: {sqlPackagePath}");

            var commonPaths = new[]
            {
                @"C:\Program Files\Microsoft SQL Server\170\DAC\bin\SqlPackage.exe",
                @"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\SQLDB\DAC\SqlPackage.exe"
            };

            sqlPackagePath = commonPaths.FirstOrDefault(File.Exists);

            if (sqlPackagePath == null)
            {
                throw new FileNotFoundException(
                    "Could not locate SqlPackage.exe");
            }

            _logger.Warning(
                $"Using discovered SqlPackage.exe: {sqlPackagePath}");
        }

        if (!Directory.Exists(_settings.DacpacDirectory))
        {
            _logger.Information(
                $"Directory does not exist: {_settings.DacpacDirectory}");

            Console.Write("Create directory? (Y/N): ");

            var response = Console.ReadLine();

            if (string.Equals(
                response,
                "Y",
                StringComparison.OrdinalIgnoreCase))
            {
                Directory.CreateDirectory(
                    _settings.DacpacDirectory);

                _logger.Information(
                    $"Directory created: {_settings.DacpacDirectory}");
            }
            else
            {
                throw new DirectoryNotFoundException(
                    _settings.DacpacDirectory);
            }
        }

        var dacpacFile = Path.Combine(
            _settings.DacpacDirectory,
            $"{_settings.Database}_{DateTime.Now:yyyyMMdd_HHmmss}.dacpac");

        _logger.Information(
            $"Creating DACPAC: {dacpacFile}");

        using var process = new Process();

        process.StartInfo.FileName = sqlPackagePath;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.StartInfo.Arguments =
            $"/Action:Extract " +
            $"/SourceConnectionString:\"Server={_settings.Server};" +
            $"Database={_settings.Database};" +
            $"Integrated Security=True;" +
            $"TrustServerCertificate=True;" +
            $"Encrypt=False;\" " +
            $"/TargetFile:\"{dacpacFile}\"";

        _logger.Information(
            $"Executing: {process.StartInfo.FileName}");

        process.Start();

        await process.WaitForExitAsync(ct);

        var standardOutput =
            await process.StandardOutput.ReadToEndAsync(ct);

        var standardError =
            await process.StandardError.ReadToEndAsync(ct);

        if (!string.IsNullOrWhiteSpace(standardOutput))
        {
            _logger.Information(standardOutput);
        }

        if (process.ExitCode != 0)
        {
            _logger.Error(standardError);

            throw new Exception(
                $"DACPAC export failed. ExitCode={process.ExitCode}");
        }

        _logger.Information(
            $"DACPAC created: {dacpacFile}");
    }
}