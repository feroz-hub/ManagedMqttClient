using MqttEventBus.Commands;
using MqttHub.Api.Models;

namespace MqttHub.Api.Commands;

public class PublishCommand:Command
{
    public PublishModel PublishModel { get; protected set; }
    
}