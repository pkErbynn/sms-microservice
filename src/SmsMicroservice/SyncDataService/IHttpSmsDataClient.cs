using SmsMicroservice.Models;

namespace SmsMicroservice.SyncDataService
{
    public interface IHttpSmsDataClient
    {
        Task<bool> SendSmsToThirdPartyAsync(SmsTextMessage smsTextMessage);
    }
}
