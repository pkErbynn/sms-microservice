using SmsMicroservice.Dto;

namespace SmsMicroservice.Messenger
{
    public interface ISmsService
    {
        Task SendSmsMessage(MessageRecievedDto recievedMessage);
    }
}
