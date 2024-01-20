using MqttHub.Api.Models;

namespace MqttHub.Api.Service.Interface;

public interface IPublishService
{
    void Publish(PublishModel model);
}