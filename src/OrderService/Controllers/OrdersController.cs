using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using OrderService.DTOs;
using OrderService.Services;
using System.Diagnostics;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly RabbitMqProducer _producer;
        private readonly ILogger<OrdersController> _logger;
        private static readonly ActivitySource ActivitySource = new("OrderService");

        public OrdersController(RabbitMqProducer producer, ILogger<OrdersController> logger)
        {
            _producer = producer;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult CreateOrder(CreateOrderRequest request)
        {
            using var activity = ActivitySource.StartActivity("Create Order");

            activity?.SetTag("order.product_name", request.ProductName);
            activity?.SetTag("order.quantity", request.Quantity);

            try
            {
                // Validation
                using (var validation =
                       ActivitySource.StartActivity("Validate Order"))
                {
                    validation?.AddEvent(
                        new ActivityEvent("Validation Started"));

                    if (string.IsNullOrWhiteSpace(request.ProductName))
                    {
                        throw new ArgumentException("Product name is required");
                    }

                    validation?.AddEvent(
                        new ActivityEvent("Validation Completed"));
                }

                // Build Event
                using (ActivitySource.StartActivity("Create Order Event"))
                {
                    activity?.AddEvent(
                        new ActivityEvent("Creating Order Event"));
                }

                var orderEvent = new OrderCreatedEvent
                {
                    ProductName = request.ProductName,
                    Quantity = request.Quantity
                };

                // Publish RabbitMQ
                using (ActivitySource.StartActivity("Publish RabbitMQ"))
                {
                    activity?.AddEvent(
                        new ActivityEvent("Publishing Message"));

                    _producer.PublishOrderCreated(orderEvent);

                    activity?.AddEvent(
                        new ActivityEvent("Message Published"));
                }

                _logger.LogInformation(
                    "Order created for product {ProductName} with quantity {Quantity}",
                    request.ProductName,
                    request.Quantity);

                activity?.SetStatus(ActivityStatusCode.Ok);

                return Ok(new
                {
                    Message = "Order created and event published"
                });
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                activity?.SetStatus(ActivityStatusCode.Error);

                _logger.LogError(ex, "Failed to create order");

                return BadRequest(ex.Message);
            }
        }
    }
}