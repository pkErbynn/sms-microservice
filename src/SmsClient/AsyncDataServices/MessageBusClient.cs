using RabbitMQ.Client;
using SmsClient.Dtos;
using SmsClient.Enums;
using System.Text;
using System.Text.Json;

namespace SmsClient.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string EXCHANGE_NAME = "sms-trigger";
        private const string ROUTING_KEY = "sms-routing-key";
        private const string QUEUE_NAME = "sms-queue";
        private const string CLIENT_NAME = "sms-sender";

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(_configuration["RabbitMQ:Url"]),
            };

            try
            {
                factory.ClientProvidedName = CLIENT_NAME;
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: EXCHANGE_NAME, type: ExchangeType.Direct);
                _channel.QueueDeclare(QUEUE_NAME, false, false, false, null);
                _channel.QueueBind(QUEUE_NAME, EXCHANGE_NAME, ROUTING_KEY, null);
                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("--> Connected to MessageBus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
            }
        }

        public void PublishNewSmsMessage(MessagePublishedDto messagePublishedDto)
        {
            messagePublishedDto.Command = MessageCommands.SendSms.ToString();
            var message = JsonSerializer.Serialize(messagePublishedDto);
            if(_connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMQ Connection Open, sending sms message...");
                SendMessage(message);
            }
            else
            {
                Console.WriteLine("--> RabbitMQ connection is closed, not sending");
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(
                    exchange: EXCHANGE_NAME,
                    routingKey: ROUTING_KEY,
                    basicProperties: null,
                    body: body);
            Console.WriteLine($"--> We have sent {message}");
        }

        private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs eventArgs)
        {
            Console.WriteLine("--> Shutting down MQTT");
        }

        public void Dispose()
        {
            Console.WriteLine("MessageBus Disposed");
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }

    }
}
