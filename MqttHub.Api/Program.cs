using MediatR;
using MqttEventBus.Bus;
using MqttEventBus.MqttBus;
using MqttHub.Api.CommandHandlers;
using MqttHub.Api.Commands;
using MqttHub.Api.Service.Implementation;
using MqttHub.Api.Service.Interface;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IEventBus, EventBus>(sp =>
{
    var scopefactory = sp.GetRequiredService<IServiceScopeFactory>();
    return new EventBus(sp.GetService<IMediator>(), scopefactory,sp.GetService<IMqttClient>(),sp.GetService<IManagedMqttClient>());
});
builder.Services.AddSingleton(typeof(Dictionary<string, List<Type>>));
builder.Services.AddSingleton(typeof(List<Type>));
builder.Services.AddScoped<IPublishService, PublishService>();
builder.Services.AddMediatR(cfg=>cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddSingleton(typeof(Dictionary<string, List<Type>>));
builder.Services.AddSingleton(typeof(List<Type>));
builder.Services.AddScoped<IRequestHandler<LogCommand,bool>, LogCommandHandler>();

// builder.Services.AddSingleton<IMqttClient>(options => options.GetRequiredService<IMqttClient>());
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors();
app.MapControllers();

app.Run();

