using SmsClient.Dtos;

namespace SmsClient.AsyncDataServices
{
    public interface IMessageBusClient
    {
        void PublishNewSmsMessage(MessagePublishedDto messagePublishedDto);
    }
}
