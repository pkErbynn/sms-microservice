namespace SmsMicroservice.AsyncDataService
{
    public interface IMessageQueue
    {
        void Connect();
        void Dispose();
        void Subscribe(string queueName, Action<string> messageHandler);
    }
}
