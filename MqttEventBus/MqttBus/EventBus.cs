using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MqttEventBus.Bus;
using MqttEventBus.Commands;
using MqttEventBus.Events;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using Newtonsoft.Json;

namespace MqttEventBus.MqttBus;

public class EventBus:IEventBus
{
   
    private readonly IMediator _mediator;
    private readonly Dictionary<string, List<Type>> _handlers;
    private readonly List<Type> _eventTypes;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly MqttFactory _mqttFactory;
    private IMqttClient _mqttClient;
    private IManagedMqttClient _managedMqttClient;
    
    public EventBus(IMediator mediator, IServiceScopeFactory scopeFactory,IMqttClient mqttClient,IManagedMqttClient managedMqttClient )
    {
        _mediator = mediator;
        _scopeFactory = scopeFactory;
        _mqttFactory = new MqttFactory();
        _mqttClient = mqttClient;
        _managedMqttClient=managedMqttClient;
        _eventTypes = new List<Type>();
        _handlers = new Dictionary<string, List<Type>>();

    }
    public Task SendCommand<T>(T command) where T : Command
    {
        return _mediator.Send(command);
    }

    public async Task Publish<T>(T @event, string topic) where T : Event
    {
        var eventName = @event.GetType().Name;
        var message = JsonConvert.SerializeObject(@event);
        ConnectMqttClient();
        _mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
            .WithTopic(eventName)
            .WithPayload(message)
            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag()
            .Build()).Wait();

    }
    
    public async Task ManagedMqttClientPublish<T>(T @event, string topic) where T : Event
    {
        var eventName = @event.GetType().Name;
        var message =Encoding.UTF8.GetBytes( JsonConvert.SerializeObject(@event));
 
        if (ConntectManagedMqttClient() && _managedMqttClient != null)
        {
            _managedMqttClient.EnqueueAsync(new MqttApplicationMessage()
            {
                Topic = eventName,
                Payload = message,
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                Retain=true
 
            }).Start();
 
            _managedMqttClient.StopAsync();
            SpinWait.SpinUntil(() => _managedMqttClient.PendingApplicationMessagesCount == 0, 10000);
 
            Console.Out.WriteLine($"Pending messages = {_managedMqttClient.PendingApplicationMessagesCount}");
        }
    }

    public void Subscribe<T, TH>(IEnumerable<string> topics) where T : Event where TH : IEventHandler<T>
    {
        var subscriptionOptions = _mqttFactory.CreateSubscribeOptionsBuilder();
        ConntectManagedMqttClient();
        //subscriptionOptions.WithTopicFilter(f => f.WithTopic(topic));

            List<MqttTopicFilter> mqttTopicFilters =
                topics.Select(topic => new MqttTopicFilterBuilder().WithTopic(topic).Build()).ToList();
            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                await ProcessEvent(e.ApplicationMessage.Topic, payload);

            };

            _managedMqttClient.SubscribeAsync(mqttTopicFilters);



    }

    public void  Subscribe<T, TH>(string topic) where T : Event where TH : IEventHandler<T>
    {
        var eventName = typeof(T).Name;

        ConnectMqttClient();

        _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            if (e.ApplicationMessage.Topic == eventName)
            {
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                await ProcessEvent(eventName, payload);
            }
        };
       
    }
    
    
    private void ConnectMqttClient()
    {
        if (_mqttClient == null || !_mqttClient.IsConnected)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost",1883)
                .Build();

            var managedOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(options)
                .Build();

            _mqttClient = _mqttFactory.CreateMqttClient();
            _mqttClient.ConnectAsync(options).Wait();
        }
    }
    
    private async Task ProcessEvent(string eventName, string message)
    {
        using var scope = _scopeFactory.CreateScope();
        var eventType = _mediator.GetType();

        if (eventType != null)
        {
            var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
            var handler = scope.ServiceProvider.GetService(concreteType);

            if (handler != null)
            {
                var @event = JsonConvert.DeserializeObject(message, eventType);
                await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
            }
        }
    }
    
    
    private bool ConntectManagedMqttClient()

    {

        bool result = false;

        try

        {

            if (_managedMqttClient == null || !_managedMqttClient.IsConnected)

            {

                var managedClientOption = new MqttClientOptionsBuilder()

                    .WithClientId("AdminManagedMqttClient")

                    .WithTcpServer("localhost", 1883)

                    .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)

                    //.WithNoKeepAlive()

                    .WithCleanSession(true)

                    // .WithCleanStart(true)

                    .Build();
 
                _managedMqttClient = _mqttFactory.CreateManagedMqttClient();
 
                var managedmqttClientOption = new ManagedMqttClientOptionsBuilder()

                    .WithClientOptions(managedClientOption)

                    .WithMaxPendingMessages(10)

                    //.WithPendingMessagesOverflowStrategy(MqttPendingMessagesOverflowStrategy.DropOldestQueuedMessage)

                    .WithAutoReconnectDelay (TimeSpan.FromSeconds(1))

                    .Build();

                _managedMqttClient.StartAsync(managedmqttClientOption);

                result = true;

            }

        }

        catch (Exception ex)

        {

            result= false;

        }

        return result;
 
    }
}