using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SmsMicroservice.AsyncDataService.MessageQueueClient
{
    /// <summary>
    /// Concrete implementation using RabbitMq
    /// </summary>
    public class RabbitMessageQueue : IMessageQueue
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;
        private string _exchangeName;
        private string _routingKey;

        public RabbitMessageQueue(IConfiguration configuration)
        {
            _configuration = configuration;
            _exchangeName = _configuration["RabbitMQ:ExchangeName"];
            _routingKey = _configuration["RabbitMQ:RoutingKey"];
        }

        public void Connect()
        {
            string uriString = _configuration["RabbitMQ:Url"];
            string clientName = _configuration["RabbitMQ:ClientName"];

            var factory = new ConnectionFactory()
            {
                Uri = new Uri(uriString),
            };

            try
            {
                factory.ClientProvidedName = clientName;
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);

                Console.WriteLine("--> Connected to MessageBus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
            }
        }

        public void Subscribe(string queueName, Action<string> messageHandler)
        {
            try
            {
                _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueBind(queue: queueName, exchange: _exchangeName, routingKey: _routingKey, arguments: null);
                var consumer = new EventingBasicConsumer(_channel);

                consumer.Received += (sender, eventArgs) =>
                {
                    Console.WriteLine("--> Event Received!");
                    var body = eventArgs.Body;
                    var message = Encoding.UTF8.GetString(body.ToArray());
                    messageHandler(message);
                };

                // Start consuming messages from the queue
                _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Error subscribing to RabbitMQ queue: {ex.Message}");
                // Handle the subscription error as needed
            }
        }

        public void Dispose()
        {
            if (_channel != null && _channel.IsOpen)
            {
                _channel.Close();
            }

            if (_connection != null && _connection.IsOpen)
            {
                _connection.Close();
            }

        }
    }
}
