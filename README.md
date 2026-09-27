# DatabaseMaintenance

A .NET 10.0 console application for SQL Server database maintenance tasks including backup, Dacpac export, and index maintenance.

## Features

- **Database Backup**: Creates full database backups to a specified directory
- **Dacpac Export**: Exports database schema as a Dacpac package
- **Index Maintenance**: Rebuilds, reorganizes indexes and updates statistics

## Prerequisites

- .NET 10.0 SDK or later
- Microsoft SQL Server
- SQLPackage.exe (included with SQL Server Data Tools)

## Configuration

Edit `DatabaseMaintenance/appsettings.json` to configure your database settings:

```json
{
  "Database": {
    "Server": "SQL01",
    "Database": "MyDatabase",
    "BackupDirectory": "E:\\SqlBackups",
    "DacpacDirectory": "E:\\Dacpacs",
    "SqlPackagePath": "C:\\Program Files\\Microsoft SQL Server\\170\\DAC\\bin\\SqlPackage.exe",
    "PerformIndexMaintenance": true
  }
}
```

### Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `Server` | SQL Server instance name | `SQL01` |
| `Database` | Database name to maintain | `MyDatabase` |
| `BackupDirectory` | Path to store backup files | `E:\\SqlBackups` |
| `DacpacDirectory` | Path to store Dacpac files | `E:\\Dacpacs` |
| `SqlPackagePath` | Path to SqlPackage.exe | `C:\\Program Files\\Microsoft SQL Server\\170\\DAC\\bin\\SqlPackage.exe` |
| `PerformIndexMaintenance` | Enable/disable index maintenance | `true` |

## Usage

### Running the Application

```bash
cd DatabaseMaintenance
 dotnet run
```

Or run the compiled executable:

```bash
DatabaseMaintenance\bin\Release\net10.0\DatabaseMaintenance.exe
```

### Build

```bash
cd DatabaseMaintenance
dotnet build -c Release
```

## Project Structure

```
DatabaseMaintenance/
├── DatabaseMaintenance/
│   ├── Program.cs              # Entry point
│   ├── appsettings.json        # Configuration file
│   ├── Configuration/
│   │   └── AppSettings.cs      # Configuration model
│   ├── Services/
│   │   ├── IDatabaseTask.cs    # Task interface
│   │   ├── BackupTask.cs       # Database backup task
│   │   ├── DacpacExportTask.cs # Dacpac export task
│   │   ├── IndexMaintenanceTask.cs # Index maintenance orchestrator
│   │   ├── RebuildIndexTask.cs # Index rebuild task
│   │   ├── ReorganizeIndexTask.cs # Index reorganize task
│   │   ├── UpdateStatisticsTask.cs # Statistics update task
│   │   └── SqlServerConnectionFactory.cs # Connection factory
│   ├── Logging/
│   │   └── ILogger.cs          # Logging interface
│   └── DatabaseMaintenance.csproj
├── DatabaseMaintenance.slnx
└── README.md
```

## Tasks

### Backup Task

Creates a full backup of the specified database using SQL Server native backup with the current timestamp in the filename.

### Dacpac Export Task

Exports the database schema as a Dacpac package using SqlPackage.exe. Requires SQLPackage.exe to be installed.

### Index Maintenance Task

Performs the following maintenance operations:
- **Rebuild Indexes**: Rebuilds indexes with high fragmentation (>30%)
- **Reorganize Indexes**: Reorganizes indexes with moderate fragmentation (10-30%)
- **Update Statistics**: Updates all database statistics

## Dependencies

- [Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/) 7.1.0
- [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration/) 10.0.12
- [Microsoft.Extensions.Configuration.Binder](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Binder/) 10.0.12
- [Microsoft.Extensions.Configuration.Json](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json/) 10.0.12

## Notes

- The application uses Windows Authentication (Integrated Security) to connect to SQL Server
- Backup files are created with `.bak` extension
- Dacpac files are created with `.dacpac` extension
- Index maintenance can be disabled by setting `PerformIndexMaintenance` to `false`

## License

This project is proprietary software.
