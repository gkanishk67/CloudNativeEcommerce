using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OrderService.DTOs;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace OrderService.Services
{
    public class RabbitMqProducer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMqProducer> _logger;

        public RabbitMqProducer(
            IConfiguration configuration,
            ILogger<RabbitMqProducer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void PublishOrderCreated(OrderCreatedEvent orderEvent)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"]
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var queueName = _configuration["RabbitMQ:Queue"]!;

            channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var json = JsonSerializer.Serialize(orderEvent);

            var body = Encoding.UTF8.GetBytes(json);

            var props = channel.CreateBasicProperties();

            var propagator = Propagators.DefaultTextMapPropagator;

            propagator.Inject(
                new PropagationContext(
                    Activity.Current!.Context,
                    Baggage.Current),
                props,
                (p, key, value) =>
                {
                    p.Headers ??= new Dictionary<string, object>();
                    p.Headers[key] = Encoding.UTF8.GetBytes(value);
                });

            channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: props,
                body: body);

            _logger.LogInformation(
                "Published order created event for product {ProductName}",
                orderEvent.ProductName);
        }
    }
}