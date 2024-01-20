using MqttEventBus.Events;
using MqttHub.Api.Models;

namespace MqttHub.Api.Events;

public class LogData:Event
{
    public PublishModel PublishModel { get; protected set; }
    public string Message{get; protected set; }

    public LogData(string message, PublishModel publishModel)
    {
        Message=message;
        PublishModel = publishModel;
    }
    
}