using MqttHub.Api.Models;

namespace MqttHub.Api.Commands;

public class LogCommand:PublishCommand
{
    public string Message {get;protected set;}
    public LogCommand(string message,PublishModel publishMod)
    {
        PublishModel = publishMod;
        Message = message;
    }
}