using MediatR;
using MqttEventBus.Bus;
using MqttHub.Api.Commands;
using MqttHub.Api.Events;

namespace MqttHub.Api.CommandHandlers;

public class LogCommandHandler(IEventBus eventBus):IRequestHandler<LogCommand,bool>
{
    public Task<bool> Handle(LogCommand request, CancellationToken cancellationToken)
    {
        eventBus.Publish(new LogData(request.Message,request.PublishModel), "test");
        return Task.FromResult(true);
    }
}