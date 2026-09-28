namespace DatabaseMaintenance.Configuration;

public class AppSettings
{
    public DatabaseSettings Database { get; set; } = new();
}

public class DatabaseSettings
{
    public string Server { get; set; } = "";
    public string Database { get; set; } = "";
    public string BackupDirectory { get; set; } = "";

    public string DacpacDirectory { get; set; } = "";
    public string SqlPackagePath { get; set; } = "";
    public bool PerformIndexMaintenance { get; set; } = true;

}