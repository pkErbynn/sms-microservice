using SmsMicroservice.Models;

namespace SmsMicroservice.EventBus
{
    /// <summary>
    /// Actual Event Bus implementation goes here
    /// </summary>
    public class EventBus : IEventBus
    {
        public void Publish(SmsSentEvent @event)
        {
            Console.WriteLine($"--> Publishing to event bus: {@event}...");
        }
    }
}
