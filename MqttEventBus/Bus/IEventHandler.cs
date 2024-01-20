using MqttEventBus.Events;

namespace MqttEventBus.Bus;

public interface IEventHandler<in TEvent>:IEventHandler where TEvent : Event
{
    Task Handle(TEvent @event);
}

public interface IEventHandler
{
    
}