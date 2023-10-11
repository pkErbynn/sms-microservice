using SmsMicroservice.EventProcessor;

namespace SmsMicroservice.AsyncDataService
{
    /// <summary>
    /// Designed to interact with a generic message queue, specifically RabbitMQ, to process messages asynchronously.
    /// Used to listen to a message queue, receive messages, and process them asynchronously. It acts as a consumer to handle those messages.
    /// </summary>
    public class MessageQueueListener : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;
        private readonly IMessageQueue _messageQueueClient;
        private readonly string _queueName;

        public MessageQueueListener(IConfiguration configuration, IEventProcessor eventProcessor, IMessageQueue messageQueueConnection)
        {
            _configuration = configuration;
            _queueName = _configuration["RabbitMQ:QueueName"];

            _eventProcessor = eventProcessor;
            _messageQueueClient = messageQueueConnection; // Injected implementation

            _messageQueueClient.Connect();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                stoppingToken.ThrowIfCancellationRequested();

                _messageQueueClient.Subscribe(_queueName, HandleMessage);
                Console.WriteLine($"--> Subscribed to the Message Queue: {_queueName}");

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Can not listen to the Message Queue: {ex.Message}");
                return Task.CompletedTask;
            }
        }

        private void HandleMessage(string message)
        {
            try
            {
                Console.WriteLine("--> Command Received!");

                _eventProcessor.ProcessCommand(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Error handling message: {ex.Message}");
            }
        }

        public override void Dispose()
        {
            _messageQueueClient?.Dispose();
            base.Dispose();
        }
    }
}
