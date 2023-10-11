namespace SmsMicroservice.Logger
{
    public interface ILoggerLibrary
    {
        void LogError(string message);
        void LogInfo(string message);
        void LogWarning(string message);
    }
}
