using MqttEventBus.Bus;
using MqttEventBus.Events;
using MqttHub.Api.Commands;
using MqttHub.Api.Models;
using MqttHub.Api.Service.Interface;

namespace MqttHub.Api.Service.Implementation;

public class PublishService(IEventBus eventBus):IPublishService
{
    public void Publish(PublishModel model)
    {
        var createPublishCommand = new LogCommand("Hi from Event",model);

        eventBus.SendCommand(createPublishCommand);
    }
}