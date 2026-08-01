using Microsoft.Extensions.Hosting;
using ProductService.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace ProductService.Services
{
    public class OrderCreatedConsumer : BackgroundService
    {
        private readonly IProductDataService _productDataService;
        private readonly ILogger<OrderCreatedConsumer> _logger;

        private static readonly ActivitySource ActivitySource =
            new("ProductService");

        private static readonly TextMapPropagator Propagator =
            Propagators.DefaultTextMapPropagator;

        public OrderCreatedConsumer(
            IProductDataService productDataService,
            ILogger<OrderCreatedConsumer> logger)
        {
            _productDataService = productDataService;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq"
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: "order-created",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                Console.WriteLine("========== RABBIT MESSAGE ==========");

                Console.WriteLine($"Headers Count = {ea.BasicProperties.Headers?.Count}");

                if (ea.BasicProperties.Headers != null)
                {
                    foreach (var header in ea.BasicProperties.Headers)
                    {
                        if (header.Value is byte[] bytes)
                        {
                            Console.WriteLine($"{header.Key} = {Encoding.UTF8.GetString(bytes)}");
                        }
                    }
                }

                var parentContext = Propagator.Extract(
                    default,
                    ea.BasicProperties,
                    (props, key) =>
                    {
                        if (props.Headers != null &&
                            props.Headers.TryGetValue(key, out var value))
                        {
                            if (value is byte[] bytes)
                            {
                                return new[]
                                {
                                    Encoding.UTF8.GetString(bytes)
                                };
                            }
                        }

                        return Enumerable.Empty<string>();
                    });

                Console.WriteLine($"Parent TraceId = {parentContext.ActivityContext.TraceId}");
                Console.WriteLine($"Parent SpanId  = {parentContext.ActivityContext.SpanId}");
                Console.WriteLine($"IsRemote       = {parentContext.ActivityContext.IsRemote}");

                Baggage.Current = parentContext.Baggage;

                using var activity = ActivitySource.StartActivity(
                    "RabbitMQ Consume",
                    ActivityKind.Consumer,
                    parentContext.ActivityContext);

                Console.WriteLine("========== CONSUMER ACTIVITY ==========");
                Console.WriteLine($"Activity = {activity?.Id}");
                Console.WriteLine($"Recorded = {activity?.Recorded}");
                Console.WriteLine($"TraceId  = {activity?.TraceId}");
                Console.WriteLine($"ParentId = {activity?.ParentId}");

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var orderEvent =
                    JsonSerializer.Deserialize<OrderCreatedEvent>(message);

                if (orderEvent != null)
                {
                    var product =
                        await _productDataService.GetByNameAsync(orderEvent.ProductName);

                    if (product != null)
                    {
                        product.Stock -= orderEvent.Quantity;

                        await _productDataService.UpdateAsync(product);

                        _logger.LogInformation(
                            "Stock updated for product {ProductName}. Remaining stock: {Stock}",
                            product.Name,
                            product.Stock);
                    }
                }
            };

            channel.BasicConsume(
                queue: "order-created",
                autoAck: true,
                consumer: consumer);

            return Task.CompletedTask;
        }
    }
}