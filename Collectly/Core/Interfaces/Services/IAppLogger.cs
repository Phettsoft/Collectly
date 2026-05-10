namespace Collectly.Core.Interfaces.Services;

public interface IAppLogger
{
    void Info(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
    void Debug(string message);
}
