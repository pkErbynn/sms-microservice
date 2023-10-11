using SmsMicroservice.Models;

namespace SmsMicroservice.EventBus
{
    public interface IEventBus
    {
        void Publish(SmsSentEvent @event);
    }
}