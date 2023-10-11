using SmsClient.Dtos;

namespace SmsClient.Service
{
    public interface ISmsService
    {
        MessagePublishedDto SendMessage(MessagePublishedDto message);
    }
}
