namespace SmsMicroservice.SyncDataService
{
    /// <summary>
    ///  Stores configuration settings such as API URL, API key, and retry parameters.
    /// </summary>
    public class SmsHttpOption
    {
        public string Url { get; set; }
        public int DelayBetweenRetriesMilliseconds { get; set; }
        public int MaxRetryAttempts { get; set; }
        public string ApiKey { get; set; }
    }
}