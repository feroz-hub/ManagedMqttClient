using Microsoft.AspNetCore.Mvc;
using MqttHub.Api.Models;
using MqttHub.Api.Service.Interface;

namespace MqttHub.Api.Controllers;
[Route("api/mqtt")]
[ApiController]

public class MqttController(IPublishService publishService): ControllerBase
{
    
    [HttpPost("PublishRequest")]
    public async Task<IActionResult> PublishRequest([FromBody] PublishModel publishModel)
    {
        publishService.Publish(publishModel);
        return Ok();
    }
}