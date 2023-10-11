namespace SmsMicroservice.Logger.LoggerClient
{
    /// <summary>
    /// Actual implementation goes here
    /// </summary>
    public class CustomLogger : ILoggerLibrary
    {
        public void LogError(string message)
        {
            Console.WriteLine($"[Error]: {message}");
        }

        public void LogInfo(string message)
        {
            Console.WriteLine($"[Info]: {message}");
        }

        public void LogWarning(string message)
        {
            Console.WriteLine($"[Warn]: {message}");
        }
    }
}
