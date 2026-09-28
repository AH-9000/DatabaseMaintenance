namespace DatabaseMaintenance.Logging;

public interface IAppLogger
{
    void Information(string message);
    void Error(string message);
    void Warning(string message);
}