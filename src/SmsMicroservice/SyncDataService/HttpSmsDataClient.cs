using SmsMicroservice.Models;
using System.Text.Json;
using System.Text;

namespace SmsMicroservice.SyncDataService
{
    /// <summary>
    /// The HttpSmsDataClient class is responsible for sending SMS messages to a 3rd-party SMS service via HTTP requests. 
    /// It provides a reliable mechanism for delivering SMS messages within the SMS Microservice.
    /// </summary>
    public class HttpSmsDataClient : IHttpSmsDataClient
    {
        private readonly HttpClient _httpClient;
        private readonly SmsHttpOption  _smsHttpOption;

        /// <summary>
        /// Sends SMS messages using HTTP POST requests.
        /// Implements a retry mechanism for reliable SMS delivery.
        /// Supports dynamic configuration via SmsHttpOption.
        /// Handles exceptions and logs errors when SMS sending fails.
        /// </summary>
        /// <param name="httpClient">Used for making HTTP requests.</param>
        /// <param name="configuration">Provides access to configuration settings. Reads configuration settings such as API URL, API key, and retry parameters.</param>
        public HttpSmsDataClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _smsHttpOption = new SmsHttpOption
            {
                Url = configuration["ThirdPartyAPI:Url"],
                ApiKey = configuration["ThirdPartyAPI:ApiKey"],
                DelayBetweenRetriesMilliseconds = Convert.ToInt32(configuration["ThirdPartyAPI:DelayBetweenRetriesMilliseconds"]),
                MaxRetryAttempts = Convert.ToInt32(configuration["ThirdPartyAPI:MaxRetryAttempts"]),
            };
        }

        public async Task<bool> SendSmsToThirdPartyAsync(SmsTextMessage smsTextMessage)
        {

            int retryCont = 0;
            while (retryCont < _smsHttpOption.MaxRetryAttempts)
            {
                try
                {
                    if (smsTextMessage == null)
                        throw new ArgumentNullException();

                    var httpContent = new StringContent(
                                JsonSerializer.Serialize(smsTextMessage),
                                Encoding.UTF8,
                                "application/json");
                    _httpClient.DefaultRequestHeaders.Add("ApiKey", _smsHttpOption.ApiKey);

                    var response = await _httpClient.PostAsync(_smsHttpOption.Url, httpContent);
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occured sending SMS: {ex.Message}");
                }

                retryCont++;
                if (retryCont < _smsHttpOption.MaxRetryAttempts)
                {
                    await Task.Delay(_smsHttpOption.DelayBetweenRetriesMilliseconds);
                }
            }
            return false;
        }
    }
}
