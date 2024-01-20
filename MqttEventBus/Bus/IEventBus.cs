using MqttEventBus.Commands;
using MqttEventBus.Events;

namespace MqttEventBus.Bus;

public interface IEventBus
{
    Task SendCommand<T>(T command) where T : Command;

    Task Publish<T>(T @event, string topic) where T : Event;
    
    Task ManagedMqttClientPublish<T>(T @event, string topic) where T : Event;  

    void Subscribe<T, TH>(IEnumerable<string> topics) where T : Event where TH : IEventHandler<T>;
    void Subscribe<T, TH>(string topic)
        where T : Event
        where TH : IEventHandler<T>;
}