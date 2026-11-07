using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MicroRabbit.Infra.Bus
{
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly Dictionary<string, Type> _eventTypes;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly RabbitMQSettings _settings;

        // Keep connections and channels alive while consumers are running
        private readonly List<IConnection> _connections = new();
        private readonly List<IChannel> _channels = new();

        public RabbitMQBus(
            IMediator mediator,
            IServiceScopeFactory serviceScopeFactory,
            IOptions<RabbitMQSettings> options)
        {
            _mediator = mediator;
            _serviceScopeFactory = serviceScopeFactory;
            _settings = options.Value;

            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new Dictionary<string, Type>();
        }

        private ConnectionFactory CreateConnectionFactory()
        {
            return new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };
        }

        // Send Command through MediatR
        public Task SendCommand<T>(T command)
            where T : Command
        {
            return _mediator.Send(command);
        }

        // Publish Event to RabbitMQ
        public async Task Publish<T>(T @event)
            where T : Event
        {
            var factory = CreateConnectionFactory();

            await using var connection =
                await factory.CreateConnectionAsync();

            await using var channel =
                await connection.CreateChannelAsync();

            var eventName = @event.GetType().Name;

            await channel.QueueDeclareAsync(
                queue: eventName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: eventName,
                body: body);
        }

        // Register Event Handler
        public async Task Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            if (!_eventTypes.ContainsKey(eventName))
            {
                _eventTypes.Add(eventName, typeof(T));
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            // Check if handler is already registered
            if (_handlers[eventName].Contains(handlerType))
            {
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} already registered for '{eventName}'");
            }

            _handlers[eventName].Add(handlerType);

            await StartBasicConsume<T>();
        }

        // Start RabbitMQ Consumer
        private async Task StartBasicConsume<T>()
            where T : Event
        {
            var factory = CreateConnectionFactory();

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            var eventName = typeof(T).Name;

            await channel.QueueDeclareAsync(
                queue: eventName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (sender, ea) =>
            {
                await Consumer_Received(channel, ea);
            };

            await channel.BasicConsumeAsync(
                queue: eventName,
                autoAck: false,
                consumer: consumer);

            // Keep references alive
            _connections.Add(connection);
            _channels.Add(channel);
        }

        // Receive Message from RabbitMQ
        private async Task Consumer_Received(
            IChannel channel,
            BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;

            var message = Encoding.UTF8.GetString(
                e.Body.ToArray());

            try
            {
                await ProcessEvent(eventName, message);

                // Acknowledge message after successful processing
                await channel.BasicAckAsync(
                    deliveryTag: e.DeliveryTag,
                    multiple: false);
            }
            catch (Exception)
            {
                // Reject and requeue message if processing fails
                await channel.BasicNackAsync(
                    deliveryTag: e.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        }

        // Execute Registered Handlers
        private async Task ProcessEvent(
            string eventName,
            string message)
        {
            if (!_handlers.ContainsKey(eventName))
                return;

            using var scope = _serviceScopeFactory.CreateScope();

            var subscriptions = _handlers[eventName];

            foreach (var subscription in subscriptions)
            {
                var handler = scope.ServiceProvider
                    .GetRequiredService(subscription);

                var eventType = _eventTypes[eventName];

                var @event = JsonSerializer.Deserialize(
                    message,
                    eventType);

                if (@event == null)
                    throw new InvalidOperationException(
                        $"Could not deserialize event: {eventName}");

                var handlerInterface = typeof(IEventHandler<>)
                    .MakeGenericType(eventType);

                var handleMethod = handlerInterface.GetMethod("Handle");

                var task = (Task)handleMethod!.Invoke(
                    handler,
                    new object[] { @event })!;

                await task;
            }
        }

     
    }
}
