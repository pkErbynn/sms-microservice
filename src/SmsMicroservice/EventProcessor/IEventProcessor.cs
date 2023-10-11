namespace SmsMicroservice.EventProcessor
{
    public interface IEventProcessor
    {
        void ProcessCommand(string message);
    }
}