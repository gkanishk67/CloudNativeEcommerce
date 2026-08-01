using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProductService.Configurations;
using ProductService.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Serilog:SeqServerUrl"]!)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetSampler(new AlwaysOnSampler())

            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("ProductService"))

            .AddSource("ProductService")

            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
            })

            .AddHttpClientInstrumentation()

            .AddOtlpExporter(options =>
            {
                options.Endpoint =
                    new Uri("http://seq:5341/ingest/otlp/v1/traces");

                options.Protocol =
                    OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("ProductService"))

            .AddAspNetCoreInstrumentation()

            .AddHttpClientInstrumentation()

            .AddRuntimeInstrumentation()

            .AddPrometheusExporter();
    });

// Add services to the container.
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IProductDataService, ProductDataService>();

builder.Services.AddHostedService<OrderCreatedConsumer>();

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapPrometheusScrapingEndpoint();

app.Run();