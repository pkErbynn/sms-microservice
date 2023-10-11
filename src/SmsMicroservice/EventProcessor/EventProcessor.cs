using SmsMicroservice.Dto;
using SmsMicroservice.EventProcessor.EventTypeEnum;
using SmsMicroservice.Messenger;
using System.Text.Json;

namespace SmsMicroservice.EventProcessor
{
    /// <summary>
    /// Event processor processes the command that is fetched from the message queue and direct it to the Messenger processor
    /// </summary>
    public class EventProcessor : IEventProcessor
    {
        IServiceScopeFactory _scopeFactory;

        public EventProcessor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void ProcessCommand(string message)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var messageSender = scope.ServiceProvider.GetRequiredService<ISmsService>();

                MessageRecievedDto receivedMessage =  DeserializeMessage(message);
                CommandType commandType = DetermineCommand(receivedMessage);
                switch (commandType)
                {
                    case CommandType.SendSms:
                        messageSender.SendSmsMessage(receivedMessage);
                        break;
                    default:
                        break;
                }
            }
        }

        private CommandType DetermineCommand(MessageRecievedDto receivedMessage)
        {
            switch (receivedMessage.Command)
            {
                case "SendSms":
                    Console.WriteLine("--> SmsSend Event Detected");
                    return CommandType.SendSms;
                default:
                    Console.WriteLine("--> Could not determine the event type");
                    return CommandType.Undetermined;
            }
        }

        private Func<string, MessageRecievedDto> DeserializeMessage = message => JsonSerializer.Deserialize<MessageRecievedDto>(message);
        }
    }

