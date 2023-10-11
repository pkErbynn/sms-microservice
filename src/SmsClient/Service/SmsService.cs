using SmsClient.AsyncDataServices;
using SmsClient.Dtos;
using SmsClient.Enums;
using SmsClient.Models;

namespace SmsClient.Service
{
    public class SmsService : ISmsService
    {
        private readonly IMessageBusClient _messageBusClient;

        public SmsService(IMessageBusClient messageBusClient)
        {
            _messageBusClient= messageBusClient;
        }
        public MessagePublishedDto SendMessage(MessagePublishedDto message)
        {
            try
            {
                message.Command = MessageCommands.SendSms.ToString();
                message.Id = Guid.NewGuid();
                _messageBusClient.PublishNewSmsMessage(message);
                return message;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send asynchronously: {ex.Message}");
                return message;

            }
        }
    }
}
